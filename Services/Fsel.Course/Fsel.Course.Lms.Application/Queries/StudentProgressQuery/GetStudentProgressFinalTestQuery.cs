// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.StudentProgressQuery
{
    using System.Linq.Dynamic.Core;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.SystemService.Models;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetStudentProgressFinalTestQuery : IRequest<MethodResult<UnitStudentProgressModel>>
    {
        public Guid StudentId { get; set; }
        public Guid CourseId { get; set; }
    }

    public class GetStudentProgressFinalTestQueryHandler : IRequestHandler<GetStudentProgressFinalTestQuery, MethodResult<UnitStudentProgressModel>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IFinalTestResultRepository _finalTestResultRepository;
        private readonly IFinalTestRepository _finalTestRepository;
        private readonly IUserService _userService;
        private readonly ISystemService _systemService;
        private readonly ICourseUnitMockTestRepository _courseUnitMockTestRepository;

        public GetStudentProgressFinalTestQueryHandler(ICourseRepository courseRepository, IFinalTestResultRepository finalTestResultRepository, IFinalTestRepository finalTestRepository, IUserService userService, ISystemService systemService, ICourseUnitMockTestRepository courseUnitMockTestRepository)
        {
            _courseRepository = courseRepository;
            _finalTestResultRepository = finalTestResultRepository;
            _finalTestRepository = finalTestRepository;
            _userService = userService;
            _systemService = systemService;
            _courseUnitMockTestRepository = courseUnitMockTestRepository;
        }

        public async Task<MethodResult<UnitStudentProgressModel>> Handle(GetStudentProgressFinalTestQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<UnitStudentProgressModel> methodResult = new MethodResult<UnitStudentProgressModel>();
            UnitStudentProgressModel finalStudentProgress = new UnitStudentProgressModel();
            var studentResults = await _userService.GetStudentsByStudentIdsAsync(new List<Guid> { request.StudentId });
            if (!studentResults.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResults));
                return methodResult;
            }
            var student = studentResults?.Content?.Result?.FirstOrDefault();
            var userId = student?.Human?.UserId;
            var course = await _courseRepository.GetByIdAsync(request.CourseId);
            if (course == null)
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            var courseUnitMockTests = await _courseUnitMockTestRepository.Queryable.Where(x => x.CourseId == request.CourseId).OrderBy(x => x.DisplayOrder).ToListAsync(cancellationToken);
            if (courseUnitMockTests == null || !courseUnitMockTests.Any())
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            if (course.CourseType == EnumCourseType.Ielts)
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            var finalTestId = courseUnitMockTests.Where(x => x.FinalTestId != null).Select(x => x.FinalTestId ?? default).FirstOrDefault();

            var finalTestResult = await _finalTestResultRepository.Queryable.Where(x => x.FinalTestId == finalTestId && x.CourseId == request.CourseId && x.StudentId == request.StudentId).FirstOrDefaultAsync(cancellationToken);
            if (finalTestResult == null)
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            var featureAccessTimeResult = await _systemService.GetFeatureAccessTimeAsync(new FeatureAccessTimeQueryModel
            {
                UserId = userId ?? default,
                ObjectId = finalTestResult.Id,
                EnumFeature = EnumFeature.FinalTest,
                CourseId = course.Id
            });
            var featureAccessTime = featureAccessTimeResult?.Content?.Result;

            var finalTest = await _finalTestRepository.Queryable.Include(x => x.FinalTestSections)
                                                    .ThenInclude(x => x.SectionGroup)
                                                    .ThenInclude(x => x!.Sections)
                                                    .ThenInclude(x => x.SectionQuestions)
                                                    .ThenInclude(x => x.Question)
                                                    .Include(x => x.FinalTestSections)
                                                    .ThenInclude(x => x.SectionGroup)
                                                    .ThenInclude(x => x!.Sections)
                                                    .ThenInclude(x => x.SectionQuestions)
                                                    .ThenInclude(x => x.FinalTestAnswers.Where(x => x.FinalTestResultId == finalTestResult.Id))
                                                    .Where(x => x.Id == finalTestId)
                                                    .AsNoTracking()
                                                    .FirstOrDefaultAsync(cancellationToken);

            if (finalTest != null)
            {
                finalStudentProgress.Type = nameof(finalTestResult.FinalTest);
                finalStudentProgress.ObjectId = finalTest.Id;
                finalStudentProgress.Name = finalTest.Name;
                finalStudentProgress.SkillScores = finalTest.FinalTestSections.Select(x => x.SectionGroup).Select(x =>
                {
                    var sectionQuestions = x!.Sections.SelectMany(x => x.SectionQuestions).ToList();
                    var correctTotal = sectionQuestions.Select(x => x.Question).Sum(x => x!.CorrectTotal);
                    var correctCount = sectionQuestions.SelectMany(x => x.FinalTestAnswers).Sum(x => x!.CorrectCount);
                    var totalQuestion = sectionQuestions.Select(x => x.Question).Count();
                    var countQuestion = sectionQuestions.SelectMany(x => x.FinalTestAnswers).Count();
                    var skillScores = new TestSkillScores
                    {
                        Skill = x.CourseSkill,
                        CorrectCount = correctCount,
                        TotalCount = correctTotal,
                        Percent = NumberHelper.GetPercent(correctCount, correctTotal),
                        CountQuestion = countQuestion,
                        TotalQuestion = totalQuestion,
                        PercentProgress = NumberHelper.GetPercent(countQuestion, totalQuestion)
                    };
                    return skillScores;
                }).ToList();
            }
            var totalSkill = finalStudentProgress.SkillScores.Count;
            var skillDone = finalStudentProgress.SkillScores.Where(x => x.CountQuestion == x.TotalQuestion).Count();
            var isDone = finalTestResult.Status == EnumResultStatus.Done;

            finalStudentProgress.Status = finalTestResult.Status;
            finalStudentProgress.CorrectPercent = Math.Round(finalStudentProgress.SkillScores.Average(x => x.Percent), 0);
            finalStudentProgress.ContentProgress = string.Format("{0} / {1}", isDone ? 1 : 0, 1);
            finalStudentProgress.ProcessPercent = NumberHelper.ConvertPercentDouble((double)finalStudentProgress.SkillScores.Average(x => x.CountQuestion / x.TotalQuestion));
            finalStudentProgress.TotalSkill = totalSkill;
            if (featureAccessTime != null)
            {
                finalStudentProgress.TimeSpent = featureAccessTime.AccessTime;
                finalStudentProgress.LastVisited = featureAccessTime.LastVisited ?? null;
            }

            methodResult.Result = finalStudentProgress;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
