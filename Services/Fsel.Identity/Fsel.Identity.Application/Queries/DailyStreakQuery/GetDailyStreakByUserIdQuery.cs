// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.DailyStreakQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetDailyStreakByUserIdQuery : IRequest<MethodResult<DailyStreakModel>>
    {
        public Guid Id { get; set; }
    }
    public class GetDailyStreakByUserIdQueryHandler : IRequestHandler<GetDailyStreakByUserIdQuery, MethodResult<DailyStreakModel>>
    {
        private readonly IStudentDailyStreakRepository _studentDailyStreakRepository;

        public GetDailyStreakByUserIdQueryHandler(IStudentDailyStreakRepository studentDailyStreakRepository)
        {
            _studentDailyStreakRepository = studentDailyStreakRepository;
        }

        public async Task<MethodResult<DailyStreakModel>> Handle(GetDailyStreakByUserIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<DailyStreakModel>();
            DailyStreakModel dailyStreakModel = new DailyStreakModel();
            var dailyStreak = await _studentDailyStreakRepository.Queryable.Where(x => x.StudentId == request.Id).ToListAsync(cancellationToken);
            if (dailyStreak.Any())
            {
                var userActivityDays = dailyStreak.GroupBy(log => log.DailyDate)
                        .Select(group => new
                        {
                            Date = group.Key,
                            Days = group.Distinct().Count()
                        }).ToList();

                var (numberOfDaysStreak, isDaysStreakIncrease) = await DateTimeHelper.CountContinuousDaysAsync(userActivityDays.Select(x => x.Date).ToList());
                dailyStreakModel.IsDaysStreakIncrease = isDaysStreakIncrease;
                dailyStreakModel.NumberOfDaysStreak = numberOfDaysStreak;
            }

            methodResult.Result = dailyStreakModel;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
