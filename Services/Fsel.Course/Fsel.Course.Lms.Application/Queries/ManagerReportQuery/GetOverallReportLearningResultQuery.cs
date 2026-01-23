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
        private readonly ICourseRepository _courseRepository;
        private readonly ICourseResultRepository _courseResultRepository;

        public GetOverallReportLearningResultQueryHandler(IMediator mediator,
            IFinalTestResultRepository finalTestResultRepository,
            IUnitResultRepository unitResultRepository,
            ICourseUnitMockTestRepository courseUnitMockTestRepository,
            IMockTestResultRepository mockTestResultRepository,
            ICourseRepository courseRepository,
            ICourseResultRepository courseResultRepository)
        {
            _mediator = mediator;
            _finalTestResultRepository = finalTestResultRepository;
            _unitResultRepository = unitResultRepository;
            _courseUnitMockTestRepository = courseUnitMockTestRepository;
            _mockTestResultRepository = mockTestResultRepository;
            _courseRepository = courseRepository;
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
                ListSchoolClass = request.ListSchoolClass,
                ListSchoolGrade = request.ListSchoolGrade,
                ListCourseLevel = request.ListCourseLevel,
                IsLearning = request.IsLearning,
                ListCompletionStatus = request.ListCompletionStatus,
                ListCurrentLevel = request.ListCurrentLevel,
                ListOverallScore = request.ListOverallScore,
                ListLearningStatus = request.ListLearningStatus,

                EndDate = request.EndDate,
                Keyword = request.Keyword,
                CourseType = request.CourseType,
                ManagerReportType = EnumManagerReportType.ReportLearningResults,
            }, cancellationToken);
            if (!userResults.IsOK)
            {
                methodResult.AddError(userResults.ErrorMessages);
                return methodResult;
            }
            var students = userResults?.Result?.ToList() ?? new List<StudentDtoModel>();

            methodResult.Result = await GetOverallReportAsync(request, students);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<OverallReportLearningResultModel> GetOverallReportAsync(GetOverallReportLearningResultQuery request, List<StudentDtoModel> students)
        {
            var courseLevels = EnumCourseLevelHelper.GetEnumCourseLevels(request.CourseType);
            var overallModules = new List<OverallModuleReportModel>();

            (var unitModules, students, var overallAvgPercent) = await GetUnitResultGroups(request, students, courseLevels);
            var studentIds = students.Select(x => x.Id).ToList();
            var courseIds = students.Select(x => x.CourseId).Distinct().ToList();

            var overallReport = new OverallReportLearningResultModel
            {
                TotalStudent = students.Count,
                CourseLevelProgresses = BuildCourseLevelProgress(courseLevels, students)
            };

            if (request.CourseType == EnumCourseType.Academic || request.CourseType == EnumCourseType.EnglishFoundation)
            {
                var finalTestResults = await (from baseQ in _courseResultRepository.Queryable.WhereBulkContains(studentIds, x => x.StudentId)
                                              join cum in _courseUnitMockTestRepository.Queryable on baseQ.CourseId equals cum.CourseId
                                              join ftr in _finalTestResultRepository.Queryable on new { baseQ.StudentId, baseQ.CourseId, FinalTestId = cum.FinalTestId } equals new { ftr.StudentId, ftr.CourseId, FinalTestId = (Guid?)ftr.FinalTestId } into finalTestGroup
                                              from ftr in finalTestGroup.DefaultIfEmpty()
                                              where baseQ.WorkingStatus == EnumWorkingStatus.Active &&
                                              (!request.EndDate.HasValue || (ftr.UpdatedDate ?? ftr.CreatedDate).Date <= request.EndDate.Value.Date) && ftr.Status == EnumResultStatus.Done
                                              select new FinalTestResult
                                              {
                                                  Percent = ftr.Percent,
                                                  CorrectCount = ftr.CorrectCount,
                                                  CorrectTotal = ftr.CorrectTotal,
                                              }).ToListAsync();
                var overallPercentFinalTest = finalTestResults.Any() ? NumberHelper.ConvertRound(finalTestResults.Average(x => x.Percent)) : ValueDefault;
                overallReport.OverallAvgPercentFinal = overallPercentFinalTest;
                overallModules.Add(new OverallModuleReportModel
                {
                    Percent = overallPercentFinalTest,
                    Index = ValueDefault,
                    TotalStudent = finalTestResults.Count,
                    Type = nameof(FinalTest)
                });
            }
            else
            {
                var mockTestResults = await (from baseQ in _courseResultRepository.Queryable.WhereBulkContains(studentIds, x => x.StudentId)
                                             join cum in _courseUnitMockTestRepository.Queryable on baseQ.CourseId equals cum.CourseId
                                             join c in _courseRepository.Queryable on cum.CourseId equals c.Id
                                             join mtr in _mockTestResultRepository.Queryable on new { baseQ.StudentId, baseQ.CourseId, MockTestId = cum.MockTestId } equals new { mtr.StudentId, mtr.CourseId, MockTestId = (Guid?)mtr.MockTestId } into mockTestGroup
                                             from mtr in mockTestGroup.DefaultIfEmpty()
                                             where baseQ.WorkingStatus == EnumWorkingStatus.Active &&
                                             (!request.EndDate.HasValue || (mtr.UpdatedDate ?? mtr.CreatedDate).Date <= request.EndDate.Value.Date) && mtr.Status == EnumResultStatus.Done && !mtr.UnitId.HasValue
                                             select new
                                             {
                                                 CourseLevel = c.CourseLevel,
                                                 MockTestResult = mtr
                                             }).ToListAsync();

                var mockTestGroups = (await _courseUnitMockTestRepository.Queryable.Include(x => x.Course).WhereBulkContains(courseIds, x => x.CourseId)
                                           .Where(x => x.MockTestId.HasValue).ToListAsync())
                                           .GroupBy(x => new { x.Number, x.Course!.CourseLevel })
                                           .Select(x => new
                                           {
                                               CourseLevel = x.Key.CourseLevel,
                                               Number = x.Key.Number,
                                               MockTestIds = x.Where(u => u.MockTestId.HasValue).Select(u => u.MockTestId.GetValueOrDefault()).ToList()
                                           })
                                           .ToList();

                foreach (var item in courseLevels)
                {
                    var courseLevelProgress = overallReport.CourseLevelProgresses?.FirstOrDefault(x => x.CourseLevel == item);
                    if (courseLevelProgress == null)
                    {
                        continue;
                    }
                    courseLevelProgress.OverallTestResults = Enumerable.Range(1, CourseProgressValue.CountFullMockTest).Select(i =>
                    {
                        var mockTestIds = mockTestGroups.Where(x => x.CourseLevel == item).Where(x => x.Number == i).SelectMany(x => x.MockTestIds).ToList();
                        var mockTestResultIndexs = mockTestResults.Where(x => x.CourseLevel == item).Select(x => x.MockTestResult).Where(x => mockTestIds.Contains(x.MockTestId)).ToList();
                        var resultScores = mockTestResultIndexs.Select(x =>
                        {
                            return x.SkillScores != null && x.SkillScores.Any() ? NumberHelper.RoundNumberDouble(x.SkillScores.Average(x => x.Scores)) : ValueDefault;
                        }).ToList();
                        return new OverallTestResultModel
                        {
                            Index = i,
                            TotalStudent = resultScores.Count,
                            Score = resultScores.Any() ? NumberHelper.RoundNumberDouble(resultScores.Average()) : ValueDefault,
                            Type = nameof(MockTest)
                        };
                    }).ToList();
                }
            }

            overallModules.AddRange(unitModules);
            overallReport.OverallModules = overallModules;
            overallReport.OverallAvgPercent = overallAvgPercent;
            return overallReport;
        }

        private async Task<(IList<OverallModuleReportModel>, List<StudentDtoModel>, double)> GetUnitResultGroups(GetOverallReportLearningResultQuery request, List<StudentDtoModel> students, IList<EnumCourseLevel> courseLevels)
        {
            var courseIds = students.Select(x => x.CourseId).Distinct().ToList();
            var unitGroups = (await _courseUnitMockTestRepository.Queryable.WhereBulkContains(courseIds, x => x.CourseId)
                                   .Where(x => x.UnitId.HasValue).AsNoTracking().ToListAsync())
                                   .GroupBy(x => x.Number)
                                   .Select(x => new
                                   {
                                       Number = x.Key,
                                       UnitIds = x.Where(u => u.UnitId.HasValue).Select(u => new
                                       {
                                           CourseId = u.CourseId,
                                           UnitId = u.UnitId.GetValueOrDefault()
                                       }).Distinct().ToList()
                                   })
                                   .ToList();

            var unitResultGroups = await (from baseQ in _courseResultRepository.Queryable.WhereBulkContains(students.Select(x => x.Id), x => x.StudentId)
                                          join cum in _courseUnitMockTestRepository.Queryable on baseQ.CourseId equals cum.CourseId
                                          join ur in _unitResultRepository.Queryable.Where(x => x.Status == EnumResultStatus.Done)
                                          .Where(x => (!request.EndDate.HasValue || (x.UpdatedDate ?? x.CreatedDate).Date <= request.EndDate.Value.Date))
                                          on baseQ.Id equals ur.CourseResultId into unitGroup
                                          from ur in unitGroup.DefaultIfEmpty()
                                          where baseQ.WorkingStatus == EnumWorkingStatus.Active
                                          group new { baseQ, ur }
                                          by new { baseQ.CourseId, baseQ.StudentId } into g
                                          select new
                                          {
                                              StudentId = g.Key.StudentId,
                                              OverallPercent = g.Where(x => x.ur != null).Select(x => x.ur).Any() ? Math.Round(g.Select(x => x.ur).Average(x => x.Percent)) : default,
                                              UnitResults = g.Where(x => x.ur != null).Select(x => x.ur).ToList()
                                          })
                                          .ToListAsync();

            if (request.OverallScores != null && request.OverallScores.Any())
            {
                unitResultGroups = unitResultGroups
                    .Where(x => request.OverallScores.Any(score =>
                        score == EnumOverallScore.Accuracy75OrMore
                            ? x.OverallPercent >= (int)EnumOverallScore.Accuracy75OrMore
                            : x.OverallPercent < (int)EnumOverallScore.Accuracy75OrMore))
                    .ToList();
            }

            var unitResults = unitResultGroups.Where(x => x.UnitResults != null && x.UnitResults.Any()).SelectMany(x => x.UnitResults).ToList();
            if (request.OverallScores?.Any() == true)
            {
                var matchedStudentIds = unitResultGroups.Select(x => x.StudentId).Distinct().ToHashSet();
                students = students.Where(s => matchedStudentIds.Contains(s.Id)).ToList();
            }

            var countUnit = GetUnitCountByCourseType(request.CourseType ?? default);
            var unitModules = Enumerable.Range(1, countUnit).Select(i =>
            {
                var courseUnits = unitGroups.Where(x => x.Number == i).SelectMany(x => x.UnitIds).ToList();
                var resultPercents = unitResults.Where(x => courseUnits.Any(y => y.UnitId == x.UnitId && y.CourseId == x.CourseId)).Select(x => x.Percent).ToList();
                return new OverallModuleReportModel
                {
                    Percent = resultPercents.Any() ? NumberHelper.ConvertRound(resultPercents.Average()) : ValueDefault,
                    Index = i,
                    TotalStudent = resultPercents.Count,
                    Type = nameof(Domain.Entities.Unit)
                };
            }).ToList();
            return (unitModules, students, unitResults.Any() ? NumberHelper.ConvertRound(unitResults.Average(x => x.Percent)) : ValueDefault);
        }

        private static IList<CourseLevelProgressModel> BuildCourseLevelProgress(IList<EnumCourseLevel> courseLevels, List<StudentDtoModel> students)
        {
            return courseLevels.Select(level => new CourseLevelProgressModel
            {
                CourseLevel = level,
                TotalStudent = students.Count(s => s.CourseLevel == level)
            }).ToList();
        }

        private static int GetUnitCountByCourseType(EnumCourseType courseType)
        {
            return courseType switch
            {
                EnumCourseType.Academic => CourseProgressValue.CountUnitAca,
                EnumCourseType.Ielts => CourseProgressValue.CountUnitIELTS,
                _ => CourseProgressValue.CountUnitRFIA2
            };
        }

        private static IList<dynamic> FilterUnitResultsByScore(IList<dynamic> unitResultGroups, List<EnumOverallScore> scores)
        {
            return unitResultGroups
                .Where(x => scores.Any(score =>
                    score == EnumOverallScore.Accuracy75OrMore
                        ? x.OverallPercent >= (int)EnumOverallScore.Accuracy75OrMore
                        : x.OverallPercent < (int)EnumOverallScore.Accuracy75OrMore))
                .ToList();
        }
    }
}
