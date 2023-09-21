// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.StudentProgressQuery
{
    using System.Linq.Dynamic.Core;
    using System.Threading;
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

    public class GetStudentProgressMockTestsQuery : IRequest<MethodResult<IList<UnitStudentProgressModel>>>
    {
        public Guid StudentId { get; set; }
        public Guid CourseId { get; set; }
    }

    public class GetStudentProgressMockTestsQueryHandler : IRequestHandler<GetStudentProgressMockTestsQuery, MethodResult<IList<UnitStudentProgressModel>>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly ISectionGroupRepository _sectionGroupRepository;
        private readonly IMockTestSectionRepository _mockTestSectionRepository;
        private readonly IMockTestScoreRepository _mockTestScoreRepository;
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly IMockTestRepository _mockTestRepository;
        private readonly IUserService _userService;
        private readonly ISystemService _systemService;
        private readonly ICourseUnitMockTestRepository _courseUnitMockTestRepository;

        public GetStudentProgressMockTestsQueryHandler(ICourseRepository courseRepository, ISectionGroupRepository sectionGroupRepository, IMockTestSectionRepository mockTestSectionRepository, IMockTestScoreRepository mockTestScoreRepository, IMockTestResultRepository mockTestResultRepository, ILessonResultRepository lessonResultRepository, IUnitRepository unitRepository, IMockTestRepository mockTestRepository, IFinalTestRepository finalTestRepository, IUserService userService, ISystemService systemService, ICourseUnitMockTestRepository courseUnitMockTestRepository)
        {
            _courseRepository = courseRepository;
            _sectionGroupRepository = sectionGroupRepository;
            _mockTestSectionRepository = mockTestSectionRepository;
            _mockTestScoreRepository = mockTestScoreRepository;
            _mockTestResultRepository = mockTestResultRepository;
            _mockTestRepository = mockTestRepository;
            _userService = userService;
            _systemService = systemService;
            _courseUnitMockTestRepository = courseUnitMockTestRepository;
        }

        public async Task<MethodResult<IList<UnitStudentProgressModel>>> Handle(GetStudentProgressMockTestsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<UnitStudentProgressModel>> methodResult = new MethodResult<IList<UnitStudentProgressModel>>();
            IList<UnitStudentProgressModel> mockTestStudentProgress = new List<UnitStudentProgressModel>();
            var studentResults = await _userService.GetStudentsByStudentIdsAsync(new List<Guid> { request.StudentId });
            if (!studentResults.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResults));
                return methodResult;
            }

            var student = studentResults?.Content?.Result?.FirstOrDefault();
            var studentId = student?.Id;
            var userId = student?.Human?.UserId;
            var course = await _courseRepository.GetByIdAsync(request.CourseId);
            if (course == null)
            {
                methodResult.Result = default;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            var courseUnitMockTests = await _courseUnitMockTestRepository.Queryable.Where(x => x.CourseId == request.CourseId).OrderBy(x => x.DisplayOrder).ToListAsync(cancellationToken);
            if (courseUnitMockTests == null || !courseUnitMockTests.Any())
            {
                methodResult.Result = default;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }

            if (course.CourseType == EnumCourseType.Academic)
            {
                methodResult.Result = default;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            var mockTestIds = courseUnitMockTests.Where(x => x.MockTestId != null).Select(x => x.MockTestId ?? default).ToList();
            var featureAccessTimes = await _systemService.GetFeatureAccessTimesAsync(new FeatureAccessTimesQueryModel
            {
                UserId = userId ?? default,
                FeatureAccessTimes = mockTestIds.Select(x => new FeatureAccessTimeQueryModel
                {
                    UserId = userId ?? default,
                    ObjectId = x,
                    EnumFeature = EnumFeature.MockTest,
                    CourseId = course.Id
                }).ToList(),
            });
            var featureAccessTimeTest = featureAccessTimes?.Content?.Result?.ToList();
            var mockTestResults = await _mockTestResultRepository.Queryable.Include(x => x.MockTest).Where(x => mockTestIds.Contains(x.MockTestId) && x.CourseId == request.CourseId && x.StudentId == request.StudentId).ToListAsync(cancellationToken);
            foreach (var item in mockTestResults)
            {
                UnitStudentProgressModel mockTestProgress = new UnitStudentProgressModel();
                var query = from baseQ in _mockTestRepository.Queryable
                            join msg in _mockTestSectionRepository.Queryable on baseQ.Id equals msg.MockTestId
                            join sg in _sectionGroupRepository.Queryable on msg.SectionGroupId equals sg.Id
                            join mr in _mockTestResultRepository.Queryable on baseQ.Id equals mr.MockTestId into mrGroupG
                            from mrGroup in mrGroupG.DefaultIfEmpty()
                            join ms in _mockTestScoreRepository.Queryable on mrGroup.Id equals ms.MockTestResultId into msGroupG
                            from msGroup in msGroupG.DefaultIfEmpty()
                            where baseQ.Id == item.MockTestId && mrGroup.StudentId == request.StudentId
                            group new { sg, mrGroup, msGroup } by new { sg.CourseSkill } into g
                            select new
                            {
                                Skill = g.Key.CourseSkill,
                                MockTestResult = g.Select(x => x.mrGroup).FirstOrDefault(),
                                MockTestScores = g.Select(x => x.msGroup).ToList()
                            };

                var skillMockTest = await query.ToListAsync(cancellationToken);
                mockTestProgress.SkillScores = skillMockTest.Select(x =>
                {
                    var skillScore = x.MockTestResult?.SkillScores?.FirstOrDefault(z => z.Skill == x.Skill);
                    var skillScores = new TestSkillScores
                    {
                        Skill = x.Skill,
                        CorrectCount = skillScore?.CorrectCount ?? default,
                        TotalCount = skillScore?.TotalCount ?? default,
                        CountQuestion = skillScore?.CountQuestion ?? default,
                        TotalQuestion = skillScore?.TotalQuestion ?? default,
                        Scores = skillScore?.Scores ?? default,
                        Percent = skillScore?.Percent ?? default,
                    };
                    if (x.Skill == EnumCourseSkill.Speaking || x.Skill == EnumCourseSkill.Writing)
                    {
                        if (x.MockTestScores.Any() && x.MockTestScores.All(x => x != null))
                        {
                            skillScores.Status = EnumResultStatus.Done;
                        }
                        else
                        {
                            skillScores.Status = EnumResultStatus.Process;
                        }
                    }
                    return skillScores;
                }).ToList();
                var mockTest = item.MockTest;
                if (mockTest != null)
                {
                    mockTestProgress.ObjectId = mockTest.Id;
                    mockTestProgress.Name = mockTest.Name;
                }
                var isDone = item.Status == EnumResultStatus.Done;
                mockTestProgress.ContentProgress = string.Format("{0} / {1}", isDone ? 1 : 0, 1);
                mockTestProgress.ProcessPercent = NumberHelper.ConvertPercentDouble(mockTestProgress.SkillScores.Average(x => x.CountQuestion / (x.TotalQuestion > 0 ? x.TotalQuestion : 1)));
                mockTestProgress.Scores = Math.Round(mockTestProgress.SkillScores.Average(x => x.Scores), 1);
                mockTestProgress.Status = item.Status;
                mockTestProgress.CorrectPercent = item.Percent;
                var featureAccessTime = featureAccessTimeTest?.FirstOrDefault(x => x.ObjectId == item.Id);
                if (featureAccessTime != null)
                {
                    mockTestProgress.TimeSpent = featureAccessTime.AccessTime;
                    mockTestProgress.LastVisited = featureAccessTime.LastVisited ?? null;
                }
                mockTestProgress.DisplayOrder = courseUnitMockTests.FirstOrDefault(x => x.MockTestId == item.MockTestId)?.DisplayOrder ?? default;
                mockTestProgress.Type = nameof(item.MockTest);
                mockTestProgress.TotalSkill = skillMockTest.Count;
                mockTestStudentProgress.Add(mockTestProgress);
            }

            methodResult.Result = mockTestStudentProgress;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
