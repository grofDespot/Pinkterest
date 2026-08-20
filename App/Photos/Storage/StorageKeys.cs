namespace Pinkterest.Application.Photos.Storage;

public static class StorageKeys
{
    public static string ForPhoto(Guid ownerId, Guid photoId, string extension) =>
        $"photos/{ownerId:N}/{photoId:N}{extension}";

    public static string ForThumbnail(Guid ownerId, Guid photoId) =>
        $"thumbnails/{ownerId:N}/{photoId:N}.jpg";
}
