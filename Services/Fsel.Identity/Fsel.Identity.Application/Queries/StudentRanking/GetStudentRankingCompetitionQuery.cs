// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.StudentRanking

{
    using System.Linq.Dynamic.Core;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Shared.Constants;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetStudentRankingCompetitionQuery : BaseQueryModel, IRequest<MethodResult<PagingItemsModel<StudentRankingModel>>>
    {
    }

    public class GetStudentRankingCompetitionQueryHandler : IRequestHandler<GetStudentRankingCompetitionQuery, MethodResult<PagingItemsModel<StudentRankingModel>>>
    {
        private readonly IStudentRankingRepository _studentRankingRepository;
        private readonly IMapper _mapper;
        private readonly IStudentRepository _studentRepository;
        private readonly IStudentDailyStreakRepository _studentDailyStreakRepository;

        public GetStudentRankingCompetitionQueryHandler(IStudentRankingRepository studentRankingRepository, IMapper mapper, IStudentRepository studentRepository, IStudentDailyStreakRepository studentDailyStreakRepository)
        {
            _studentRankingRepository = studentRankingRepository;
            _mapper = mapper;
            _studentRepository = studentRepository;
            _studentDailyStreakRepository = studentDailyStreakRepository;
        }

        public async Task<MethodResult<PagingItemsModel<StudentRankingModel>>> Handle(GetStudentRankingCompetitionQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<PagingItemsModel<StudentRankingModel>> methodResult = new MethodResult<PagingItemsModel<StudentRankingModel>>();

            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ResourceSettings.AcademicStudentsName);

            var listStudentCompetition = ConvertHelper.DeserializeFromFilePath<IList<StudentJoinCompetitionModel>>(path);

            List<Guid> competitionStudentIds = listStudentCompetition!.Select(x => x.StudentId).ToList();

            var listStudentCompetion = listStudentCompetition!.ToList();


            var studentRankingsQuery = _studentRankingRepository.Queryable.Where(x => competitionStudentIds.Contains(x.StudentId))
                                                                          .OrderBy(x => x.CurrentPosition)
                                                                          .Select(x => new StudentRankingModel
                                                                          {
                                                                              StudentId = x.StudentId,
                                                                          });

            var queryResult = from student in studentRankingsQuery.ToList()
                              join studentFile in listStudentCompetion on student.StudentId equals studentFile.StudentId
                              select new StudentRankingModel
                              {
                                  StudentId = student.StudentId,
                                  SchoolName = studentFile.SchoolName,
                                  Grade = studentFile.Grade,
                                  Process = 0,
                                  OverallScore = 0,
                                  CompetitionEndDate = DateTime.UtcNow.AddDays(10),
                                  FullName = studentFile.FullName,
                                  AvatarPath = string.Empty,
                                  CurrentPosition = 0,
                                  UserId = studentFile.UserId
                              };


            var studentIds = studentRankingsQuery.Select(s => s.StudentId);
            var studentInfo = _studentRepository.Queryable.Include(x => x.Human).Where(x => studentIds.Contains(x.Id)).ToList();

            // var studentRankingResult = _mapper.Map<List<StudentRankingModel>>(studentRankingsQuery);
            var studentDailyStreak = _studentDailyStreakRepository.Queryable.Where(x => studentIds.Contains(x.StudentId)).ToList();

            // Lấy số ngày đăng nhập liên tiếp của từng user dựa vào ngày hiện tại trở về
            var listDailyStreakByStudents = studentIds.Select(studentId => new
            {
                StudentId = studentId,
                ConsecutiveDays = GetConsecutiveDays(studentDailyStreak, studentId)
            }).ToList();


            int totalItem = queryResult.Count();

            var lists = queryResult.ApplyPaging(request).ToList();

            methodResult.Result = new PagingItemsModel<StudentRankingModel>(lists, request, totalItem);
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
