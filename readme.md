##### EF, AutoMapper, Logging with Serilog, RateLimting, Cors

##### Entity framework
- Database First approach
- Code First approach
- Model First Approach

force drop db
```sql
USE master;
ALTER DATABASE schooldb SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
DROP DATABASE schooldb;

```

Ef Scaffold command
```
Scaffold-DbContext "Server=.;Database=schooldb;Trusted_Connection=True;" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models
```

Ef Scaffold command with the force option
```
Scaffold-DbContext "Server=.;Database=schooldb;Trusted_Connection=True;" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models -Force
```

using the dotnet cli
```
dotnet ef dbcontext scaffold "Server=localhost,1433;Database=schooldb;User Id=SA;Password=Bassguitar1;Encrypt=false;TrustServerCertificate=True;" Microsoft.EntityFrameworkCore.SqlServer -o Repos/Models --context LearnDataContext --context-dir Repos --data-annotations
```

##### Install Ef
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet add package Microsoft.EntityFrameworkCore.Design

To Scafold, you need to create the table first in the database
```sql
CREATE Database schooldb;

USE SchoolDB;
-- Customer Table
-- Code , Name, Email, PhoneNumber, Creditlimit, IsActive, CreatedDate, UpdatedDate, TaxCode
CREATE TABLE Customer(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Code VARCHAR(50) NOT NULL,
    Name VARCHAR(50) NOT NULL,
    Email VARCHAR(50) NOT NULL,
    PhoneNumber VARCHAR(50) NOT NULL,
    Creditlimit DECIMAL(18,2) NOT NULL,
    IsActive BIT NOT NULL,
    CreatedDate DATETIME NOT NULL,
    UpdatedDate DATETIME NOT NULL,
    TaxCode VARCHAR(50) NOT NULL
);

-- User Table
-- Code , Name, Email, PhoneNumber, IsActive, CreatedDate, UpdatedDate, Password
CREATE TABLE [User](
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Code VARCHAR(50) NOT NULL,
    Name VARCHAR(50) NOT NULL,
    Email VARCHAR(50) NOT NULL,
    PhoneNumber VARCHAR(50) NOT NULL,
    IsActive BIT NOT NULL,
    CreatedDate DATETIME NOT NULL,
    UpdatedDate DATETIME NOT NULL,
    Password VARCHAR(50) NOT NULL
);
```


---

##### Logging
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.File