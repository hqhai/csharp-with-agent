// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ProgressQuery
{
    using System.Linq;
    using System.Threading;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetOverallScoreByMockTestQuery : IRequest<MethodResult<IList<OverallScoreReportByMockTestModel>>>
    {
        public Guid CourseId { get; set; }
    }

    public class GetOverallScoreByMockTestQueryHandler : IRequestHandler<GetOverallScoreByMockTestQuery, MethodResult<IList<OverallScoreReportByMockTestModel>>>
    {
        private readonly AuthContext _authContext;
        private readonly ICourseRepository _courseRepository;
        private readonly IMockTestRepository _mockTestRepository;
        private readonly IUnitRepository _unitRepository;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private const int TotalSkillFullMockTest = 4;

        public GetOverallScoreByMockTestQueryHandler(AuthContext authContext
            , ICourseRepository courseRepository
            , IMockTestRepository mockTestRepository
            , IUnitRepository unitRepository
            , IMapper mapper
            , IUserService userService)
        {
            _authContext = authContext;
            _courseRepository = courseRepository;
            _mockTestRepository = mockTestRepository;
            _unitRepository = unitRepository;
            _mapper = mapper;
            _userService = userService;
        }

        public async Task<MethodResult<IList<OverallScoreReportByMockTestModel>>> Handle(GetOverallScoreByMockTestQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<OverallScoreReportByMockTestModel>> methodResult = new MethodResult<IList<OverallScoreReportByMockTestModel>>();

            var studentResult = await _userService.GetStudentByUserIdWithCacheAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentResult));
                return methodResult;
            }
            var student = studentResult?.Content?.Result;
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }
            var studentId = student.Id;
            var course = await _courseRepository.ReadQueryable.Include(x => x.CourseUnitMockTests.OrderBy(x => x.DisplayOrder))
                                                            .Where(x => x.Id == request.CourseId)
                                                            .AsNoTracking()
                                                            .FirstOrDefaultAsync(cancellationToken);
            if (course == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentResult));
                return methodResult;
            }
            if (course.CourseType != EnumCourseType.Ielts)
            {
                methodResult.AddErrorBadRequest(nameof(EnumCourseErrorCode.CourseNotTypeIElts));
                return methodResult;
            }
            var mockTestIds = course.CourseUnitMockTests.Where(x => x.MockTestId.HasValue).Select(x => x.MockTestId!.Value).ToList();

            var mockTests = await _mockTestRepository.ReadQueryable.Include(x => x.MockTestResults.Where(x => x.CourseId == course.Id && x.StudentId == studentId))
                                                                    .ThenInclude(x => x.SectionGroupResults.Where(x => x.StudentId == studentId))
                                                               .Include(x => x.MockTestResults.Where(x => x.CourseId == course.Id && x.StudentId == studentId))
                                                                    .ThenInclude(x => x.MockTestScores)
                                                               .Where(x => mockTestIds.Contains(x.Id))
                                                               .ToListAsync(cancellationToken);

            var courseUnitMockTests = course.CourseUnitMockTests.OrderBy(x => x.DisplayOrder).ToList();
            var units = await GetUnitsAsync(courseUnitMockTests);
            var courseMockTests = courseUnitMockTests.Where(x => x.MockTestId.HasValue).ToList();
            List<OverallScoreReportByMockTestModel> mockTestOverallScores = new List<OverallScoreReportByMockTestModel>();
            foreach (var item in courseMockTests)
            {
                mockTestOverallScores.Add(await GetOverallScoreMockTestByUnit(item, courseUnitMockTests, units, studentId));
                if (item.MockTestId.HasValue)
                {
                    var mockTest = mockTests.FirstOrDefault(x => x.Id == item.MockTestId.Value);
                    mockTestOverallScores.Add(GetOverallScoreReport(mockTest));
                }
            }
            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = mockTestOverallScores;
            return methodResult;
        }

        private OverallScoreReportByMockTestModel GetOverallScoreReport(MockTest? mockTest)
        {
            if (mockTest == null)
            {
                return new OverallScoreReportByMockTestModel();
            }
            var mockTestDto = new OverallScoreReportByMockTestModel
            {
                Id = mockTest.Id,
                Name = mockTest.Name,
                Status = mockTest.MockTestResults.Select(x => x.Status).FirstOrDefault(),
                OverallScoreReportSkills = mockTest.MockTestResults.Select(x => new OverallScoreReportSkillModel
                {
                    Id = x.Id,
                    Percent = NumberHelper.GetPercent((x.SkillScores?.Count ?? default), TotalSkillFullMockTest),
                    Score = NumberHelper.RoundNumberDouble(x.SkillScores?.Average(x => x.Scores) ?? default),
                    SkillScores = GetTestSkillScores(x, mockTest),
                    Status = x.Status
                }).ToList(),
                Percent = NumberHelper.GetPercent(mockTest.MockTestResults.SelectMany(x => x.SectionGroupResults).Count(x => x.Status == EnumResultStatus.Done), TotalSkillFullMockTest),
            };

            return mockTestDto;
        }

        private async Task<IList<Domain.Entities.Unit>> GetUnitsAsync(IList<CourseUnitMockTest>? courseUnitMockTests)
        {
            ArgumentNullException.ThrowIfNull(courseUnitMockTests);
            var unitIds = courseUnitMockTests.Where(x => x.UnitId.HasValue).Select(x => x.UnitId!.Value).ToList();
            return await _unitRepository.ReadQueryable.Include(x => x.UnitSkillMockTests).Where(x => unitIds.Contains(x.Id)).ToListAsync();
        }

        private static (IList<Guid>, IList<CourseUnitMockTest>) GetMockTestIds(CourseUnitMockTest? courseUnitMockTest, IList<CourseUnitMockTest> courseUnitMockTests, IList<Domain.Entities.Unit> units)
        {
            ArgumentNullException.ThrowIfNull(courseUnitMockTest);
            var courseMockTests = courseUnitMockTests.Where(x => x.MockTestId.HasValue).OrderBy(x => x.DisplayOrder).ToList();

            var displayOrder = courseUnitMockTests[courseUnitMockTests.IndexOf(courseUnitMockTest)]?.DisplayOrder;

            var displayOrderPrevious = courseMockTests.IndexOf(courseUnitMockTest) > 0 ? courseMockTests[courseMockTests.IndexOf(courseUnitMockTest) - 1].DisplayOrder : default;

            var courseUnits = courseUnitMockTests.Where(x => x.DisplayOrder < displayOrder && x.UnitId.HasValue && (displayOrderPrevious == 0 || x.DisplayOrder > displayOrderPrevious)).ToList();
            var unitIds = courseUnits.OrderBy(x => x.DisplayOrder).Select(x => x.UnitId!.Value).ToList();
            var unitMockTests = units.Where(x => unitIds.Contains(x.Id)).OrderBy(x => unitIds.IndexOf(x.Id)).ToList();
            return (unitMockTests.SelectMany(x => x.UnitSkillMockTests).Select(x => x.MockTestId).ToList(), courseUnits);
        }

        private async Task<OverallScoreReportByMockTestModel> GetOverallScoreMockTestByUnit(CourseUnitMockTest? courseUnitMockTest, IList<CourseUnitMockTest> courseUnitMockTests, IList<Domain.Entities.Unit> units, Guid? studentId)
        {
            ArgumentNullException.ThrowIfNull(courseUnitMockTest);
            var (mockTestIds, courseUnits) = GetMockTestIds(courseUnitMockTest, courseUnitMockTests, units);
            OverallScoreReportByMockTestModel overallScoreReportByMockTest = new OverallScoreReportByMockTestModel
            {
                Name = GetName(courseUnits)
            };
            var unitIds = courseUnits.Where(x => x.UnitId.HasValue).Select(x => x.UnitId!.Value).ToList();
            if (mockTestIds.Any())
            {
                var mockTests = await _mockTestRepository.ReadQueryable.Include(x => x.MockTestSections)
                                                        .ThenInclude(x => x.SectionGroup)
                                                        .Include(x => x.MockTestResults.Where(x => x.StudentId == studentId && x.CourseId == courseUnitMockTest.CourseId && x.UnitId.HasValue && unitIds.Contains(x.UnitId.Value)))
                                                        .ThenInclude(x => x.MockTestScores)
                                                        .Where(x => mockTestIds.Contains(x.Id)).ToListAsync();
                mockTests = mockTests.OrderBy(x => mockTestIds.IndexOf(x.Id)).ToList();
                var overallScoreReportSkills = mockTests.Select(x => GetOverallScoreReportSkill(x, unitIds, studentId)).ToList();
                overallScoreReportByMockTest.Percent = NumberHelper.GetPercent(overallScoreReportSkills.Count(x => x.Status == EnumResultStatus.Done), overallScoreReportSkills.Count);
                overallScoreReportByMockTest.OverallScoreReportSkills = overallScoreReportSkills;
                overallScoreReportByMockTest.Status = GetStatus(overallScoreReportSkills);
            }
            return overallScoreReportByMockTest;
        }

        private static EnumResultStatus GetStatus(IList<OverallScoreReportSkillModel>? overallScoreReportSkills)
        {
            var status = EnumResultStatus.Unfinished;
            if (overallScoreReportSkills != null && overallScoreReportSkills.Any())
            {
                if (overallScoreReportSkills.Any(x => x.Status != EnumResultStatus.Unfinished))
                {
                    status = EnumResultStatus.Process;
                }
                if (overallScoreReportSkills.All(x => x.Status == EnumResultStatus.Done))
                {
                    status = EnumResultStatus.Done;
                }
            }
            return status;
        }

        private OverallScoreReportSkillModel GetOverallScoreReportSkill(MockTest? mockTest, IList<Guid>? unitIds, Guid? studentId)
        {
            ArgumentNullException.ThrowIfNull(mockTest);
            var mockTestResult = mockTest.MockTestResults.FirstOrDefault(x => unitIds != null && x.UnitId.HasValue && x.StudentId == studentId && unitIds.Contains(x.UnitId.Value));
            return new OverallScoreReportSkillModel
            {
                Id = mockTestResult?.Id ?? null,
                Percent = NumberHelper.ConvertPercentDouble(mockTestResult?.Status == EnumResultStatus.Done ? 1 : 0),
                SkillScores = GetTestSkillScores(mockTestResult, mockTest),
                Score = mockTestResult?.SkillScores?.Select(x => x.Scores).FirstOrDefault() ?? default,
                Status = mockTestResult?.Status ?? EnumResultStatus.Unfinished,
            };
        }

        public TestSkillScores GetTestSkillScore(SkillScores? skillScores, MockTestResult? mockTestResult)
        {
            if (skillScores != null && mockTestResult != null)
            {
                var testSkillScore = _mapper.Map<TestSkillScores>(skillScores);
                testSkillScore.Status = mockTestResult.Status;
                if (skillScores.Skill == EnumCourseSkill.Speaking && mockTestResult.Status != EnumResultStatus.Unfinished)
                {
                    testSkillScore.Status = mockTestResult.MockTestScores.Any() ? EnumResultStatus.Done : EnumResultStatus.Process;
                }
                return testSkillScore;
            }
            return new TestSkillScores();
        }

        private IList<TestSkillScores>? GetTestSkillScores(MockTestResult? mockTestResult, MockTest mockTest)
        {
            var skillScores = mockTestResult?.SkillScores;
            if (skillScores != null && skillScores.Any())
            {
                return skillScores.Select(x => GetTestSkillScore(x, mockTestResult)).ToList();
            }
            return new List<TestSkillScores> { GetTestSkillScore(mockTest, mockTestResult) };
        }

        private static TestSkillScores GetTestSkillScore(MockTest mockTest, MockTestResult? mockTestResult)
        {
            return new TestSkillScores { Skill = mockTest.MockTestSections.Select(x => x.SectionGroup).Select(x => x!.CourseSkill).FirstOrDefault(), Status = mockTestResult?.Status ?? EnumResultStatus.Unfinished };
        }

        private static string GetName(IList<CourseUnitMockTest> courseUnits)
        {
            if (courseUnits.Count > 1)
            {
                return string.Format("Final result (Unit {0} - Unit{1})", courseUnits[0].Number, courseUnits[courseUnits.Count - 1].Number, StringComparison.Ordinal);
            }
            else if (courseUnits.Any())
            {
                return string.Format("Final result ( Unit {0} )", courseUnits[0].Number, StringComparison.Ordinal);
            }
            return string.Empty;
        }
    }
}
