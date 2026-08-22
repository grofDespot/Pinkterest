using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using Pinkterest.Application.Photos.Storage;

namespace Pinkterest.Infrastructure.Storage;

public sealed class S3PhotoStorage(IAmazonS3 client, IOptions<S3StorageOptions> options) : IPhotoStorage
{
    private readonly S3StorageOptions _options = options.Value;

    public string ProviderName => "s3";

    public async Task SaveAsync(
        string key,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default) =>
        await client.PutObjectAsync(
            new PutObjectRequest
            {
                BucketName = _options.BucketName,
                Key = Normalize(key),
                InputStream = content,
                ContentType = contentType,
                AutoCloseStream = false
            },
            cancellationToken);

    public async Task<Stream> OpenReadAsync(string key, CancellationToken cancellationToken = default)
    {
        var response = await client.GetObjectAsync(
            new GetObjectRequest { BucketName = _options.BucketName, Key = Normalize(key) },
            cancellationToken);

        return response.ResponseStream;
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await client.GetObjectMetadataAsync(
                new GetObjectMetadataRequest { BucketName = _options.BucketName, Key = Normalize(key) },
                cancellationToken);

            return true;
        }
        catch (AmazonS3Exception exception) when (exception.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    public async Task DeleteAsync(string key, CancellationToken cancellationToken = default) =>
        await client.DeleteObjectAsync(
            new DeleteObjectRequest { BucketName = _options.BucketName, Key = Normalize(key) },
            cancellationToken);

    private static string Normalize(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("A storage key is required.", nameof(key));
        }

        var normalized = key.Replace('\\', '/').TrimStart('/');

        if (normalized.Split('/').Any(segment => segment is ".." or "."))
        {
            throw new InvalidOperationException("The storage key resolves outside the storage root.");
        }

        return normalized;
    }
}
