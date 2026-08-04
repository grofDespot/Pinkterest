namespace Pinkterest.Application.Common.Interfaces;

public interface ICurrentUser
{
    Guid? UserId { get; }

    string? UserName { get; }

    string? IpAddress { get; }

    bool IsAuthenticated { get; }

    bool IsInRole(string role);
}
