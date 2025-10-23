// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ManagerReportQuery
{
    using System.Threading;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels.ManagerReportModels;
    using Fsel.Course.Domain.Models.QueryModels.ManagerReports;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using static Fsel.Shared.Constants.ValueSettings;

    public class SearchReportLearningResultQuery : SearchReportLearningResultQueryModel, IRequest<MethodResult<SearchReportLearningResultModel>>
    {
    }

    public class SearchReportLearningResultQueryHandler : IRequestHandler<SearchReportLearningResultQuery, MethodResult<SearchReportLearningResultModel>>
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IUnitResultRepository _unitResultRepository;
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly IFinalTestResultRepository _finalTestResultRepository;
        private readonly ICourseUnitMockTestRepository _courseUnitMockTestRepository;

        public SearchReportLearningResultQueryHandler(
            IMediator mediator,
            IMapper mapper,
            ICourseResultRepository courseResultRepository,
            ICourseRepository courseRepository,
            IUnitResultRepository unitResultRepository,
            IMockTestResultRepository mockTestResultRepository,
            IFinalTestResultRepository finalTestResultRepository,
            ICourseUnitMockTestRepository courseUnitMockTestRepository
            )
        {
            _mediator = mediator;
            _mapper = mapper;
            _courseResultRepository = courseResultRepository;
            _courseRepository = courseRepository;
            _unitResultRepository = unitResultRepository;
            _mockTestResultRepository = mockTestResultRepository;
            _finalTestResultRepository = finalTestResultRepository;
            _courseUnitMockTestRepository = courseUnitMockTestRepository;
        }

        public async Task<MethodResult<SearchReportLearningResultModel>> Handle(SearchReportLearningResultQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<SearchReportLearningResultModel>();
            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }
            var dataOverallResult = await _mediator.Send(new GetOverallReportLearningResultQuery
            {
                Keyword = request.Keyword,
                ListDistrict = request.ListDistrict,
                ListProvince = request.ListProvince,
                ListSchool = request.ListSchool,
                ListSchoolClass = request.ListSchoolClass,
                ListSchoolGrade = request.ListSchoolGrade,
                ListCourseLevel = request.ListCourseLevel,
                ListCompletionStatus = request.ListCompletionStatus,
                ListLearningStatus = request.ListLearningStatus,
                ListOverallScore = request.ListOverallScore,
                IsLearning = request.IsLearning,
                ListCurrentLevel = request.ListCurrentLevel,

                EndDate = request.EndDate,
                CourseType = request.CourseType,
            }, cancellationToken);
            var reportLearningResult = _mapper.Map<SearchReportLearningResultModel>(dataOverallResult.Result);
            var userResults = await _mediator.Send(new GetStudentReportQuery
            {
                ListDistrict = request.ListDistrict,
                ListProvince = request.ListProvince,
                ListSchool = request.ListSchool,
                ListSchoolClass = request.ListSchoolClass,
                ListSchoolGrade = request.ListSchoolGrade,
                ListCourseLevel = request.ListCourseLevel,
                IsLearning = request.IsLearning,
                ListLearningStatus = request.ListLearningStatus,
                ListCompletionStatus = request.ListCompletionStatus,
                ListCurrentLevel = request.ListCurrentLevel,
                ListOverallScore = request.ListOverallScore,

                EndDate = request.EndDate,
                PageSize = request.PageSize,
                Filters = request.Filters,
                IncludePaths = request.IncludePaths,
                SortBy = request.SortBy,
                Keyword = request.Keyword,
                Page = request.Page,
                CourseType = request.CourseType,
                ManagerReportType = EnumManagerReportType.ReportLearningResults,
                IsSearchReport = true
            }, cancellationToken);

            if (!userResults.IsOK)
            {
                methodResult.AddError(userResults.ErrorMessages);
                return methodResult;
            }
            var students = userResults?.Result;
            if (students == null || !students.Any())
            {
                methodResult.Result = reportLearningResult;
                return methodResult;
            }
            var studentIds = students.Select(x => x.Id).ToList();
            var courseIds = students.Select(x => x.CourseId).ToList();
            var dataStudent = students.Select(x => new { StudentId = x.Id, CourseId = x.CourseId.GetValueOrDefault() }).ToList();

            var unitResultGroups = await (from baseQ in _courseResultRepository.Queryable.WhereBulkContains(studentIds, x => x.StudentId)
                                          join cum in _courseUnitMockTestRepository.Queryable on baseQ.CourseId equals cum.CourseId
                                          join ur in _unitResultRepository.Queryable on new { baseQ.StudentId, baseQ.CourseId, UnitId = cum.UnitId } equals new { ur.StudentId, ur.CourseId, UnitId = (Guid?)ur.UnitId } into unitGroup
                                          from ur in unitGroup.DefaultIfEmpty()
                                          where baseQ.WorkingStatus == EnumWorkingStatus.Active &&
                                          (!request.EndDate.HasValue || (ur.UpdatedDate ?? ur.CreatedDate).Date <= request.EndDate.Value.Date) && ur.Status == EnumResultStatus.Done
                                          group new { baseQ, ur }
                                          by new { baseQ.CourseId, baseQ.StudentId } into g
                                          select new
                                          {
                                              StudentId = g.Key.StudentId,
                                              OverallPercent = g.Select(x => x.ur).Any() ? g.Select(x => x.ur).Average(x => x.Percent) : default,
                                              UnitResults = g.Select(x => x.ur).ToList(),
                                          }).ToListAsync(cancellationToken);

            var mockTestResults = await (from baseQ in _courseResultRepository.Queryable.WhereBulkContains(studentIds, x => x.StudentId)
                                         join cum in _courseUnitMockTestRepository.Queryable on baseQ.CourseId equals cum.CourseId
                                         join c in _courseRepository.Queryable on cum.CourseId equals c.Id
                                         join mtr in _mockTestResultRepository.Queryable on new { baseQ.StudentId, baseQ.CourseId, MockTestId = cum.MockTestId } equals new { mtr.StudentId, mtr.CourseId, MockTestId = (Guid?)mtr.MockTestId } into mockTestGroup
                                         from mtr in mockTestGroup.DefaultIfEmpty()
                                         where baseQ.WorkingStatus == EnumWorkingStatus.Active &&
                                         (!request.EndDate.HasValue || (mtr.UpdatedDate ?? mtr.CreatedDate).Date <= request.EndDate.Value.Date) && mtr.Status == EnumResultStatus.Done && !mtr.UnitId.HasValue
                                         select mtr).ToListAsync(cancellationToken);

            var mockTestGroupResults = mockTestResults.GroupBy(x => x.StudentId)
                                          .Select(x => new
                                          {
                                              StudentId = x.Key,
                                              MockTestResults = x.OrderBy(x => x.CreatedDate).Select((y, index) => new
                                              {
                                                  Index = index + 1,
                                                  MockTestResult = y
                                              }).ToList(),
                                          }).ToList();

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
                                              StudentId = ftr.StudentId,
                                          }).ToListAsync(cancellationToken);

            var datas = new List<LearningResultModel>();
            foreach (var item in students)
            {
                var countUnit = request.CourseType == EnumCourseType.Academic ? CourseProgressValue.CountUnitAca :
                                request.CourseType == EnumCourseType.Ielts ? CourseProgressValue.CountUnitIELTS :
                                request.CourseType == EnumCourseType.EnglishFoundation ? CourseProgressValue.CountUnitRFIA2 : default;

                var unitResultGroup = unitResultGroups.FirstOrDefault(x => x.StudentId == item.Id);
                var courseUnitResults = unitResultGroup?.UnitResults.OrderBy(x => x.CreatedDate).Select((y, index) => new
                {
                    Percent = y.Percent,
                    Index = index + 1
                });
                var overallPercent = NumberHelper.ConvertRound(unitResultGroup?.OverallPercent ?? default);
                var overallModuleReports = new List<OverallModuleReportModel>();

                var result = mockTestGroupResults.FirstOrDefault(x => x.StudentId == item.Id);
                for (int i = 1; i <= countUnit; i++)
                {
                    var unit = courseUnitResults?.FirstOrDefault(x => x.Index == i);
                    overallModuleReports.Add(new OverallModuleReportModel
                    {
                        Index = i,
                        DisplayOrder = request.CourseType == EnumCourseType.Ielts && i == CourseProgressValue.MockTestPosition ? i + 1 : i,
                        Percent = unit?.Percent,
                        Type = nameof(Domain.Entities.Unit)
                    });
                }
                if (request.CourseType == EnumCourseType.Academic || request.CourseType == EnumCourseType.EnglishFoundation)
                {
                    var finalTestResult = finalTestResults.FirstOrDefault(x => x.StudentId == item.Id);
                    int displayOrder = overallModuleReports.Count + 1;
                    overallModuleReports.Add(new OverallModuleReportModel
                    {
                        Index = displayOrder,
                        DisplayOrder = displayOrder,
                        Percent = finalTestResult?.Percent,
                        Type = nameof(FinalTest)
                    });
                }
                else
                {
                    for (int i = 1; i <= CourseProgressValue.CountFullMockTest; i++)
                    {
                        var mockTestResult = result?.MockTestResults.FirstOrDefault(x => x.Index == i);
                        var skillScores = mockTestResult?.MockTestResult.SkillScores;
                        overallModuleReports.Add(new OverallModuleReportModel
                        {
                            Index = mockTestResult?.Index ?? i,
                            DisplayOrder = CourseProgressValue.MockTestPosition * i,
                            Score = skillScores != null && skillScores.Any() ? NumberHelper.RoundNumberDouble(skillScores.Average(x => x.Scores)) : null,
                            Type = nameof(MockTest)
                        });
                    }
                }
                var learningResult = new LearningResultModel
                {
                    StudentId = item.Id,
                    Email = item.Email,
                    FullName = item.FullName,
                    PhoneNumber = item.PhoneNumber,
                    SchoolClass = item.SchoolClass,
                    SchoolGrade = item.SchoolGrade,
                    SchoolName = item.School,
                    Status = item.ExpiredDate > DateTime.UtcNow ? EnumLearningStatus.InProgress : EnumLearningStatus.Expired,
                    CourseLevel = item.CourseLevel,
                    ProcessDate = item.CreatedDate,
                    ExpiredDate = item.ExpiredDate,
                    UserName = item.UserName,
                    OverallPercent = overallPercent,
                    OverallModules = overallModuleReports.OrderBy(x => x.DisplayOrder).ToList(),
                };

                datas.Add(learningResult);
            }

            if (request.StatusLearning.HasValue)
            {
                datas = datas.Where(p => p.LearningStatus == request.StatusLearning).ToList();
            }

            reportLearningResult.PagingItems = new PagingItemsModel<LearningResultModel>(datas, request, reportLearningResult.TotalStudent);
            methodResult.Result = reportLearningResult;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
