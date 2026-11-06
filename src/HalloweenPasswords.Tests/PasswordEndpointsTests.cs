using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using HalloweenPasswords.Api.Application.Abstractions;
using HalloweenPasswords.Api.Http;
using HalloweenPasswords.Api.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HalloweenPasswords.Tests;

public class PasswordEndpointsTests
{
    [Fact]
    public async Task Post_generates_and_returns_id_and_password_then_get_returns_the_same_password()
    {
        using var factory = new PasswordApiFactory();
        using var client = factory.CreateClient();
        await factory.AssertMigrationAppliedAsync();

        var generateResponse = await client.PostAsync("/api/passwords", content: null);

        Assert.Equal(HttpStatusCode.OK, generateResponse.StatusCode);
        var generated = await generateResponse.Content
            .ReadFromJsonAsync<PasswordEndpoints.GeneratedPasswordResponse>();
        Assert.NotNull(generated);
        Assert.Equal(generated.Id.ToLowerInvariant(), generated.Id);
        Assert.True(Guid.TryParseExact(generated.Id, "D", out _));
        Assert.Equal(16, generated.Password.Length);

        using var generatedJson = JsonDocument.Parse(
            await generateResponse.Content.ReadAsStringAsync());
        Assert.Equal(
            ["id", "password"],
            generatedJson.RootElement.EnumerateObject().Select(property => property.Name));

        var getResponse = await client.GetAsync($"/api/passwords/{generated.Id}");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var retrieved = await getResponse.Content.ReadFromJsonAsync<PasswordEndpoints.PasswordResponse>();
        Assert.Equal(generated.Password, retrieved?.Password);

        var uppercaseIdResponse = await client.GetAsync(
            $"/api/passwords/{generated.Id.ToUpperInvariant()}");

        Assert.Equal(HttpStatusCode.OK, uppercaseIdResponse.StatusCode);
        var retrievedWithUppercaseId = await uppercaseIdResponse.Content
            .ReadFromJsonAsync<PasswordEndpoints.PasswordResponse>();
        Assert.Equal(generated.Password, retrievedWithUppercaseId?.Password);
    }

    [Fact]
    public async Task Get_returns_problem_details_for_malformed_guid()
    {
        using var factory = new PasswordApiFactory();
        using var client = factory.CreateClient();
        await factory.AssertMigrationAppliedAsync();

        var response = await client.GetAsync("/api/passwords/not-a-guid");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        using var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(400, problem.RootElement.GetProperty("status").GetInt32());
        Assert.False(string.IsNullOrWhiteSpace(problem.RootElement.GetProperty("title").GetString()));
    }

    [Fact]
    public async Task Get_returns_not_found_for_valid_guid_without_association()
    {
        using var factory = new PasswordApiFactory();
        using var client = factory.CreateClient();
        await factory.AssertMigrationAppliedAsync();

        var response = await client.GetAsync($"/api/passwords/{Guid.NewGuid():D}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(0, response.Content.Headers.ContentLength);
    }

    [Fact]
    public async Task Post_returns_generic_problem_details_when_persistence_fails()
    {
        using var factory = new PasswordApiFactory(failWrites: true);
        using var client = factory.CreateClient();
        await factory.AssertMigrationAppliedAsync();

        var response = await client.PostAsync("/api/passwords", content: null);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var problemJson = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("simulated storage failure", problemJson);
        using var problem = JsonDocument.Parse(problemJson);
        Assert.Equal(500, problem.RootElement.GetProperty("status").GetInt32());
        Assert.Equal(
            "An unexpected error occurred.",
            problem.RootElement.GetProperty("title").GetString());
    }

    private sealed class PasswordApiFactory(bool failWrites = false) : WebApplicationFactory<Program>
    {
        private readonly SqliteConnection _connection = OpenConnection();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<PasswordsDbContext>>();
                services.RemoveAll<PasswordsDbContext>();
                services.AddDbContext<PasswordsDbContext>(options => options.UseSqlite(_connection));

                if (failWrites)
                {
                    services.RemoveAll<IPasswordRepository>();
                    services.AddScoped<IPasswordRepository, FailingPasswordRepository>();
                }
            });
        }

        public async Task AssertMigrationAppliedAsync()
        {
            using var scope = Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<PasswordsDbContext>();
            var appliedMigrations = await dbContext.Database.GetAppliedMigrationsAsync();

            Assert.Contains("20261106190432_InitialCreate", appliedMigrations);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                _connection.Dispose();
            }
        }

        private static SqliteConnection OpenConnection()
        {
            var connection = new SqliteConnection(
                $"Data Source=PasswordApiTests_{Guid.NewGuid():N};Mode=Memory;Cache=Shared");
            connection.Open();
            return connection;
        }
    }

    private sealed class FailingPasswordRepository : IPasswordRepository
    {
        public Task AddAsync(Guid id, string value, CancellationToken cancellationToken = default)
        {
            return Task.FromException(new InvalidOperationException("simulated storage failure"));
        }

        public Task<string?> GetValueByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<string?>(null);
        }
    }
}
