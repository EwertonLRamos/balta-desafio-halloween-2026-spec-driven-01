using HalloweenPasswords.Api.Application.Abstractions;
using HalloweenPasswords.Api.Application.Services;

namespace HalloweenPasswords.Tests;

public class PasswordServiceTests
{
    [Fact]
    public async Task GenerateAsync_stores_generated_value_and_returns_its_id()
    {
        const string generatedValue = "Generated password!42";
        var generator = new StubPasswordGenerator(generatedValue);
        var repository = new InMemoryPasswordRepository();
        var service = new PasswordService(generator, repository);

        var result = await service.GenerateAsync();

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(generatedValue, result.Value);
        Assert.Equal(generatedValue, await repository.GetValueByIdAsync(result.Id));
    }

    [Fact]
    public async Task GenerateAsync_creates_distinct_associations_even_when_values_match()
    {
        const string generatedValue = "Same generated password!42";
        var generator = new StubPasswordGenerator(generatedValue);
        var repository = new InMemoryPasswordRepository();
        var service = new PasswordService(generator, repository);

        var first = await service.GenerateAsync();
        var second = await service.GenerateAsync();

        Assert.NotEqual(first.Id, second.Id);
        Assert.Equal(generatedValue, await repository.GetValueByIdAsync(first.Id));
        Assert.Equal(generatedValue, await repository.GetValueByIdAsync(second.Id));
        Assert.Equal(2, repository.Count);
    }

    [Fact]
    public async Task GetByIdAsync_returns_exact_associated_value()
    {
        var repository = new InMemoryPasswordRepository();
        var id = Guid.NewGuid();
        const string value = "Associated password!42";
        await repository.AddAsync(id, value);
        var service = new PasswordService(new StubPasswordGenerator("Unused password!42"), repository);

        var result = await service.GetByIdAsync(id);

        Assert.Equal(value, result);
    }

    [Fact]
    public async Task GetByIdAsync_returns_null_when_id_is_not_associated()
    {
        var repository = new InMemoryPasswordRepository();
        var service = new PasswordService(new StubPasswordGenerator("Unused password!42"), repository);

        var result = await service.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task GenerateAsync_does_not_return_success_when_storage_fails()
    {
        var repository = new FailingPasswordRepository();
        var service = new PasswordService(
            new StubPasswordGenerator("Unstored password!42"),
            repository);

        await Assert.ThrowsAsync<IOException>(() => service.GenerateAsync());
    }

    private sealed class StubPasswordGenerator(string value) : IPasswordGenerator
    {
        public string Generate() => value;
    }

    private sealed class InMemoryPasswordRepository : IPasswordRepository
    {
        private readonly Dictionary<Guid, string> _passwords = [];

        public int Count => _passwords.Count;

        public Task AddAsync(Guid id, string value, CancellationToken cancellationToken = default)
        {
            if (!_passwords.TryAdd(id, value))
            {
                throw new InvalidOperationException("The identifier is already associated.");
            }

            return Task.CompletedTask;
        }

        public Task<string?> GetValueByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _passwords.TryGetValue(id, out var value);
            return Task.FromResult(value);
        }
    }

    private sealed class FailingPasswordRepository : IPasswordRepository
    {
        public Task AddAsync(Guid id, string value, CancellationToken cancellationToken = default)
        {
            return Task.FromException(new IOException("Storage failed."));
        }

        public Task<string?> GetValueByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<string?>(null);
        }
    }
}
