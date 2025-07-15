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

                SchoolClass = request.SchoolClass,
                SchoolGrade = request.SchoolGrade,
                EndDate = request.EndDate,
                Keyword = request.Keyword,
                LearningStatus = request.LearningStatus,
                CourseType = request.CourseType,
                CourseLevel = request.CourseLevel,
                OverallScore = request.OverallScore,
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
            var courseIds = students.Select(x => x.CourseId).Distinct().ToList();

            var unitGroups = (await _courseUnitMockTestRepository.Queryable.WhereBulkContains(courseIds, x => x.CourseId)
                                     .Where(x => x.UnitId.HasValue).ToListAsync())
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
                                          on new { baseQ.StudentId, baseQ.CourseId, UnitId = cum.UnitId } equals new { ur.StudentId, ur.CourseId, UnitId = (Guid?)ur.UnitId } into unitGroup
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
            unitResultGroups = unitResultGroups.Where(x => !request.OverallScore.HasValue || (request.OverallScore == EnumOverallScore.Accuracy75OrMore ? x.OverallPercent >= (int)EnumOverallScore.Accuracy75OrMore : x.OverallPercent < (int)EnumOverallScore.Accuracy75OrMore)).ToList();

            var unitResults = unitResultGroups.Where(x => x.UnitResults != null && x.UnitResults.Any()).SelectMany(x => x.UnitResults).ToList();
            students = students.Where(x => !request.OverallScore.HasValue || unitResultGroups.Select(x => x.StudentId).Distinct().Contains(x.Id)).ToList();
            var studentIds = students.Select(x => x.Id).ToList();
            var overallReport = new OverallReportLearningResultModel
            {
                TotalStudent = students.Count,
                CourseLevelProgresses = courseLevels.Select(x => new CourseLevelProgressModel
                {
                    CourseLevel = x,
                    TotalStudent = students.Count(y => y.CourseLevel == x)
                }).ToList()
            };
            var countUnit = request.CourseType == EnumCourseType.Academic ? CourseProgressValue.CountUnitAca : request.CourseType == EnumCourseType.Ielts ? CourseProgressValue.CountUnitIELTS : CourseProgressValue.CountUnitRFIA2;
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
            });

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
            overallReport.OverallAvgPercent = unitResults.Any() ? NumberHelper.ConvertRound(unitResults.Average(x => x.Percent)) : ValueDefault;
            return overallReport;
        }
    }
}
