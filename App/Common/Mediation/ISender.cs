namespace Pinkterest.Application.Common.Mediation;

public interface ISender
{
    Task<TResponse> SendAsync<TResponse>(
        IRequest<TResponse> request,
        CancellationToken cancellationToken = default);
}
