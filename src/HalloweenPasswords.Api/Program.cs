using HalloweenPasswords.Api.Application.Abstractions;
using HalloweenPasswords.Api.Application.Services;
using HalloweenPasswords.Api.Http;
using HalloweenPasswords.Api.Persistence;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("Passwords")
    ?? throw new InvalidOperationException("The Passwords connection string is not configured.");

builder.Services.AddDbContext<PasswordsDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IPasswordRepository, EfPasswordRepository>();
builder.Services.AddSingleton<IPasswordGenerator, PasswordGenerator>();
builder.Services.AddScoped<IPasswordService, PasswordService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler(exceptionHandler =>
{
    exceptionHandler.Run(async context =>
    {
        var logger = context.RequestServices.GetRequiredService<ILoggerFactory>()
            .CreateLogger("UnhandledException");
        logger.LogError(
            context.Features.Get<IExceptionHandlerFeature>()?.Error,
            "An unexpected error occurred while processing the request.");

        context.Response.Clear();
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await Results.Problem(
            statusCode: StatusCodes.Status500InternalServerError,
            title: "An unexpected error occurred.")
            .ExecuteAsync(context);
    });
});

app.MapPasswordEndpoints();

await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<PasswordsDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.Run();

public partial class Program
{
}
