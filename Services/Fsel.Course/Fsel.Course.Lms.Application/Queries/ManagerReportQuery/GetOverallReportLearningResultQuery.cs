// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ManagerReportQuery
{
    using System.Linq;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels.ManagerReportModels;
    using Fsel.Course.Domain.Models.QueryModels.ManagerReports;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.ShareModels.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using static Fsel.Shared.Constants.ValueSettings;

    public class GetOverallReportLearningResultQuery : SearchReportLearningResultQueryModel, IRequest<MethodResult<OverallReportLearningResultModel>>
    {
    }

    public class GetOverallReportLearningResultQueryHandler : IRequestHandler<GetOverallReportLearningResultQuery, MethodResult<OverallReportLearningResultModel>>
    {
        private readonly IMediator _mediator;
        private readonly IFinalTestResultRepository _finalTestResultRepository;
        private readonly IUnitResultRepository _unitResultRepository;
        private readonly ICourseUnitMockTestRepository _courseUnitMockTestRepository;
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly ICourseResultRepository _courseResultRepository;

        public GetOverallReportLearningResultQueryHandler(IMediator mediator,
            IFinalTestResultRepository finalTestResultRepository,
            IUnitResultRepository unitResultRepository,
            ICourseUnitMockTestRepository courseUnitMockTestRepository,
            IMockTestResultRepository mockTestResultRepository,
            ICourseResultRepository courseResultRepository)
        {
            _mediator = mediator;
            _finalTestResultRepository = finalTestResultRepository;
            _unitResultRepository = unitResultRepository;
            _courseUnitMockTestRepository = courseUnitMockTestRepository;
            _mockTestResultRepository = mockTestResultRepository;
            _courseResultRepository = courseResultRepository;
        }

        public async Task<MethodResult<OverallReportLearningResultModel>> Handle(GetOverallReportLearningResultQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<OverallReportLearningResultModel>();

            var userResults = await _mediator.Send(new GetStudentReportQuery
            {
                ListDistrict = request.ListDistrict,
                ListProvince = request.ListProvince,
                ListSchool = request.ListSchool,
                SchoolClass = request.SchoolClass,
                SchoolGrade = request.SchoolGrade,
                EndDate = request.EndDate,
                Keyword = request.Keyword,
                LearningStatus = request.LearningStatus,
                CourseType = request.CourseType,
                CourseLevel = request.CourseLevel,
                ManagerReportType = EnumManagerReportType.ReportLearningResults,
            }, cancellationToken);
            if (!userResults.IsOK)
            {
                methodResult.AddError(userResults.ErrorMessages);
                return methodResult;
            }
            var students = userResults?.Result?.ToList() ?? new List<StudentDtoModel>();
            var studentIds = students.Select(x => x.Id).ToList();
            var courseLevels = EnumCourseLevelHelper.GetEnumCourseLevels(request.CourseType);
            var overallReport = new OverallReportLearningResultModel
            {
                TotalStudent = students.Count,
                CourseLevelProgresses = courseLevels.Select(x => new CourseLevelProgressModel
                {
                    CourseLevel = x,
                    TotalStudent = students.Count(y => y.CourseLevel == x)
                }).ToList()
            };
            await GetOverallReportAsync(overallReport, request.CourseType, students);
            if (request.CourseType == EnumCourseType.Ielts)
            {
                await GetOverallReportProgressAsync(overallReport, courseLevels, students);
            }
            methodResult.Result = overallReport;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task GetOverallReportProgressAsync(OverallReportLearningResultModel overallReport, IList<EnumCourseLevel> courseLevels, List<StudentDtoModel> students)
        {
            var courseIds = students.Select(x => x.CourseId).ToList();
            var studentIds = students.Select(x => x.Id).ToList();
            var dataStudent = students.Select(x => new { StudentId = x.Id, CourseId = x.CourseId.GetValueOrDefault() }).ToList();

            var query = await _mockTestResultRepository.Queryable
                                  .Where(x => studentIds.Contains(x.StudentId) && courseIds.Contains(x.CourseId))
                                  .Where(x => x.Status == EnumResultStatus.Done && !x.UnitId.HasValue)
                                  .ToListAsync();

            var mockTestGroups = await _courseUnitMockTestRepository.Queryable
                                       .Where(x => courseIds != null && courseIds.Contains(x.CourseId) && x.MockTestId.HasValue)
                                       .GroupBy(x => new { x.Number, x.Course!.CourseLevel })
                                       .Select(x => new
                                       {
                                           CourseLevel = x.Key.CourseLevel,
                                           Number = x.Key.Number,
                                           MockTestIds = x.Where(u => u.MockTestId.HasValue).Select(u => u.MockTestId.GetValueOrDefault()).ToList()
                                       })
                                       .ToListAsync();
            foreach (var item in courseLevels)
            {
                var courseLevelProgress = overallReport.CourseLevelProgresses?.FirstOrDefault(x => x.CourseLevel == item);
                if (courseLevelProgress == null)
                {
                    continue;
                }
                var mockTestModules = Enumerable.Range(1, CourseProgressValue.CountFullMockTest).Select(i =>
                {
                    var mockTestIds = mockTestGroups.Where(x => x.CourseLevel == item).Where(x => x.Number == i).SelectMany(x => x.MockTestIds).ToList();
                    var mockTestResults = query.Join(dataStudent,
                                              unitResult => new { unitResult.CourseId, unitResult.StudentId },
                                              student => new { student.CourseId, student.StudentId },
                                              (unitResult, student) => unitResult)
                                              .Where(x => mockTestIds.Contains(x.MockTestId)).ToList();
                    var resultScores = mockTestResults.Select(x =>
                    {
                        return x.SkillScores != null && x.SkillScores.Any() ? NumberHelper.RoundNumberDouble(x.SkillScores.Average(x => x.Scores)) : ValueDefault;
                    }).ToList();
                    return new OverallTestResultModel
                    {
                        Index = i,
                        TotalStudent = resultScores.Count,
                        Score = !resultScores.Any() ? ValueDefault : NumberHelper.RoundNumberDouble(resultScores.Average()),
                        Type = nameof(MockTest)
                    };
                });

                courseLevelProgress.OverallTestResults = mockTestModules.ToList();
            }
        }

        private async Task GetOverallReportAsync(OverallReportLearningResultModel overallReport, EnumCourseType courseType, List<StudentDtoModel> students)
        {
            var overallModules = new List<OverallModuleReportModel>();
            var courseIds = students.Select(x => x.CourseId).Distinct().ToList();
            var studentIds = students.Select(x => x.Id).ToList();
            var data = students.Select(x => new { StudentId = x.Id, CourseId = x.CourseId.GetValueOrDefault() }).ToList();

            var query = await _unitResultRepository.Queryable.Where(x => studentIds.Contains(x.StudentId) && courseIds.Contains(x.CourseId) && x.Status == EnumResultStatus.Done)
                                                             .ToListAsync();
            query = query
                .Join(data,
                  unitResult => new { unitResult.CourseId, unitResult.StudentId },
                  student => new { student.CourseId, student.StudentId },
                  (unitResult, student) => unitResult)
                .ToList();
            var unitGroups = await _courseUnitMockTestRepository.Queryable
                                       .Where(x => courseIds != null && courseIds.Contains(x.CourseId) && x.UnitId.HasValue)
                                       .GroupBy(x => x.Number)
                                       .Select(x => new
                                       {
                                           Number = x.Key,
                                           UnitIds = x.Where(u => u.UnitId.HasValue).Select(u => u.UnitId.GetValueOrDefault()).Distinct().ToList()
                                       })
                                       .ToListAsync();
            var countUnit = courseType == EnumCourseType.Academic ? CourseProgressValue.CountUnitAca : courseType == EnumCourseType.Ielts ? CourseProgressValue.CountUnitIELTS : ValueDefault;

            var unitModules = Enumerable.Range(1, countUnit).Select(i =>
            {
                var unitIds = unitGroups.Where(x => x.Number == i).SelectMany(x => x.UnitIds).ToList();
                var resultPercents = query.Where(x => unitIds.Contains(x.UnitId)).Select(x => x.Percent).ToList();
                return new OverallModuleReportModel
                {
                    Percent = !resultPercents.Any() ? ValueDefault : NumberHelper.ConvertRound(resultPercents.Average()),
                    Index = i,
                    TotalStudent = resultPercents.Count,
                    Type = nameof(Domain.Entities.Unit)
                };
            });
            // Calculate FinalTest Results only for Aca type
            if (courseType == EnumCourseType.Academic)
            {
                var finalTestResults = await _finalTestResultRepository.Queryable.Where(x => studentIds.Contains(x.StudentId) && courseIds.Contains(x.CourseId))
                                                   .Where(x => x.Status == EnumResultStatus.Done)
                                                   .ToListAsync();
                finalTestResults = finalTestResults
                .Join(data,
                  finalTestResult => new { finalTestResult.CourseId, finalTestResult.StudentId },
                  student => new { student.CourseId, student.StudentId },
                  (unitResult, student) => unitResult)
                .ToList();
                overallModules.Add(new OverallModuleReportModel
                {
                    Percent = !finalTestResults.Any() ? ValueDefault : NumberHelper.ConvertRound(finalTestResults.Average(x => x.Percent)),
                    Index = ValueDefault,
                    TotalStudent = finalTestResults.Count,
                    Type = nameof(FinalTest)
                });
                overallReport.OverallAvgPercentFinal = !finalTestResults.Any() ? ValueDefault : NumberHelper.ConvertRound(finalTestResults.Average(x => x.Percent));
            }

            overallModules.AddRange(unitModules);
            overallReport.OverallModules = overallModules;
            overallReport.OverallAvgPercent = query.Any() ? NumberHelper.ConvertRound(query.Average(x => x.Percent)) : ValueDefault;
        }
    }
}
