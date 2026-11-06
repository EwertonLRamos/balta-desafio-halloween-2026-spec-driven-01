using HalloweenPasswords.Api.Application.Services;

namespace HalloweenPasswords.Api.Http;

public static class PasswordEndpoints
{
    public static IEndpointRouteBuilder MapPasswordEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var passwords = endpoints.MapGroup("/api/passwords");

        passwords.MapPost("", GenerateAsync)
            .Produces<GeneratedPasswordResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        passwords.MapGet("/{id}", GetByIdAsync)
            .Produces<PasswordResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        return endpoints;
    }

    private static async Task<IResult> GenerateAsync(
        IPasswordService passwordService,
        CancellationToken cancellationToken)
    {
        var generatedPassword = await passwordService.GenerateAsync(cancellationToken);

        return TypedResults.Ok(new GeneratedPasswordResponse(
            generatedPassword.Id.ToString("D"),
            generatedPassword.Value));
    }

    private static async Task<IResult> GetByIdAsync(
        string id,
        IPasswordService passwordService,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParseExact(id, "D", out var passwordId)
            || !passwordId.ToString("D").Equals(id, StringComparison.OrdinalIgnoreCase))
        {
            return TypedResults.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "The identifier must be a GUID in D format.");
        }

        var password = await passwordService.GetByIdAsync(passwordId, cancellationToken);

        return password is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(new PasswordResponse(password));
    }

    public sealed record GeneratedPasswordResponse(string Id, string Password);

    public sealed record PasswordResponse(string Password);
}
