namespace Pinkterest.Application.Accounts;

public sealed record LoginRequest(string Email, string Password, bool RememberMe);
