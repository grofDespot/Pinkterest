using Pinkterest.Application.Admin.Models;
using Pinkterest.Application.Common.Mediation;

namespace Pinkterest.Application.Admin.Requests;

public sealed record GetAuditLogQuery(
    int Page = 1,
    int PageSize = 30,
    string? Action = null,
    string? UserName = null) : IRequest<AuditLogPage>;
