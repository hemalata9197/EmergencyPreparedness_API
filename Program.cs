using EmergencyManagement.Data;
using EmergencyManagement.Middleware;
using EmergencyManagement.Services.Implementations;
using EmergencyManagement.Services.Interfaces;
using EmergencyManagement.Token;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Security.Claims;
using System.Text;
using Npgsql;
using Npgsql.Json.NET;
using EmergencyManagement.Models.DTOs;
using Microsoft.Extensions.FileProviders;
using System.IdentityModel.Tokens.Jwt;

var builder = WebApplication.CreateBuilder(args);

// Register DbContext
builder.Services.AddDbContext<EHSDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("EMSConnection")));
//builder.Services.AddDbContext<EHSDbContext>(options =>
//{
//    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
//        npgsqlOptions => npgsqlOptions.UseNodaTime());

//    // Enable dynamic JSON handling
//    AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
//});
builder.Services.AddScoped<EmailTemplateService>();
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
// Register Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ISubmitReportService, SubmitReportService>();
builder.Services.AddScoped<IMastersService, MastersService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IReportsService, ReportsService>();


// JWT Config
var jwtSettingsSection = builder.Configuration.GetSection("JwtSettings");
builder.Services.Configure<JwtSettings>(jwtSettingsSection);
var jwtSettings = jwtSettingsSection.Get<JwtSettings>();
var key = Encoding.UTF8.GetBytes(jwtSettings.Secret);
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "EMS", // Must match "iss"

            ValidateAudience = true,
            ValidAudience = "EMSUsers", // Must match "aud"

            ValidateIssuerSigningKey = false,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),

            NameClaimType = ClaimTypes.Name,
            RoleClaimType = ClaimTypes.Role
        };
    });

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
        policy
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

// Controllers, Swagger, Auth
builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Emergency Management API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Enter 'Bearer' followed by your token",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});

var app = builder.Build();
NpgsqlConnection.GlobalTypeMapper.UseJsonNet();

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
app.UseStaticFiles();

var attachmentsPath = Path.Combine(Directory.GetCurrentDirectory(), "Attachments");
if (!Directory.Exists(attachmentsPath))
{
    Directory.CreateDirectory(attachmentsPath);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(attachmentsPath),
    RequestPath = "/Attachments"
});


app.UseSwagger();
app.UseSwaggerUI();

//app.UseHttpsRedirection();

app.UseCors("AllowAngularApp");

app.UseAuthentication(); // 👈 must come before Authorization
app.UseAuthorization();

app.UseMiddleware<PermissionMiddleware>(); // if used

app.MapControllers();

app.Run();
