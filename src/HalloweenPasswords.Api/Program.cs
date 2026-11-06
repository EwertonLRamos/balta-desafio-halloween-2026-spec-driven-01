using HalloweenPasswords.Api.Application.Services;
using HalloweenPasswords.Api.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("Passwords")
    ?? throw new InvalidOperationException("The Passwords connection string is not configured.");

builder.Services.AddDbContext<PasswordsDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddSingleton<IPasswordGenerator, PasswordGenerator>();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
