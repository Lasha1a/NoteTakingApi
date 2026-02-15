using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NoteTaking.Api.Common.models;
using NoteTaking.Api.Features.Auth;
using NoteTaking.Api.Features.Notes;
using NoteTaking.Api.Infrastructure.Data;
using Scalar.AspNetCore;
using System.Text;
using Serilog;
using NoteTaking.Api.Infrastructure.Middleware; // for global exception handling middleware



var builder = WebApplication.CreateBuilder(args);

//serilog configuration
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
      "logs/log-.txt",
      rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog(); //use serilog for logging

// connection string 
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// health checks for monitoring the health of the application and its dependencies
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>(
        name: "postgres-db",
        failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy
    );


// jwt authentication

var jwtSettings = builder.Configuration.GetSection("Jwt");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["Key"]!
                ))
        };
    });


//password hasher for hashing and verifying passwords
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();


builder.Services.AddAuthorization();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi("V1");

var app = builder.Build();



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseMiddleware<CorrelationIdMiddleware>(); //correlation id middleware for tracking requests across services

app.UseMiddleware<ExceptionMiddleware>(); //global exception handling middleware

//authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Health checks
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/db");

//map endpoints
RegisterUser.Map(app);
LoginUser.Map(app);

CreateNote.Map(app);
GetMyNotes.Map(app);
UpdateNote.Map(app);
DeleteNote.Map(app);
GetNoteById.Map(app);

FilterNotesByTags.Map(app);

app.Run();

