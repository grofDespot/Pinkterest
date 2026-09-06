using System.Net.Http.Headers;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Pinkterest.IntegrationTests.Infrastructure;
using Xunit;

namespace Pinkterest.IntegrationTests;

[Collection(PinkterestCollection.Name)]
public class PhotoLifecycleTests(PinkterestFixture fixture)
{
    [Fact]
    public async Task A_user_can_upload_a_photo()
    {
        var (client, _, userId) = await WebFlows.RegisterCookieClientAsync(fixture);

        var photoId = await WebFlows.UploadPhotoAsync(fixture, client, userId);

        photoId.Should().NotBe(Guid.Empty);

        var owner = await fixture.UseDbContextAsync(context =>
            context.Photos.Where(p => p.Id == photoId).Select(p => p.OwnerId).SingleAsync());
        owner.Should().Be(userId);
    }

    [Fact]
    public async Task A_preset_can_be_saved_listed_exported_imported_and_deleted()
    {
        var (client, email, userId) = await WebFlows.RegisterCookieClientAsync(fixture);
        var photoId = await WebFlows.UploadPhotoAsync(fixture, client, userId);
        var token = await client.GetAntiforgeryTokenAsync("/Photos/Upload");

        await client.PostFormAsync("/Photos/SavePreset", new Dictionary<string, string>
        {
            ["Name"] = "My preset",
            ["PhotoId"] = photoId.ToString(),
            ["Format"] = "Jpeg"
        }, token);

        var list = await client.GetAsync("/Photos/Presets");
        list.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var presetId = await fixture.UseDbContextAsync(context => context.FilterPresets
            .Where(p => p.OwnerId == userId)
            .Select(p => p.Id)
            .FirstAsync());

        var bearer = (await JwtTokens.LoginAsync(fixture, email, TestCredentials.UserPassword)).AccessToken;

        var export = await fixture.CreateClient().WithBearer(bearer)
            .GetAsync($"/api/photos/presets/{presetId}/export");
        export.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var package = await export.Content.ReadAsByteArrayAsync();
        package.Should().NotBeEmpty();

        var payload = new ByteArrayContent(package);
        payload.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        var import = new MultipartFormDataContent { { payload, "file", "preset.pkpreset" } };

        var importResponse = await fixture.CreateClient().WithBearer(bearer)
            .PostAsync("/api/photos/presets/import", import);
        importResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var delete = await client.PostFormAsync("/Photos/DeletePreset", new Dictionary<string, string>
        {
            ["id"] = presetId.ToString()
        }, token);
        delete.RedirectsTo("/Photos/Presets").Should().BeTrue();
    }
}
