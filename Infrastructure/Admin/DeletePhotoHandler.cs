using Microsoft.EntityFrameworkCore;
using Pinkterest.Application.Admin.Requests;
using Pinkterest.Application.Common.Mediation;
using Pinkterest.Application.Common.Results;
using Pinkterest.Infrastructure.Persistence;

namespace Pinkterest.Infrastructure.Admin;

public sealed class DeletePhotoHandler(ApplicationDbContext context)
    : IRequestHandler<DeletePhotoCommand, Result>
{
    public async Task<Result> HandleAsync(
        DeletePhotoCommand request,
        CancellationToken cancellationToken = default)
    {
        var photo = await context.Photos
            .SingleOrDefaultAsync(candidate => candidate.Id == request.PhotoId, cancellationToken);

        if (photo is null)
        {
            return Result.Failure(Error.NotFound("Photo"));
        }

        photo.IsDeleted = true;
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
