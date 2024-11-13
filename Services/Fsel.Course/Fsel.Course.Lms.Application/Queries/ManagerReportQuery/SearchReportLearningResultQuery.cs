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

        public SearchReportLearningResultQueryHandler(
            IMediator mediator,
            IMapper mapper,
            ICourseResultRepository courseResultRepository,
            ICourseRepository courseRepository,
            IUnitResultRepository unitResultRepository,
            IMockTestResultRepository mockTestResultRepository,
            IFinalTestResultRepository finalTestResultRepository
            )
        {
            _mediator = mediator;
            _mapper = mapper;
            _courseResultRepository = courseResultRepository;
            _courseRepository = courseRepository;
            _unitResultRepository = unitResultRepository;
            _mockTestResultRepository = mockTestResultRepository;
            _finalTestResultRepository = finalTestResultRepository;
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
                SchoolGrade = request.SchoolGrade,
                SchoolClass = request.SchoolClass,
                EndDate = request.EndDate,
                CourseLevel = request.CourseLevel,
                CourseType = request.CourseType,
            }, cancellationToken);
            var reportLearningResult = _mapper.Map<SearchReportLearningResultModel>(dataOverallResult.Result);
            var userResults = await _mediator.Send(new GetStudentReportQuery
            {
                ListDistrict = request.ListDistrict,
                ListProvince = request.ListProvince,
                ListSchool = request.ListSchool,
                SchoolGrade = request.SchoolGrade,
                SchoolClass = request.SchoolClass,
                EndDate = request.EndDate,
                PageSize = request.PageSize,
                Filters = request.Filters,
                IncludePaths = request.IncludePaths,
                Keyword = request.Keyword,
                Page = request.Page,
                CourseLevel = request.CourseLevel,
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
            if (students == null)
            {
                methodResult.Result = reportLearningResult;
                return methodResult;
            }
            var studentIds = students.Select(x => x.Id).ToList();
            var courseIds = students.Select(x => x.CourseId).ToList();
            var dataStudent = students.Select(x => new { StudentId = x.Id, CourseId = x.CourseId.GetValueOrDefault() }).ToList();

            var unitResults = await _unitResultRepository.Queryable
                                          .Where(x => studentIds.Contains(x.StudentId) && courseIds.Contains(x.CourseId) && x.Status == EnumResultStatus.Done)
                                          .ToListAsync(cancellationToken);

            var mockTestResults = await _mockTestResultRepository.Queryable
                                          .Where(x => studentIds.Contains(x.StudentId) && courseIds.Contains(x.CourseId) && x.Status == EnumResultStatus.Done)
                                          .Where(x => !x.UnitId.HasValue)
                                          .ToListAsync(cancellationToken);
            var finalTestResults = await _finalTestResultRepository.Queryable
                                          .Where(x => studentIds.Contains(x.StudentId) && courseIds.Contains(x.CourseId) && x.Status == EnumResultStatus.Done)
                                          .ToListAsync(cancellationToken);

            var unitGroupResults = unitResults.Join(dataStudent,
                                            unitResult => new { unitResult.CourseId, unitResult.StudentId },
                                            student => new { student.CourseId, student.StudentId },
                                            (unitResult, student) => unitResult)
                                          .GroupBy(x => x.StudentId)
                                          .Select(x => new
                                          {
                                              StudentId = x.Key,
                                              CourseUnitResults = x.OrderBy(x => x.CreatedDate).Select((y, index) => new
                                              {
                                                  Percent = y.Percent,
                                                  Index = index + 1
                                              })
                                          }).ToList();

            var mockTestGroupResults = mockTestResults.Join(dataStudent,
                                            mockTestResult => new { mockTestResult.CourseId, mockTestResult.StudentId },
                                            student => new { student.CourseId, student.StudentId },
                                            (unitResult, student) => unitResult)
                                          .GroupBy(x => x.StudentId)
                                          .Select(x => new
                                          {
                                              StudentId = x.Key,
                                              MockTestResults = x.OrderBy(x => x.CreatedDate).Select((y, index) => new
                                              {
                                                  Index = index + 1,
                                                  MockTestResult = y
                                              }).ToList(),
                                          }).ToList();

            var finalTestGroupResults = finalTestResults.Join(dataStudent,
                                                      unitResult => new { unitResult.CourseId, unitResult.StudentId },
                                                      student => new { student.CourseId, student.StudentId },
                                                      (unitResult, student) => unitResult)
                                                    .Select(x => new
                                                    {
                                                        StudentId = x.StudentId,
                                                        Percent = x.Percent
                                                    }).ToList();

            var countUnit = request.CourseType == EnumCourseType.Academic ? CourseProgressValue.CountUnitAca :
                            request.CourseType == EnumCourseType.Ielts ? CourseProgressValue.CountUnitIELTS : ValueDefault;

            var datas = new List<LearningResultModel>();
            foreach (var item in students)
            {
                var overallModuleReports = new List<OverallModuleReportModel>();

                var unitGroupResult = unitGroupResults.FirstOrDefault(x => x.StudentId == item.Id);
                var result = mockTestGroupResults.FirstOrDefault(x => x.StudentId == item.Id);
                for (int i = 1; i <= countUnit; i++)
                {
                    var unit = unitGroupResult?.CourseUnitResults.FirstOrDefault(x => x.Index == i);
                    overallModuleReports.Add(new OverallModuleReportModel
                    {
                        Index = i,
                        DisplayOrder = request.CourseType == EnumCourseType.Ielts && i == 5 ? i + 1 : i,
                        Percent = unit?.Percent,
                        Type = nameof(Domain.Entities.Unit)
                    });
                }
                if (request.CourseType == EnumCourseType.Academic)
                {
                    var finalTestResult = finalTestGroupResults.FirstOrDefault(x => x.StudentId == item.Id);
                    overallModuleReports.Add(new OverallModuleReportModel
                    {
                        Index = overallModuleReports.Count + 1,
                        DisplayOrder = overallModuleReports.Count + 1,
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
                    Email = item.Email,
                    FullName = item.FullName,
                    SchoolClass = item.SchoolClass,
                    SchoolGrade = item.SchoolGrade,
                    SchoolName = item.School,
                    Status = item.ExpiredDate > DateTime.UtcNow ? EnumLearningStatus.InProgress : EnumLearningStatus.Expired,
                    CourseLevel = item.CourseLevel,
                    ProcessDate = item.CreatedDate,
                    ExpiredDate = item.ExpiredDate,
                    OverallPercent = unitGroupResult != null && unitGroupResult.CourseUnitResults.Any() ? NumberHelper.ConvertRound(unitGroupResult.CourseUnitResults.Average(x => x.Percent)) : ValueDefault,
                    OverallModules = overallModuleReports.OrderBy(x => x.DisplayOrder).ToList(),
                };
                datas.Add(learningResult);
            }

            reportLearningResult.PagingItems = new PagingItemsModel<LearningResultModel>(datas, request, reportLearningResult.TotalStudent);
            methodResult.Result = reportLearningResult;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
