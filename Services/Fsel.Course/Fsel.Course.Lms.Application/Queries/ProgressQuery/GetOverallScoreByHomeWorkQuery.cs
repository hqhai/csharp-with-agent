// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ProgressQuery
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetOverallScoreByHomeWorkQuery : IRequest<MethodResult<OverallScoreReportModel>>
    {
        public Guid CourseId { get; set; }
    }

    public class GetOverallScoreByHomeWorkQueryHandler : IRequestHandler<GetOverallScoreByHomeWorkQuery, MethodResult<OverallScoreReportModel>>
    {
        private readonly AuthContext _authContext;
        private readonly IHomeWorkRepository _homeWorkRepository;
        private readonly IUnitRepository _unitRepository;
        private readonly ILessonHomeWorkRepository _lessonHomeWorkRepository;
        private readonly IUserService _userService;

        public GetOverallScoreByHomeWorkQueryHandler(AuthContext authContext
            , IHomeWorkRepository homeWorkRepository
            , IUnitRepository unitRepository
            , ILessonHomeWorkRepository lessonHomeWorkRepository
            , IUserService userService)
        {
            _authContext = authContext;
            _homeWorkRepository = homeWorkRepository;
            _unitRepository = unitRepository;
            _lessonHomeWorkRepository = lessonHomeWorkRepository;
            _userService = userService;
        }

        public async Task<MethodResult<OverallScoreReportModel>> Handle(GetOverallScoreByHomeWorkQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<OverallScoreReportModel> methodResult = new MethodResult<OverallScoreReportModel>();
            OverallScoreReportModel overallScoreReport = new OverallScoreReportModel();

            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentResult));
                return methodResult;
            }
            var studentId = studentResult?.Content?.Result?.Id;

            var units = await _unitRepository.Queryable.Include(x => x.CourseUnitMockTests)
                                                        .Include(x => x.UnitLessons)
                                                        .Where(x => x.CourseUnitMockTests.Any(x => x.CourseId == request.CourseId))
                                                        .ToListAsync(cancellationToken);
            if (units == null || units.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(units));
                return methodResult;
            }
            var lessonIds = units.SelectMany(x => x.UnitLessons).Select(x => x.LessonId).ToList();

            var lessonHomeWorks = await _lessonHomeWorkRepository.Queryable.Include(x => x.HomeWork).Where(x => lessonIds.Contains(x.LessonId)).ToListAsync(cancellationToken);
            if (lessonHomeWorks == null || lessonHomeWorks.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(lessonHomeWorks));
                return methodResult;
            }
            var homeWorkLessons = lessonHomeWorks.Select(x => x.HomeWork ?? new HomeWork()).ToList();
            var homeWorkIds = homeWorkLessons.Select(x => x.Id).ToList();
            var homeWorks = await _homeWorkRepository.Queryable.Include(x => x.HomeWorkResults.Where(x => x.StudentId == studentId))
                                                                .ThenInclude(x => x.HomeWorkAnswers)
                                                                .Include(x => x.HomeWorkQuestions)
                                                                .ThenInclude(x => x.Question)
                                                                .Where(x => homeWorkIds.Contains(x.Id))
                                                                .ToListAsync(cancellationToken);
            if (homeWorks == null || homeWorks.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(homeWorks));
                return methodResult;
            }
            foreach (var item in homeWorkLessons)
            {
                if (item != null)
                {
                    var homeWork = homeWorks.FirstOrDefault(x => x.Id == item.Id);
                    if (homeWork != null)
                    {
                        item.HomeWorkQuestions = homeWork.HomeWorkQuestions;
                        item.HomeWorkResults = homeWork.HomeWorkResults;
                    }
                }
            }

            var skillScores = homeWorkLessons.GroupBy(x => x.CourseSkill).Select(x => new SkillScores
            {
                Skill = x.Key,
                CountQuestion = x.SelectMany(x => x.HomeWorkResults).SelectMany(x => x.HomeWorkAnswers).Count(),
                TotalQuestion = x.SelectMany(x => x.HomeWorkQuestions).Select(x => x.Question).Count(),
                CorrectCount = x.SelectMany(x => x.HomeWorkResults).SelectMany(x => x.HomeWorkAnswers).Sum(x => x.CorrectCount),
                TotalCount = x.SelectMany(x => x.HomeWorkQuestions).Select(x => x.Question).Sum(x => x!.CorrectTotal),
            }).ToList();
            skillScores.ForEach(x => x.Percent = x.TotalCount > 0 ? x.CorrectCount / x.TotalCount : default);
            overallScoreReport.SkillScores = skillScores;
            overallScoreReport.CountQuestion = overallScoreReport.SkillScores.Sum(x => x.CountQuestion);
            overallScoreReport.TotalQuestion = overallScoreReport.SkillScores.Sum(x => x.TotalQuestion);
            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = overallScoreReport;
            return methodResult;
        }
    }
}
