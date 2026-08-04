namespace Pinkterest.Application.Accounts;

public sealed record RegisterRequest(string DisplayName, string Email, string Password, Guid PackageId);
