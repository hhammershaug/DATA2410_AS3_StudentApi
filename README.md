### Harald Hammershaug
- Studentnummer: S374171
- E‑post: S374171@oslomet.no

# Student Management Web API (Assignment #3)

Liten REST Web API som bruker SQL Server. CRUD + to endepunkter: beregne karakterer og kursvis rapport.

## Krav
- .NET 10 SDK
- SQL Server + SSMS

## Oppsett
1) Opprett database i SSMS  
   Databases → New Database… → `StudentsDb`
2) appsettings.json (Windows‑auth):
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=StudentsDb;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

## Kjør API

```powershell
dotnet restore .\data2410-api-v1.csproj
dotnet run --project .\data2410-api-v1.csproj
```

- Base URL: se konsollen (f.eks. https://localhost:7010)
- Docs (Scalar): https://localhost:7010/scalar/v1
- Helse: https://localhost:7010/health
- Stopp: Ctrl+C

## Test (Scalar)
1. Åpne: https://localhost:7010/scalar/v1
2. Seed 2–3 studenter via POST `/api/Students`
   - Headers: `Content-Type: application/json`
   - Body (eksempel):
     ```json
     { "name": "Jane Smith", "course": "DATA2410", "marks": 92 }
     ```
   - Flere eksempler:
     ```json
     { "name": "John Doe", "course": "DATA2410", "marks": 85 }
     { "name": "Alex Lee", "course": "DATA2410", "marks": 73 }
     ```
   - Verifiser med GET `/api/Students` (liste alle)
3. Kjør POST `/api/Students/calculate-grades` (uten body)
   - Forventer at hver student får `"grade"` satt til A/B/C/D
4. Kjør GET `/api/Students/report`
   - Forventer kursvis: `totalStudents`, `averageMarks` (2 desimaler), og `gradeDistribution`


## Feilsøking
- HTTPS: `dotnet dev-certs https --trust` (start API på nytt)
- DB: sjekk `DefaultConnection` og at `StudentsDb` finnes i SSMS
- 415/400: sett `Content-Type: application/json` og send gyldig JSON (marks som tall)

```powershell
# Hvis nettleser/Scalar fortsatt klager på sertifikat:
dotnet dev-certs https --clean
dotnet dev-certs https --trust
```

