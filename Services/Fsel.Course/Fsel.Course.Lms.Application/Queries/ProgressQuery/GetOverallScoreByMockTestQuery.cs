// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ProgressQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
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
        private readonly IUserService _userService;

        public GetOverallScoreByMockTestQueryHandler(AuthContext authContext
            , ICourseRepository courseRepository
            , IUserService userService)
        {
            _authContext = authContext;
            _courseRepository = courseRepository;
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
                                                            .ThenInclude(x => x!.MockTestResults)
                                                            .Include(x => x.CourseUnitMockTests)
                                                            .ThenInclude(x => x.Unit)
                                                            .ThenInclude(x => x!.MockTestResults)
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
            List<Domain.Entities.Unit> units = new List<Domain.Entities.Unit>();
            List<OverallScoreReportByMockTestModel> mockTestOverallScores = new List<OverallScoreReportByMockTestModel>();
            foreach (var item in course.CourseUnitMockTests)
            {
                if (item.Unit == null && units.Count > 0)
                {
                    OverallScoreReportByMockTestModel overallScoreReportByMockTest = new OverallScoreReportByMockTestModel
                    {
                        Name = "Unit 1 - Unit" + (item.DisplayOrder - 1),
                    };

                    var overallScoreReportSkills = units.Where(x => x.MockTestResults != null && x.MockTestResults.Count > 0).SelectMany(x => x.MockTestResults)
                        .Where(x => x.SkillScores != null && x.SkillScores.Any())
                        .Select(x => new OverallScoreReportSkillModel
                        {
                            Id = x.Id,
                            Percent = x.Percent,
                            Status = x.Status,
                            SkillScores = x.SkillScores,
                        }).ToList();
                    overallScoreReportByMockTest.Percent = overallScoreReportSkills.Any(x => x.Status == EnumResultStatus.Done) ? overallScoreReportSkills.Average(x => x.Percent) : default;
                    overallScoreReportByMockTest.OverallScoreReportSkills = overallScoreReportSkills;
                    mockTestOverallScores.Add(overallScoreReportByMockTest);
                    units.Clear();
                }
                else if (item.MockTest != null)
                {
                    mockTestOverallScores.Add(new OverallScoreReportByMockTestModel
                    {
                        Id = item.MockTestId ?? default,
                        Name = item.MockTest.Name,
                        OverallScoreReportSkills = item.MockTest.MockTestResults.Where(x => x.StudentId == studentId).Select(x => new OverallScoreReportSkillModel
                        {
                            Id = x.Id,
                            Percent = x.Percent,
                            SkillScores = x.SkillScores,
                            Status = x.Status
                        }).ToList(),
                        Percent = item.MockTest.MockTestResults.FirstOrDefault(x => x.StudentId == studentId)?.Percent ?? default,
                    });
                }
                else
                {
                    if (item.Unit != null && item.Unit.UnitSkillMockTests.Any())
                    {
                        units.Add(item.Unit);
                    }
                }
            }
            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = mockTestOverallScores;
            return methodResult;
        }
    }
}
