### Harald Hammershaug
- Studentnummer: S374171
- E‑post: S374171@oslomet.no

# Student Management Web API (Assignment #3)

Liten REST Web API som håndterer studenter i en lokal SQL‑database. Returnerer JSON over HTTP.  
Inneholder CRUD + to endepunkter: beregne karakterer og kursvise rapporter.

## Teknologi Brukt (Stack)
- C# + ASP.NET Core (.NET 10)
- SQL Server (lokal) + SSMS
- JSON over HTTP (REST)
- Scalar (OpenAPI UI) / Postman

## Forutsetninger
- .NET 10 SDK (`dotnet --version` → 10.x)
- SQL Server Developer + SSMS installert
- VS Code + “C# Dev Kit”

## Kom i gang (med starter-repo)

### klon repoet
git clone https://github.com/kashifsanadar/data2410-api-v1.git
cd data2410-api-v1
code .

#### 1) Opprett database
Åpne SSMS → koble til localhost (eller localhost\SQLEXPRESS)
Databases → New Database… → navn: StudentsDb

#### 2) Sett connection string (appsettings.json)
Windows Authentication (anbefalt for lokal utvikling):

```bash
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=StudentsDb;Trusted_Connection=True;TrustServerCertificate=True;"
}
```
