using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Pinkterest.Domain.Constants;
using Xunit;

namespace Pinkterest.UnitTests.Security;

public class AuthorizationPolicyTests
{
    private static readonly IAuthorizationService Authorization = BuildAuthorization();

    private static IAuthorizationService BuildAuthorization()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddAuthorizationCore(options =>
        {
            options.AddPolicy(Policies.IsAdministrator, policy =>
                policy.RequireRole(Roles.Administrator));
            options.AddPolicy(Policies.IsRegisteredUser, policy =>
                policy.RequireRole(Roles.RegisteredUser, Roles.Administrator));
        });

        return services.BuildServiceProvider().GetRequiredService<IAuthorizationService>();
    }

    private static ClaimsPrincipal User(params string[] roles) =>
        new(new ClaimsIdentity(roles.Select(role => new Claim(ClaimTypes.Role, role)), "test"));

    private static ClaimsPrincipal Anonymous() => new(new ClaimsIdentity());

    [Fact]
    public async Task An_administrator_satisfies_the_administrator_policy()
    {
        var result = await Authorization.AuthorizeAsync(
            User(Roles.Administrator), null, Policies.IsAdministrator);

        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task A_registered_user_is_denied_the_administrator_policy()
    {
        var result = await Authorization.AuthorizeAsync(
            User(Roles.RegisteredUser), null, Policies.IsAdministrator);

        result.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task A_registered_user_satisfies_the_registered_user_policy()
    {
        var result = await Authorization.AuthorizeAsync(
            User(Roles.RegisteredUser), null, Policies.IsRegisteredUser);

        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task An_administrator_also_satisfies_the_registered_user_policy()
    {
        var result = await Authorization.AuthorizeAsync(
            User(Roles.Administrator), null, Policies.IsRegisteredUser);

        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task An_anonymous_principal_satisfies_neither_policy()
    {
        var anonymous = Anonymous();

        (await Authorization.AuthorizeAsync(anonymous, null, Policies.IsAdministrator))
            .Succeeded.Should().BeFalse();
        (await Authorization.AuthorizeAsync(anonymous, null, Policies.IsRegisteredUser))
            .Succeeded.Should().BeFalse();
    }
}
