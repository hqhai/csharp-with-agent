// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ProgressQuery
{
    using System.Collections;
    using System.Linq;
    using System.Threading;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
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

            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentResult));
                return methodResult;
            }
            var studentId = studentResult?.Content?.Result?.Id;
            var course = await _courseRepository.Queryable.Include(x => x.CourseUnitMockTests)
                                                            .ThenInclude(x => x.MockTest)
                                                            .ThenInclude(x => x!.MockTestResults.Where(x => x.StudentId == studentId && !x.UnitId.HasValue))
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
            var courseUnitMockTests = course.CourseUnitMockTests.OrderBy(x => x.DisplayOrder).ToList();
            var units = await GetUnitsAsync(courseUnitMockTests);
            var courseMockTests = courseUnitMockTests.Where(x => x.MockTestId.HasValue).ToList();
            List<OverallScoreReportByMockTestModel> mockTestOverallScores = new List<OverallScoreReportByMockTestModel>();
            foreach (var item in courseMockTests)
            {
                mockTestOverallScores.Add(await GetOverallScoreMockTestByUnit(item, courseUnitMockTests, units, studentId));
                if (item.MockTest != null)
                {
                    mockTestOverallScores.Add(GetOverallScoreReport(item.MockTest));
                }
            }
            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = mockTestOverallScores;
            return methodResult;
        }

        private OverallScoreReportByMockTestModel GetOverallScoreReport(MockTest mockTest)
        {
            return new OverallScoreReportByMockTestModel
            {
                Id = mockTest.Id,
                Name = mockTest.Name,
                Status = GetStatus(mockTest.MockTestResults),
                OverallScoreReportSkills = mockTest.MockTestResults.Select(x => new OverallScoreReportSkillModel
                {
                    Id = x.Id,
                    Percent = x.Percent,
                    SkillScores = GetTestSkillScores(x, mockTest),
                    Status = x.Status
                }).ToList(),
                Percent = mockTest.MockTestResults.FirstOrDefault()?.Percent ?? default,
            };
        }

        private async Task<IList<Domain.Entities.Unit>> GetUnitsAsync(IList<CourseUnitMockTest>? courseUnitMockTests)
        {
            ArgumentNullException.ThrowIfNull(courseUnitMockTests);
            var unitIds = courseUnitMockTests.Where(x => x.UnitId.HasValue).Select(x => x.UnitId!.Value).ToList();
            return await _unitRepository.Queryable.Include(x => x.UnitSkillMockTests).Where(x => unitIds.Contains(x.Id)).ToListAsync();
        }

        private async Task<OverallScoreReportByMockTestModel> GetOverallScoreMockTestByUnit(CourseUnitMockTest? courseUnitMockTest, IList<CourseUnitMockTest> courseUnitMockTests, IList<Domain.Entities.Unit> units, Guid? studentId)
        {
            ArgumentNullException.ThrowIfNull(courseUnitMockTest);
            var displayOrder = courseUnitMockTests[courseUnitMockTests.IndexOf(courseUnitMockTest)].DisplayOrder;
            var courseUnits = courseUnitMockTests.Where(x => x.DisplayOrder < displayOrder && x.UnitId.HasValue).ToList();
            var unitIds = courseUnits.OrderBy(x => x.DisplayOrder).Select(x => x.UnitId!.Value).ToList();
            var unitMockTests = units.Where(x => unitIds.Contains(x.Id)).OrderBy(x => unitIds.IndexOf(x.Id)).ToList();
            var mockTestIds = unitMockTests.SelectMany(x => x.UnitSkillMockTests).Select(x => x.MockTestId).ToList();
            OverallScoreReportByMockTestModel overallScoreReportByMockTest = new OverallScoreReportByMockTestModel
            {
                Name = GetName(courseUnits)
            };
            if (mockTestIds.Any())
            {
                var mockTests = await _mockTestRepository.Queryable.Include(x => x.MockTestSections)
                                                                    .ThenInclude(x => x.SectionGroup)
                                                                    .Include(x => x.MockTestResults.Where(x => x.StudentId == studentId && x.CourseId == courseUnitMockTest.CourseId && x.UnitId.HasValue))
                                                                    .ThenInclude(x => x.MockTestScores)
                                                                    .Where(x => mockTestIds.Contains(x.Id)).ToListAsync();
                mockTests = mockTests.OrderBy(x => mockTestIds.IndexOf(x.Id)).ToList();
                var overallScoreReportSkills = mockTests.Select(x => GetOverallScoreReportSkill(x, studentId)).ToList();
                overallScoreReportByMockTest.Percent = NumberHelper.GetPercent(overallScoreReportSkills.Count(x => x.Status == EnumResultStatus.Done), overallScoreReportSkills.Count);
                overallScoreReportByMockTest.OverallScoreReportSkills = overallScoreReportSkills;
                overallScoreReportByMockTest.Status = GetStatus(overallScoreReportSkills);
            }
            return overallScoreReportByMockTest;
        }

        private OverallScoreReportSkillModel GetOverallScoreReportSkill(MockTest? mockTest, Guid? studentId)
        {
            ArgumentNullException.ThrowIfNull(mockTest);
            var mockTestResult = mockTest.MockTestResults.FirstOrDefault(x => x.StudentId == studentId);
            return new OverallScoreReportSkillModel
            {
                Id = mockTestResult?.Id ?? null,
                Percent = NumberHelper.ConvertPercentDouble(mockTestResult?.Status == EnumResultStatus.Done ? 1 : 0),
                SkillScores = GetTestSkillScores(mockTestResult, mockTest),
                Status = mockTestResult?.Status ?? EnumResultStatus.Unfinished,
            };
        }

        private IList<TestSkillScores>? GetTestSkillScores(MockTestResult? mockTestResult, MockTest mockTest)
        {
            if (mockTestResult != null && mockTestResult.SkillScores != null && mockTestResult.SkillScores.Any())
            {
                var skillScores = mockTestResult.SkillScores;
                return mockTestResult.SkillScores?.Select(x => GetTestSkillScore(x, mockTestResult)).ToList();
            }
            return new List<TestSkillScores> { GetTestSkillScore(mockTest, mockTestResult) };
        }

        private static TestSkillScores GetTestSkillScore(MockTest mockTest, MockTestResult? mockTestResult)
        {
            return new TestSkillScores { Skill = mockTest.MockTestSections.Select(x => x.SectionGroup).Select(x => x!.CourseSkill).FirstOrDefault(), Status = mockTestResult?.Status ?? EnumResultStatus.Unfinished };
        }

        public TestSkillScores GetTestSkillScore(SkillScores? skillScores, MockTestResult? mockTestResult)
        {
            if (skillScores != null && mockTestResult != null)
            {
                var testSkillScore = _mapper.Map<TestSkillScores>(skillScores);

                if ((skillScores.Skill == EnumCourseSkill.Speaking || skillScores.Skill == EnumCourseSkill.Writing) && mockTestResult.Status != EnumResultStatus.Unfinished)
                {
                    if (mockTestResult.MockTestScores.Any())
                    {
                        testSkillScore.Status = EnumResultStatus.Done;
                    }
                    else
                    {
                        testSkillScore.Status = EnumResultStatus.Process;
                    }
                }
                else
                {
                    testSkillScore.Status = mockTestResult.Status;
                }
                return testSkillScore;
            }
            return new TestSkillScores();
        }

        private static EnumResultStatus GetStatus(object? data)
        {
            var status = EnumResultStatus.Unfinished;
            if (data is IList list)
            {
                List<object>? datas = list.Cast<object>().ToList();
                if (datas != null && datas.Any())
                {
                    if (datas.Any(x => (EnumResultStatus)x.GetPropValue("Status") != EnumResultStatus.Unfinished))
                    {
                        status = EnumResultStatus.Process;
                    }
                    if (datas.All(x => (EnumResultStatus)x.GetPropValue("Status") == EnumResultStatus.Done))
                    {
                        status = EnumResultStatus.Done;
                    }
                }
            }
            return status;
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
