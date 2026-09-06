using System.Net.Http.Headers;
using Microsoft.EntityFrameworkCore;
using Pinkterest.Domain.Enums;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace Pinkterest.IntegrationTests.Infrastructure;

public static class WebFlows
{
    public static async Task<(HttpClient Client, string Email, Guid UserId)> RegisterCookieClientAsync(
        PinkterestFixture fixture)
    {
        var client = fixture.CreateClient();
        var email = $"flow-{Guid.CreateVersion7():N}@pinkterest.test";

        var freePackageId = await fixture.UseDbContextAsync(context =>
            context.Packages.Where(p => p.Tier == PackageTier.Free).Select(p => p.Id).SingleAsync());

        await client.PostFormAsync("/Account/Register", new Dictionary<string, string>
        {
            ["DisplayName"] = "Flow user",
            ["Email"] = email,
            ["Password"] = TestCredentials.UserPassword,
            ["ConfirmPassword"] = TestCredentials.UserPassword,
            ["PackageId"] = freePackageId.ToString()
        });

        var userId = await fixture.UseDbContextAsync(context =>
            context.Users.Where(u => u.Email == email).Select(u => u.Id).SingleAsync());

        return (client, email, userId);
    }

    public static async Task<HttpClient> SignInAsync(PinkterestFixture fixture, string email, string password)
    {
        var client = fixture.CreateClient();

        await client.PostFormAsync("/Account/Login", new Dictionary<string, string>
        {
            ["Email"] = email,
            ["Password"] = password
        });

        return client;
    }

    public static async Task<Guid> UploadPhotoAsync(
        PinkterestFixture fixture, HttpClient client, Guid userId)
    {
        var token = await client.GetAntiforgeryTokenAsync("/Photos/Upload");

        var file = new ByteArrayContent(PngBytes());
        file.Headers.ContentType = new MediaTypeHeaderValue("image/png");

        var form = new MultipartFormDataContent
        {
            { new StringContent(token), "__RequestVerificationToken" },
            { new StringContent("A test photo"), "Description" },
            { new StringContent("Original"), "Format" },
            { file, "File", "test.png" }
        };

        await client.PostAsync("/Photos/Upload", form);

        return await fixture.UseDbContextAsync(context => context.Photos
            .Where(p => p.OwnerId == userId)
            .OrderByDescending(p => p.UploadedUtc)
            .Select(p => p.Id)
            .FirstAsync());
    }

    private static byte[] PngBytes()
    {
        using var image = new Image<Rgba32>(8, 8);
        using var stream = new MemoryStream();
        image.SaveAsPng(stream);
        return stream.ToArray();
    }
}
