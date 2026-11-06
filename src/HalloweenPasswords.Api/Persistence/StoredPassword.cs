namespace HalloweenPasswords.Api.Persistence;

public sealed class StoredPassword
{
    public Guid Id { get; set; }

    public required string Value { get; set; }
}
