using AccountAPI;
using AccountAPI.Entities;
using AccountAPI.Middleware;
using AccountAPI.Models.Validators;
using AccountAPI.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Threading.RateLimiting;
using System.Text;
using Microsoft.AspNetCore.RateLimiting;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

var authenctiationsettings = new AuthenticationSettings();

builder.Configuration.GetSection("Authentication").Bind(authenctiationsettings);

builder.Services.AddSingleton(authenctiationsettings);
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "Bearer";
    options.DefaultAuthenticateScheme = "Bearer";
    options.DefaultChallengeScheme = "Bearer";
}).AddJwtBearer(cfg =>
{
    cfg.RequireHttpsMetadata = false;
    cfg.SaveToken = true;
    cfg.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidIssuer = authenctiationsettings.JwtIssuer,
        ValidAudience = authenctiationsettings.JwtIssuer,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authenctiationsettings.JwtKey))
    };
});


builder.Services.AddTransient<IEmailSender, EmailSender>();


// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddHttpContextAccessor();

builder.Services.AddValidatorsFromAssemblyContaining<AddUserValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterUserValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<ChangePasswordValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<LoginChangePasswordValidator>();

builder.Services.AddDbContext<AccountDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SQLConnect")));

builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IPasswordHasher<Account>, PasswordHasher<Account>>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ErrorHandlingMiddleware>();

builder.Services.AddAutoMapper(typeof(AccountMappingProfile));

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddFixedWindowLimiter("loginLimiter", opt =>
    {
        opt.PermitLimit = 10;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 0;
    });

    options.AddFixedWindowLimiter("refreshTokenLimiter", opt =>
    {
        opt.PermitLimit = 15;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 0;
    });

    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.ContentType = "application/json";

        await context.HttpContext.Response.WriteAsync(
            """
        {
            "error": "Too many requests",
            "message": "Przekroczono limit prób logowania. Spróbuj ponownie za chwilê."
        }
        """,
            token);
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseRouting();

app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseRateLimiter();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }

