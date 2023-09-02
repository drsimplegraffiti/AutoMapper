using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;
using Techie.Container;
using Techie.Helper;
using Techie.Modal;
using Techie.Repos;
using Techie.Repos.Models;
using Techie.Service;

var builder = WebApplication.CreateBuilder(args);
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Logging.AddSerilog();


// Add services to the container.
//                            Interface          Implementation
builder.Services.AddTransient<ICustomerService, CustomerService>();
builder.Services.AddScoped<IRefreshHandler, RefreshHandler>();
builder.Services.AddDbContext<LearnDataContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("apicon")));

// add basic authentication
// builder.Services.AddAuthentication("BasicAuthentication").AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);

// add jwt authentication
// var _authkey = Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"] ?? throw new InvalidOperationException());
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = true;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuerSigningKey = true,
        ValidateIssuer = false,
        ValidateAudience = false,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecurityKey"] ?? throw new InvalidOperationException())),
        ClockSkew = TimeSpan.Zero
    };
});


// auto mapper
var automapper = new MapperConfiguration(item => item.AddProfile(new AutoMapperHandler()));
IMapper mapper = automapper.CreateMapper();
builder.Services.AddSingleton(mapper);
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.AllowAnyHeader().AllowAnyMethod().WithOrigins("*");
    });
});

// add rate limiting
builder.Services.AddRateLimiter(_ => _.AddFixedWindowLimiter(policyName: "fixed window", options =>
{
    options.Window = TimeSpan.FromSeconds(10);
    options.QueueLimit = 0;
    options.PermitLimit = 3;
    options.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
}).RejectionStatusCode = StatusCodes.Status429TooManyRequests);

var _jwtsetting = builder.Configuration.GetSection("JwtSettings");
builder.Services.Configure<JwtSettings>(_jwtsetting);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// minimal api
app.MapGet("/", () => "Hello World!");
//http://localhost:5197/getchannel?channelname=bill
app.MapGet("/getchannel", (string channelname) => $"Welcome to {channelname} channel")
    .WithOpenApi(opt =>
    {   
        var param = opt.Parameters[0];
        param.Description = "Name of the channel";
        return opt;
    });

app.MapGet("/getcustomer", async(LearnDataContext _context) => {
    var data = await _context.Customers.ToListAsync();
    return Results.Ok(data);
});

// get customer by id
app.MapGet("/getcustomer/{id}", async(LearnDataContext _context, int id) => {
    var data = await _context.Customers.FirstOrDefaultAsync(item => item.Id == id);
    if (data == null)
        return Results.NotFound("No data found");
    return Results.Ok(data);
});

// add customer
app.MapPost("/addcustomer", async(LearnDataContext _context, Customer customer) => {
    await _context.Customers.AddAsync(customer);
    await _context.SaveChangesAsync();
    return Results.Ok("Added successfully");
});

// update customer
app.MapPut("/updatecustomer/{id}", async(LearnDataContext _context, Customer customer, int id) => {
    var data = await _context.Customers.FirstOrDefaultAsync(item => item.Id == id);
    if (data == null)
        return Results.NotFound("No data found");
    data.Name = customer.Name;
    data.Email = customer.Email;
    data.UpdatedDate = customer.UpdatedDate;
    data.Creditlimit = customer.Creditlimit;
    data.Code = customer.Code;
    data.PhoneNumber = customer.PhoneNumber;
    data.TaxCode = customer.TaxCode;
    await _context.SaveChangesAsync();
    return Results.Ok("Updated successfully");
});

// delete customer
app.MapDelete("/deletecustomer/{id}", async(LearnDataContext _context, int id) => {
    var data = await _context.Customers.FirstOrDefaultAsync(item => item.Id == id);
    if (data == null)
        return Results.NotFound("No data found");
    _context.Customers.Remove(data);
    await _context.SaveChangesAsync();
    return Results.Ok("Deleted successfully");
});

app.UseRateLimiter();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseStaticFiles();
app.UseCors("CorsPolicy");

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();

