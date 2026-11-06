using HalloweenPasswords.Api.Application.Services;

namespace HalloweenPasswords.Tests;

public class PasswordGeneratorTests
{
    private readonly PasswordGenerator _generator = new();

    [Fact]
    public void Generate_returns_only_printable_ascii_characters()
    {
        var password = _generator.Generate();

        Assert.All(password, character => Assert.InRange(character, '!', '~'));
    }

    [Fact]
    public void Generate_returns_exactly_sixteen_characters()
    {
        var password = _generator.Generate();

        Assert.Equal(16, password.Length);
    }

    [Fact]
    public void Generate_includes_at_least_one_special_character()
    {
        var password = _generator.Generate();

        Assert.Contains(password, character => !char.IsAsciiLetterOrDigit(character));
    }
}
