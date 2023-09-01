##### EF, AutoMapper, Logging with Serilog, RateLimting, Cors, Basic Authenticartion

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

using the dotnet cli with the force option

```
dotnet ef dbcontext scaffold "Server=localhost,1433;Database=schooldb;User Id=SA;Password=Bassguitar1;Encrypt=false;TrustServerCertificate=True;" Microsoft.EntityFrameworkCore.SqlServer -o Repos/Models --context LearnDataContext --context-dir Repos --data-annotations --force
```

After migration remove this line
![Alt text](<iScreen Shoter - Code - 230901145719.jpg>)

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

![Alt text](image.png)

---

##### Add basic Authentication

```sql

using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Techie.Repos;

namespace Techie.Helper
{
    public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly LearnDataContext _context;
        public BasicAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder,
            ISystemClock clock, LearnDataContext context) : base(options, logger, encoder, clock)
        {
            _context = context;
        }

        protected async override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
            {
                return AuthenticateResult.Fail("No header found");
            }

            var headervalue = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
            if (headervalue != null)
            {
                var bytes = Convert.FromBase64String(headervalue.Parameter);
                string credentials = Encoding.UTF8.GetString(bytes);
                string[] array = credentials.Split(":");
                string username = array[0];
                string password = array[0];
                var user = _context.Users.FirstOrDefault(x => x.Email == username && x.Password == password);
                if (user != null)
                {
                    var claims = new[] {
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Name, user.Email)
                    };
                    var identity = new ClaimsIdentity(claims, Scheme.Name);
                    var principal = new ClaimsPrincipal(identity);
                    var ticket = new AuthenticationTicket(principal, Scheme.Name);
                    return AuthenticateResult.Success(ticket);
                }

                return AuthenticateResult.Fail("Invalid username or password");

            }
            else
            {
                return AuthenticateResult.Fail("Invalid header");


            }
        }
    }
}
```

// In program.cs

```csharp
builder.Services.AddAuthentication("BasicAuthentication").AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);

app.UseCors("CorsPolicy");

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();

```

In the Controller

```csharp

namespace Techie.Container
{

    [Authorize]
    [EnableRateLimiting("fixed window")]
    // [EnableCors("CorsPolicy")]
    [Route("api/[controller]")]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _service;
        public CustomerController(ICustomerService service)
        {
            _service = service;
        }

        // [DisableCors]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _service.GetAll();
            if (data == null)
                return NotFound();
            return Ok(data);
        }

        [HttpPost]
        public async Task<IActionResult> AddCustomer([FromBody] CustomerModel customer)
        {
             await _service.AddCustomer(customer);
            return Ok("Added successfully");
        }
```

![Alt text](image-2.png)

To use the Basic Authentication
Use the base64e encoder : https://www.base64encode.org/
![Alt text](image-5.png)
Copy the encoded string and add it to the header
![Alt text](image-6.png)
![Alt text](image-7.png)

---

##### Remove the Basic Authentication

```csharp

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Techie.Modal;
using Techie.Service;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Techie.Container
{

    [Authorize]
    [EnableRateLimiting("fixed window")]
    // [EnableCors("CorsPolicy")]
    [Route("api/[controller]")]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _service;
        public CustomerController(ICustomerService service)
        {
            _service = service;
        }

        // [DisableCors]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _service.GetAll();
            if (data == null)
                return NotFound();
            return Ok(data);
        }

        [HttpPost]
        public async Task<IActionResult> AddCustomer([FromBody] CustomerModel customer)
        {
             await _service.AddCustomer(customer);
            return Ok("Added successfully");
        }

        [AllowAnonymous] // This will allow the method to be accessed without authentication
        [DisableRateLimiting]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await _service.GetById(id);
            if (data == null)
                return NotFound("No data found");
            return Ok(data);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveById(int id)
        {
            var data = await _service.RemoveById(id);
            if (data == null)
                return NotFound();
            return Ok(data);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromBody] CustomerModel customer, int id)
        {
            var data = await _service.Update(customer, id);
            if (data == null)
                return NotFound();
            return Ok(data);
        }

    }
}

```
