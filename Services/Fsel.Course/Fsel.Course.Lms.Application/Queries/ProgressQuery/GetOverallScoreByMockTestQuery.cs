// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ProgressQuery
{
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
        private readonly IUserService _userService;

        public GetOverallScoreByMockTestQueryHandler(AuthContext authContext
            , ICourseRepository courseRepository
            , IMockTestRepository mockTestRepository
            , IUnitRepository unitRepository
            , IUserService userService)
        {
            _authContext = authContext;
            _courseRepository = courseRepository;
            _mockTestRepository = mockTestRepository;
            _unitRepository = unitRepository;
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
                                                            .Include(x => x.CourseUnitMockTests)
                                                            .Where(x => x.Id == request.CourseId)
                                                            .AsNoTracking()
                                                            .FirstOrDefaultAsync(cancellationToken);
            if (course == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentResult));
                return methodResult;
            }

            if (course.CourseLevel.GetEnumCourseType() != EnumCourseType.Ielts)
            {
                methodResult.AddErrorBadRequest(nameof(EnumCourseErrorCode.CourseNotTypeIElts));
                return methodResult;
            }
            var courseUnitMockTests = course.CourseUnitMockTests.OrderBy(x => x.DisplayOrder).ToList();
            var unitIds = courseUnitMockTests.Where(x => x.UnitId.HasValue).Select(x => x.UnitId!.Value).ToList();
            var units = await _unitRepository.Queryable.Include(x => x.UnitSkillMockTests)
                                                        .Where(x => unitIds.Contains(x.Id))
                                                        .ToListAsync(cancellationToken);
            var courseMockTests = courseUnitMockTests.Where(x => x.MockTestId.HasValue).ToList();
            List<OverallScoreReportByMockTestModel> mockTestOverallScores = new List<OverallScoreReportByMockTestModel>();
            foreach (var item in courseMockTests)
            {
                mockTestOverallScores.Add(await GetOverallScoreMockTestByUnit(item, courseUnitMockTests, units, studentId));
                if (item.MockTest != null)
                {
                    mockTestOverallScores.Add(new OverallScoreReportByMockTestModel
                    {
                        Id = item.MockTestId ?? default,
                        Name = item.MockTest.Name,
                        OverallScoreReportSkills = item.MockTest.MockTestResults.Where(x => x.StudentId == studentId && x.MockTestId == item.MockTestId).Select(x => new OverallScoreReportSkillModel
                        {
                            Id = x.Id,
                            Percent = x.Percent,
                            SkillScores = x.SkillScores,
                            Status = x.Status
                        }).ToList(),
                        Percent = item.MockTest.MockTestResults.FirstOrDefault(x => x.StudentId == studentId && x.MockTestId == item.MockTestId)?.Percent ?? default,
                    });
                }
            }
            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = mockTestOverallScores;
            return methodResult;
        }

        private async Task<OverallScoreReportByMockTestModel> GetOverallScoreMockTestByUnit(CourseUnitMockTest? courseUnitMockTest, IList<CourseUnitMockTest> courseUnitMockTests, IList<Domain.Entities.Unit> units, Guid? studentId)
        {
            ArgumentNullException.ThrowIfNull(courseUnitMockTest);
            var displayOrder = courseUnitMockTests[courseUnitMockTests.IndexOf(courseUnitMockTest)].DisplayOrder;
            var courseUnits = courseUnitMockTests.Where(x => x.DisplayOrder < displayOrder && x.UnitId.HasValue).ToList();
            var unitIds = courseUnits.Select(x => x.UnitId!.Value).ToList();
            var unitMockTests = units.Where(x => unitIds.Contains(x.Id)).ToList();
            var mockTestIds = unitMockTests.SelectMany(x => x.UnitSkillMockTests).Select(x => x.MockTestId).ToList();
            OverallScoreReportByMockTestModel overallScoreReportByMockTest = new OverallScoreReportByMockTestModel
            {
                Name = GetName(courseUnits)
            };
            if (mockTestIds.Any())
            {
                var mockTests = await _mockTestRepository.Queryable.Include(x => x.MockTestSections)
                                                                    .ThenInclude(x => x.SectionGroup)
                                                                    .Include(x => x.MockTestResults.Where(x => x.StudentId == studentId && x.UnitId.HasValue))
                                                                    .Where(x => mockTestIds.Contains(x.Id)).ToListAsync();

                var overallScoreReportSkills = mockTests.Select(x => GetOverallScoreReportSkill(x, studentId)).ToList();
                overallScoreReportByMockTest.Percent = overallScoreReportSkills.Any(x => x.Status == EnumResultStatus.Done) ? NumberHelper.ConvertDoubleDecimal(overallScoreReportSkills.Average(x => x.Percent)) : default;
                overallScoreReportByMockTest.OverallScoreReportSkills = overallScoreReportSkills;
            }
            return overallScoreReportByMockTest;
        }

        private static OverallScoreReportSkillModel GetOverallScoreReportSkill(MockTest? mockTest, Guid? studentId)
        {
            ArgumentNullException.ThrowIfNull(mockTest);
            var mockTestResult = mockTest.MockTestResults.FirstOrDefault(x => x.StudentId == studentId);
            return new OverallScoreReportSkillModel
            {
                Id = mockTestResult?.Id ?? null,
                Percent = mockTestResult?.Percent ?? default,
                SkillScores = mockTestResult?.SkillScores ?? new List<SkillScores> {
                        new SkillScores { Skill = mockTest.MockTestSections.FirstOrDefault()!.SectionGroup!.CourseSkill },
                },
                Status = mockTestResult?.Status ?? EnumResultStatus.Unfinished,
            };
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
