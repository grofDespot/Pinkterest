using Pinkterest.Application.Admin.Models;
using Pinkterest.Application.Common.Mediation;

namespace Pinkterest.Application.Admin.Requests;

public sealed record GetAdminStatisticsQuery : IRequest<AdminStatistics>;
