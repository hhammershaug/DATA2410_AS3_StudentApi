### Harald Hammershaug
- Studentnummer: S374171
- E-post: S374171@oslomet.no

# Student Management Web API (Assignment #3)

Small REST Web API that manages students in a SQL database. Returns JSON over HTTP.  
Implements CRUD plus two endpoints: calculate grades and course-wise report.

## Stack
- C# + ASP.NET Core (.NET 8)
- SQL Server (local)
- JSON over HTTP (REST)
- Scalar (OpenAPI UI) or Postman for testing

## Prerequisites
- .NET SDK installed (`dotnet --version`)
- SQL Server + SSMS (Developer edition)
- VS Code + “C# Dev Kit” extension (recommended)

## Setup (from scratch)
```bash
# create solution + webapi project
mkdir StudentApi && cd StudentApi
dotnet new sln -n StudentApi
dotnet new webapi -n StudentApi -f net8.0
dotnet sln add StudentApi/StudentApi.csproj

# add EF Core for SQL Server
cd StudentApi
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools

# trust dev HTTPS cert (first time only)
dotnet dev-certs https --trust
