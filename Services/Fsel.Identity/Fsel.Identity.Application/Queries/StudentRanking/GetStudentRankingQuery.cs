// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.StudentRanking

{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetStudentRankingQuery : BaseQueryModel, IRequest<MethodResult<List<StudentRankingModel>>>
    {
        public EnumCourseLevel CourseLevel { get; set; }
    }

    public class GetStudentRankingQueryHandler : IRequestHandler<GetStudentRankingQuery, MethodResult<List<StudentRankingModel>>>
    {
        private readonly IStudentRankingRepository _studentRankingRepository;
        private readonly IMapper _mapper;
        private readonly IStudentRepository _studentRepository;
        private readonly IStudentDailyStreakRepository _studentDailyStreakRepository;
        private const int TOP_LEADER = 30;

        public GetStudentRankingQueryHandler(IStudentRankingRepository studentRankingRepository, IMapper mapper, IStudentRepository studentRepository, IStudentDailyStreakRepository studentDailyStreakRepository)
        {
            _studentRankingRepository = studentRankingRepository;
            _mapper = mapper;
            _studentRepository = studentRepository;
            _studentDailyStreakRepository = studentDailyStreakRepository;
        }

        public async Task<MethodResult<List<StudentRankingModel>>> Handle(GetStudentRankingQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<List<StudentRankingModel>> methodResult = new MethodResult<List<StudentRankingModel>>();

            var studentRankingsQuery = await _studentRankingRepository.Queryable.Where(x => x.CourseLevel == request.CourseLevel)
                                                                                .OrderByDescending(x => x.TotalScore)
                                                                                .ThenBy(x => x.CurrentPosition)
                                                                                .Take(TOP_LEADER)
                                                                                .ToListAsync(cancellationToken);

            var studentIds = studentRankingsQuery.Select(s => s.StudentId);
            var studentInfo = _studentRepository.Queryable.Include(x => x.User).Where(x => studentIds.Contains(x.Id)).ToList();
            var studentRankingResult = _mapper.Map<List<StudentRankingModel>>(studentRankingsQuery);

            var studentDailyStreak = _studentDailyStreakRepository.Queryable.Where(x => studentIds.Contains(x.StudentId)).ToList();

            // Lấy số ngày đăng nhập liên tiếp của từng user dựa vào ngày hiện tại trở về
            var listDailyStreakByStudents = studentIds.Select(studentId => new
            {
                StudentId = studentId,
                ConsecutiveDays = GetConsecutiveDays(studentDailyStreak, studentId)
            }).ToList();

            studentRankingResult.ForEach(x =>
            {
                var student = studentInfo.FirstOrDefault(s => s.Id == x.StudentId);
                var dailyStreak = listDailyStreakByStudents.FirstOrDefault(d => d.StudentId == x.StudentId)?.ConsecutiveDays ?? 0;
                if (student != null)
                {
                    x.FullName = student.User?.FullName;
                    x.AvatarPath = student.User?.AvatarPath;
                    x.UserId = student.UserId;
                    x.DailyStreak = dailyStreak;
                }
            });

            methodResult.Result = studentRankingResult;
            methodResult.Result = _mapper.Map<List<StudentRankingModel>>(studentRankingResult);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        public static int GetConsecutiveDays(ICollection<StudentDailyStreak> studentDailyStreaks, Guid studentId)
        {
            var userStreaks = studentDailyStreaks?
                .Where(s => s.StudentId == studentId)
                .OrderBy(s => s.DailyDate)
                .ToList() ?? new List<StudentDailyStreak>();

            int consecutiveDays = 0;
            DateTime currentDate = DateTime.UtcNow.Date;

            var todayStreak = userStreaks.LastOrDefault(s => s.DailyDate.Date == currentDate);
            var yesterdayStreak = userStreaks.LastOrDefault(s => s.DailyDate.Date == currentDate.AddDays(-1));

            if (todayStreak == null && yesterdayStreak == null)
            {
                return consecutiveDays;
            }

            var targetStreak = todayStreak ?? yesterdayStreak;

            if (targetStreak == yesterdayStreak)
            {
                currentDate = currentDate.AddDays(-1);
            }

            for (int i = userStreaks.IndexOf(targetStreak!); i >= 0; i--)
            {
                double dailyRange = (currentDate - userStreaks[i].DailyDate.Date).TotalDays;

                if (dailyRange >= 0 && dailyRange <= consecutiveDays)
                {
                    consecutiveDays++;
                }
                else
                {
                    break;  // Ngừng nếu gặp ngày không liền kề
                }
            }

            return consecutiveDays;
        }
    }
}
