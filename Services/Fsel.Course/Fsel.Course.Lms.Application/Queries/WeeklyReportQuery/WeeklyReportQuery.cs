// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.WeeklyReportQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums;
    using Fsel.Common.Models;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class WeeklyReportQuery : IRequest<MethodResult<bool>>
    {
    }
    public class WeeklyReportQueryHandler : IRequestHandler<WeeklyReportQuery, MethodResult<bool>>
    {
        private readonly IUserService _userService;
        private readonly ISystemService _systemService;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly IUnitResultRepository _unitResultRepository;
        public WeeklyReportQueryHandler(IUserService userService, ISystemService systemService, ILessonResultRepository lessonResultRepository, IUnitResultRepository unitResultRepository)
        {
            _userService = userService;
            _systemService = systemService;
            _lessonResultRepository = lessonResultRepository;
            _unitResultRepository = unitResultRepository;
        }

        public async Task<MethodResult<bool>> Handle(WeeklyReportQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var studentResults = await _userService.ExecuteListQueryAsync(new BaseQueryModel { IncludePaths = new List<string>() { "Human" } });

            var students = studentResults.Content?.Result;
            if (students == null)
            {
                return methodResult;
            }

            DateTime currentDate = DateTime.UtcNow;

            // Lấy ngày thứ 6 gần nhất lúc 13h
            DateTime lastFridayAt13 = GetLastFridayAt13(currentDate);

            // Lấy danh sách ngày từ thứ 6 tuần trước đến giờ
            var dates = GenerateDateList(lastFridayAt13, currentDate);

            var studentDailyStreakResults = await _userService.GetAllDailyStreak(new BaseQueryModel()
            {
                Filters = new List<GenericFilterModel>() { new GenericFilterModel()
                {
                    Property = "DailyDate",
                    Operator = EnumFilterOperator.GreaterThan,
                    Value = lastFridayAt13
                } }
            });

            var featureAccessTimeResults = await _systemService.GetListFeatureAccessTime(new BaseQueryModel()
            {
                Filters = new List<GenericFilterModel>() { new GenericFilterModel()
                {
                    Property = "CreatedDate",
                    Operator = EnumFilterOperator.GreaterThan,
                    Value = lastFridayAt13
                } }
            });

            foreach (var item in students)
            {
                var studentDailyStreaks = studentDailyStreakResults.Content?.Result?.Where(p => p.StudentId == item.Id);

                var featureAccessTimes = featureAccessTimeResults.Content?.Result;

                var lessonResultsDone = await _lessonResultRepository.Queryable.Include(x => x.ClassForumResults).Include(x => x.Unit).Where(p => p.StudentId == item.Id && p.Status == EnumResultStatus.Done).Where(p => p.UpdatedDate >= lastFridayAt13 && p.UpdatedDate <= currentDate).OrderBy(n => n.UpdatedDate).ToListAsync(cancellationToken);

                //var unitResultsDone = await _unitResultRepository.Queryable.Where(p => p.StudentId == item.Id && p.Status == EnumResultStatus.Done).OrderByDescending(n => n.UpdatedDate).ToListAsync(cancellationToken);

                //var lessonResults = lessonResultsDone.Where(p => p.UpdatedDate >= lastFridayAt13 && p.UpdatedDate <= currentDate);

                //if (lessonResults != null)
                //{
                //    foreach (var lessonResult in lessonResults)
                //    {
                //        if (lessonResult.ClassForumResults != null && lessonResult.ClassForumResults.Any(p => p.Status == EnumClassForumResultStatus.Graded))
                //        {
                //            var unit = lessonResult.SkillScores;
                //        }
                //    }
                //}
            }
            return methodResult;
        }

        private static DateTime GetLastFridayAt13(DateTime currentDate)
        {
            while (currentDate.DayOfWeek != DayOfWeek.Friday)
            {
                currentDate = currentDate.AddDays(-1);
            }
            return currentDate.AddHours(13);
        }

        private static List<DateTime> GenerateDateList(DateTime startDate, DateTime endDate)
        {
            List<DateTime> dateList = new List<DateTime>();

            while (startDate <= endDate)
            {
                dateList.Add(startDate);
                startDate = startDate.AddDays(1);
            }

            return dateList;
        }
    }
}
