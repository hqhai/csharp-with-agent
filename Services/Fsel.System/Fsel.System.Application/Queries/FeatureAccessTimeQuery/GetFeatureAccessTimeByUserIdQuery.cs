// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.FeatureAccessTimeQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.System.Domain.IRepositories;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetFeatureAccessTimeByUserIdQuery : IRequest<MethodResult<IList<UserFeatureAccessSummaryDto>>>
    {
        public IList<Guid>? UserIds { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class GetFeatureAccessTimeByUserIdQueryHandler : IRequestHandler<GetFeatureAccessTimeByUserIdQuery, MethodResult<IList<UserFeatureAccessSummaryDto>>>
    {
        private readonly IFeatureAccessTimeRepository _featureAccessTimeRepository;

        public GetFeatureAccessTimeByUserIdQueryHandler(IFeatureAccessTimeRepository featureAccessTimeRepository)
        {
            _featureAccessTimeRepository = featureAccessTimeRepository;
        }

        public async Task<MethodResult<IList<UserFeatureAccessSummaryDto>>> Handle(GetFeatureAccessTimeByUserIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<UserFeatureAccessSummaryDto>>();
            if (request.UserIds == null || !request.UserIds.Any())
            {
                return methodResult;
            }

            var query = _featureAccessTimeRepository.Queryable.WhereBulkContains(request.UserIds, x => x.CreatedUserId);
            if (request.StartDate.HasValue)
            {
                query = query.Where(x => x.CreatedDate.Date >= request.StartDate.Value.Date);
            }
            if (request.EndDate.HasValue)
            {
                query = query.Where(x => x.CreatedDate.Date <= request.EndDate.Value.Date);
            }

            var logs = await query.Select(x => new
            {
                x.CreatedUserId,
                x.LastVisited,
                x.AccessTime
            }).ToListAsync(cancellationToken);

            var weekly = logs.GroupBy(x => x.CreatedUserId)
                              .Select(g =>
                              {
                                  return new UserFeatureAccessSummaryDto
                                  {
                                      CreatedUserId = g.Key,
                                      LastVisited = g.OrderByDescending(x => x.LastVisited).FirstOrDefault() != null ? g.OrderByDescending(x => x.LastVisited).FirstOrDefault()!.LastVisited : null,
                                      AccessTime = g.Sum(x => x.AccessTime)
                                  };
                              })
                              .ToList();

            methodResult.Result = weekly;
            return methodResult;
        }
    }

    public class UserFeatureAccessSummaryDto
    {
        public Guid? CreatedUserId { get; set; }
        public DateTime? LastVisited { get; set; }
        public long? AccessTime { get; set; }
    }
}
