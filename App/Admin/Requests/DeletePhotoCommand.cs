using Pinkterest.Application.Common.Mediation;
using Pinkterest.Application.Common.Results;

namespace Pinkterest.Application.Admin.Requests;

public sealed record DeletePhotoCommand(Guid PhotoId) : IRequest<Result>;
