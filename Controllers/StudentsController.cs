using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using data2410_api_v1.Models;

namespace data2410_api_v1.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StudentsController(IConfiguration config) : ControllerBase
{
    private readonly string _connectionString = config.GetConnectionString("DefaultConnection")!;

    private static string GetGrade(int marks) => marks switch
    {
        >= 90 => "A",
        >= 80 => "B",
        >= 60 => "C",
        _ => "D"
    };

    [HttpGet]
    public async Task<ActionResult<List<Student>>> GetAll()
    {
        var students = new List<Student>();
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        using var cmd = new SqlCommand("SELECT Id, Name, Course, Marks, Grade FROM Students", conn);
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            students.Add(new Student
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Course = reader.GetString(2),
                Marks = reader.GetInt32(3),
                Grade = reader.IsDBNull(4) ? null : reader.GetString(4)
            });
        }

        return students;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Student>> GetById(int id)
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        using var cmd = new SqlCommand("SELECT Id, Name, Course, Marks, Grade FROM Students WHERE Id = @Id", conn);
        cmd.Parameters.AddWithValue("@Id", id);

        using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return NotFound();

        return new Student
        {
            Id = reader.GetInt32(0),
            Name = reader.GetString(1),
            Course = reader.GetString(2),
            Marks = reader.GetInt32(3),
            Grade = reader.IsDBNull(4) ? null : reader.GetString(4)
        };
    }

    [HttpPost]
    public async Task<ActionResult<Student>> Create(Student student)
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        using var cmd = new SqlCommand(
            "INSERT INTO Students (Name, Course, Marks) OUTPUT INSERTED.Id VALUES (@Name, @Course, @Marks)", conn);
        cmd.Parameters.AddWithValue("@Name", student.Name);
        cmd.Parameters.AddWithValue("@Course", student.Course);
        cmd.Parameters.AddWithValue("@Marks", student.Marks);

        student.Id = (int)await cmd.ExecuteScalarAsync();
        return CreatedAtAction(nameof(GetById), new { id = student.Id }, student);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Student updated)
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        using var cmd = new SqlCommand(
            "UPDATE Students SET Name = @Name, Course = @Course, Marks = @Marks WHERE Id = @Id", conn);
        cmd.Parameters.AddWithValue("@Id", id);
        cmd.Parameters.AddWithValue("@Name", updated.Name);
        cmd.Parameters.AddWithValue("@Course", updated.Course);
        cmd.Parameters.AddWithValue("@Marks", updated.Marks);

        var rows = await cmd.ExecuteNonQueryAsync();
        return rows == 0 ? NotFound() : NoContent();
    }

    // -------- 1 HTTP POST ENDPOINT: /api/Students/calculate-grades --------
    // Read all students, set grade (A/B/C/D) from Marks, save, return list
    [HttpPost("calculate-grades")]
    public async Task<ActionResult<List<Student>>> CalculateGrades()
    {
        // list we return
        var updated = new List<Student>();

        // open DB connection
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        // read STUDENTS(Id, Marks)
        var updates = new List<(int Id, string Grade)>();
        using (var cmd = new SqlCommand("SELECT Id, Marks FROM Students", conn))
        using (var r = await cmd.ExecuteReaderAsync())
        {
            while (await r.ReadAsync())
            {
                int id = r.GetInt32(0);
                int marks = r.GetInt32(1);
                // calculate GRADE using GetGrade(marks):
                string grade = GetGrade(marks); // map marks → A/B/C/D
                updates.Add((id, grade));
            }
        }

        // save grades for all students in one go
        using (var tx = conn.BeginTransaction())
        {
            foreach (var u in updates)
            {
                using var up = new SqlCommand(
                    "UPDATE Students SET Grade = @g WHERE Id = @id", conn, tx);
                up.Parameters.AddWithValue("@g", (object?)u.Grade ?? DBNull.Value);
                up.Parameters.AddWithValue("@id", u.Id);
                await up.ExecuteNonQueryAsync();
            }
            tx.Commit(); // finish all updates
        }

        // read full list with updated grades
        using (var listCmd = new SqlCommand(
            "SELECT Id, Name, Course, Marks, Grade FROM Students ORDER BY Id", conn))
        using (var r2 = await listCmd.ExecuteReaderAsync())
        {
            while (await r2.ReadAsync())
            {
                updated.Add(new Student
                {
                    Id = r2.GetInt32(0),
                    Name = r2.GetString(1),
                    Course = r2.GetString(2),
                    Marks = r2.GetInt32(3),
                    Grade = r2.IsDBNull(4) ? null : r2.GetString(4)
                });
            }
        }

        // return student grades
        return Ok(updated);
    }
    // ---------------- END POST calculate-grades ----------------
    

    // -------- 2 HTTP GET ENDPOINT: /api/Students/report --------
    // Return per-course totals, average marks (2 decimals), and grade counts
    [HttpGet("report")]
    public async Task<IActionResult> Report()
    {
        var result = new List<object>();

        // open DB connection
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        // group by Course and compute stats
        const string sql = @"
            SELECT
            Course AS courseName,
            COUNT(*) AS totalStudents,
            ROUND(AVG(CAST(Marks AS float)), 2) AS averageMarks,
            SUM(CASE WHEN Marks >= 90 THEN 1 ELSE 0 END) AS A,
            SUM(CASE WHEN Marks >= 80 AND Marks < 90 THEN 1 ELSE 0 END) AS B,
            SUM(CASE WHEN Marks >= 60 AND Marks < 80 THEN 1 ELSE 0 END) AS C,
            SUM(CASE WHEN Marks < 60 THEN 1 ELSE 0 END) AS D
            FROM Students
            GROUP BY Course
            ORDER BY Course;";

        using var cmd = new SqlCommand(sql, conn);
        using var r = await cmd.ExecuteReaderAsync();
        while (await r.ReadAsync())
        {
            string courseName = r.GetString(0);
            int total = r.GetInt32(1);
            double average = Convert.ToDouble(r.GetValue(2)); // read numeric as double
            int a = r.GetInt32(3);
            int b = r.GetInt32(4);
            int c = r.GetInt32(5);
            int d = r.GetInt32(6);

            result.Add(new
            {
                courseName,
                totalStudents = total,
                averageMarks = average,
                gradeDistribution = new { A = a, B = b, C = c, D = d }
            });
        }

        // return student report
        return Ok(result);
    }
    // ---------------- END GET report ----------------


    // -------- HTTP DELETE: /api/Students/{id} --------
    // Delete one student by id
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        using var cmd = new SqlCommand("DELETE FROM Students WHERE Id = @Id", conn);
        cmd.Parameters.AddWithValue("@Id", id);

        var rows = await cmd.ExecuteNonQueryAsync();
        return rows == 0 ? NotFound() : NoContent(); // 404 if not found, 204 on success
    }
    // ---------------- END DELETE ----------------
    

}
// END StudentsController
