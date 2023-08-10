// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.LogActionQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetLogActionByUserIdQuery : IRequest<MethodResult<LogActionDaysModel>>
    {
        public Guid Id { get; set; }
    }

    public class GetLogActionByUserIdQueryHandler : IRequestHandler<GetLogActionByUserIdQuery, MethodResult<LogActionDaysModel>>
    {
        private readonly ILogActionRepository _logActionRepository;

        public GetLogActionByUserIdQueryHandler(ILogActionRepository logActionRepository)
        {
            _logActionRepository = logActionRepository;
        }

        public async Task<MethodResult<LogActionDaysModel>> Handle(GetLogActionByUserIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<LogActionDaysModel>();
            LogActionDaysModel logActionDaysModel = new LogActionDaysModel();
            var logActions = await _logActionRepository.Queryable.Where(x => x.CreatedUserId == request.Id).ToListAsync(cancellationToken);
            if (logActions.Any())
            {
                var userActivityDays = logActions.GroupBy(log => log.CreatedDate.Date)
                        .Select(group => new
                        {
                            Date = group.Key,
                            Days = group.Distinct().Count()
                        }).ToList();

                var (numberOfDaysStreak, isDaysStreakIncrease) = await CountContinuousDaysAsync(userActivityDays.Select(x => x.Date).ToList());
                logActionDaysModel.IsDaysStreakIncrease = isDaysStreakIncrease;
                logActionDaysModel.NumberOfDaysStreak = numberOfDaysStreak;
            }

            methodResult.Result = logActionDaysModel;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private static async Task<(int, bool)> CountContinuousDaysAsync(IEnumerable<DateTime> dates)
        {
            int count = 0;
            DateTime? previousDate = null;
            bool isDaysStreakIncrease = true;
            foreach (var date in dates)
            {
                if (previousDate == null || (date - previousDate.Value).TotalDays == 1)
                {
                    count++;
                    isDaysStreakIncrease = true;
                }
                else
                {
                    count = 1;
                    isDaysStreakIncrease = false;
                }
                previousDate = date;
                await Task.Delay(1);
            }
            return (count, isDaysStreakIncrease);
        }
    }
}
