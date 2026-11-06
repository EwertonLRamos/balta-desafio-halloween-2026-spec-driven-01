using System.Security.Cryptography;

namespace HalloweenPasswords.Api.Application.Services;

public sealed class PasswordGenerator : IPasswordGenerator
{
    private const int PasswordLength = 16;

    private static readonly char[] AllowedCharacters = Enumerable
        .Range('!', '~' - '!' + 1)
        .Select(value => (char)value)
        .ToArray();

    private static readonly char[] SpecialCharacters = AllowedCharacters
        .Where(character => !char.IsAsciiLetterOrDigit(character))
        .ToArray();

    public string Generate()
    {
        var characters = new char[PasswordLength];
        var containsSpecialCharacter = false;

        for (var index = 0; index < characters.Length; index++)
        {
            var character = AllowedCharacters[RandomNumberGenerator.GetInt32(AllowedCharacters.Length)];
            characters[index] = character;
            containsSpecialCharacter |= !char.IsAsciiLetterOrDigit(character);
        }

        if (!containsSpecialCharacter)
        {
            var position = RandomNumberGenerator.GetInt32(characters.Length);
            var specialCharacter = SpecialCharacters[RandomNumberGenerator.GetInt32(SpecialCharacters.Length)];
            characters[position] = specialCharacter;
        }

        return new string(characters);
    }
}
