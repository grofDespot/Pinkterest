namespace Pinkterest.Application.Photos.Storage;

public interface IPhotoStorageFactory
{
    IPhotoStorage Create();
}
