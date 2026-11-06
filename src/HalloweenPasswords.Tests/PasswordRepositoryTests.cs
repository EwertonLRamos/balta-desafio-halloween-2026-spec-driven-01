using HalloweenPasswords.Api.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace HalloweenPasswords.Tests;

public class PasswordRepositoryTests
{
    [Fact]
    public async Task AddAsync_persists_password_and_GetValueByIdAsync_returns_exact_value()
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var dbContext = CreateDbContext(connection);
        var repository = new EfPasswordRepository(dbContext);
        var id = Guid.NewGuid();
        const string value = "Exact password!42";

        await repository.AddAsync(id, value);

        Assert.Equal(value, await repository.GetValueByIdAsync(id));
    }

    [Fact]
    public async Task GetValueByIdAsync_returns_null_when_id_is_not_associated()
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var dbContext = CreateDbContext(connection);
        var repository = new EfPasswordRepository(dbContext);

        var value = await repository.GetValueByIdAsync(Guid.NewGuid());

        Assert.Null(value);
    }

    [Fact]
    public async Task AddAsync_does_not_replace_password_for_existing_id()
    {
        await using var connection = await CreateOpenConnectionAsync();
        var id = Guid.NewGuid();

        await using (var initialContext = CreateDbContext(connection))
        {
            await new EfPasswordRepository(initialContext).AddAsync(id, "Original password!");
        }

        await using (var duplicateContext = CreateDbContext(connection))
        {
            var duplicateRepository = new EfPasswordRepository(duplicateContext);

            await Assert.ThrowsAsync<DbUpdateException>(
                () => duplicateRepository.AddAsync(id, "Replacement password!"));
        }

        await using var verificationContext = CreateDbContext(connection);
        var verificationRepository = new EfPasswordRepository(verificationContext);

        Assert.Equal("Original password!", await verificationRepository.GetValueByIdAsync(id));
    }

    private static async Task<SqliteConnection> CreateOpenConnectionAsync()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        await using var dbContext = CreateDbContext(connection);
        await dbContext.Database.EnsureCreatedAsync();

        return connection;
    }

    private static PasswordsDbContext CreateDbContext(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<PasswordsDbContext>()
            .UseSqlite(connection)
            .Options;

        return new PasswordsDbContext(options);
    }
}
