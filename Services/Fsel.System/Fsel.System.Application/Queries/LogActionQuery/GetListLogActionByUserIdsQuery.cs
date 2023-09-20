// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.LogActionQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Shared.Helpers;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using global::System;
    using global::System.Collections.Generic;
    using global::System.Linq;
    using global::System.Threading.Tasks;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetListLogActionByUserIdsQuery : IRequest<MethodResult<IList<LogActionDaysModel>>>
    {
        public IList<Guid>? Ids { get; set; }
    }

    public class GetListLogActionByUserIdsQueryHandler : IRequestHandler<GetListLogActionByUserIdsQuery, MethodResult<IList<LogActionDaysModel>>>
    {
        private readonly ILogActionRepository _logActionRepository;

        public GetListLogActionByUserIdsQueryHandler(ILogActionRepository logActionRepository)
        {
            _logActionRepository = logActionRepository;
        }

        public async Task<MethodResult<IList<LogActionDaysModel>>> Handle(GetListLogActionByUserIdsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<LogActionDaysModel>>();
            var logActionDays = new List<LogActionDaysModel>();
            if (request.Ids == null || request.Ids.Count == 0)
            {
                methodResult.Result = null;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            foreach (var id in request.Ids)
            {
                LogActionDaysModel logActionDaysModel = new LogActionDaysModel();
                var logActions = await _logActionRepository.Queryable.Where(x => x.CreatedUserId == id).ToListAsync(cancellationToken);
                if (logActions.Any())
                {
                    var userActivityDays = logActions.GroupBy(log => log.CreatedDate.Date)
                            .Select(group => new
                            {
                                Date = group.Key,
                                Days = group.Distinct().Count()
                            }).ToList();

                    var (numberOfDaysStreak, isDaysStreakIncrease) = await DateTimeHelper.CountContinuousDaysAsync(userActivityDays.Select(x => x.Date).ToList());
                    logActionDaysModel.IsDaysStreakIncrease = isDaysStreakIncrease;
                    logActionDaysModel.NumberOfDaysStreak = numberOfDaysStreak;
                    logActionDays.Add(logActionDaysModel);
                }
            }

            methodResult.Result = logActionDays;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
