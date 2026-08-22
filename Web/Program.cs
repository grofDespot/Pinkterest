using Microsoft.AspNetCore.Mvc;
using Pinkterest.Application.Common.Interfaces;
using Pinkterest.Domain.Constants;
using Pinkterest.Infrastructure;
using Pinkterest.Infrastructure.Persistence.Seeding;
using Pinkterest.Web.Observability;
using Pinkterest.Web.Security;
using Pinkterest.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddPinkterestObservability();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

builder.Services.AddAuthorizationBuilder()
    .AddPolicy(Policies.IsAdministrator, policy =>
        policy.RequireRole(Roles.Administrator))
    .AddPolicy(Policies.IsRegisteredUser, policy =>
        policy.RequireRole(Roles.RegisteredUser, Roles.Administrator));

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "pinkterest.auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.ExpireTimeSpan = TimeSpan.FromHours(2);
    options.SlidingExpiration = true;
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseSecurityHeaders();

if (builder.Configuration.GetValue("Https:Redirect", true))
{
    app.UseHttpsRedirection();
}
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapPrometheusScrapingEndpoint().RequireAuthorization(Policies.IsAdministrator);
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

await using (var scope = app.Services.CreateAsyncScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    await seeder.SeedAsync();
}

await app.RunAsync();

public partial class Program;
