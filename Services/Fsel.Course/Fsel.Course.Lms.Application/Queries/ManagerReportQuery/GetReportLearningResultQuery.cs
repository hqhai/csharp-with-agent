// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ManagerReportQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
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

    public class GetReportLearningResultQuery : SearchReportLearningResultQueryModel, IRequest<MethodResult<IList<LearningResultReportModel>>>
    {
    }

    public class GetReportLearningResultQueryHandler : IRequestHandler<GetReportLearningResultQuery, MethodResult<IList<LearningResultReportModel>>>
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IUnitResultRepository _unitResultRepository;
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly IFinalTestResultRepository _finalTestResultRepository;
        private readonly ICourseUnitMockTestRepository _courseUnitMockTestRepository;

        public GetReportLearningResultQueryHandler(
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

        public async Task<MethodResult<IList<LearningResultReportModel>>> Handle(GetReportLearningResultQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<LearningResultReportModel>>();
            var datas = new List<LearningResultReportModel>();

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
                OverallScore = request.OverallScore,
                LearningStatus = request.LearningStatus,
                ManagerReportType = EnumManagerReportType.ReportLearningResults,
                SortBy = request.SortBy
            }, cancellationToken);

            if (!userResults.IsOK)
            {
                methodResult.AddError(userResults.ErrorMessages);
                return methodResult;
            }
            var students = userResults?.Result;
            if (students == null)
            {
                methodResult.Result = datas;
                return methodResult;
            }
            var studentIds = students.Select(x => x.Id).ToList();
            var courseIds = students.Where(x => x.CourseId.HasValue).Select(x => x.CourseId!.Value).Distinct().ToList();

            if (studentIds == null || courseIds == null)
            {
                return methodResult;
            }

            var dataStudent = students.Select(x => new { StudentId = x.Id, CourseId = x.CourseId.GetValueOrDefault() }).ToList();

            var unitResultGroups = await (from baseQ in _courseResultRepository.Queryable.WhereBulkContains(students.Select(x => x.Id), x => x.StudentId)
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

            var mockTestResults = await _mockTestResultRepository.Queryable
                                                                 .WhereBulkContains(studentIds, x => x.StudentId)
                                                                 .WhereBulkContains(courseIds, x => x.CourseId)
                                                                 .Where(x => x.Status == EnumResultStatus.Done)
                                                                 .ToListAsync(cancellationToken);

            var mockTestGroupResults = mockTestResults.Join(dataStudent,
                                            mockTestResult => new { mockTestResult.CourseId, mockTestResult.StudentId },
                                            student => new { student.CourseId, student.StudentId },
                                            (mockTestResult, student) => mockTestResult)
                                          .GroupBy(x => x.StudentId)
                                          .Select(x => new
                                          {
                                              StudentId = x.Key,
                                              MockTestResults = x.Where(x => !x.UnitId.HasValue).OrderBy(x => x.CreatedDate).Select((y, index) => new
                                              {
                                                  Index = index + 1,
                                                  MockTestResult = y
                                              }).ToList(),
                                              SkillMockTestResults = x.Where(x => x.UnitId.HasValue).OrderBy(x => x.CreatedDate).ToList(),
                                          }).ToList();

            var finalTestResults = await (from baseQ in _courseResultRepository.Queryable.WhereBulkContains(students.Select(x => x.Id), x => x.StudentId)
                                          join cum in _courseUnitMockTestRepository.Queryable on baseQ.CourseId equals cum.CourseId
                                          join ftr in _finalTestResultRepository.Queryable on new { baseQ.StudentId, baseQ.CourseId, FinalTestId = cum.FinalTestId } equals new { ftr.StudentId, ftr.CourseId, FinalTestId = (Guid?)ftr.FinalTestId } into finalTestGroup
                                          from ftr in finalTestGroup.DefaultIfEmpty()
                                          where baseQ.WorkingStatus == EnumWorkingStatus.Active &&
                                          (!request.EndDate.HasValue || (ftr.UpdatedDate ?? ftr.CreatedDate).Date <= request.EndDate.Value.Date) && ftr.Status == EnumResultStatus.Done
                                          select new
                                          {
                                              Percent = ftr.Percent,
                                              StudentId = ftr.StudentId,
                                          }).ToListAsync(cancellationToken);

            var countUnit = request.CourseType == EnumCourseType.Academic ? CourseProgressValue.CountUnitAca :
                            request.CourseType == EnumCourseType.Ielts ? CourseProgressValue.CountUnitIELTS : ValueDefault;

            foreach (var item in students)
            {
                var unitResultGroup = unitResultGroups.FirstOrDefault(x => x.StudentId == item.Id);
                var courseUnitResults = unitResultGroup?.UnitResults.OrderBy(x => x.CreatedDate).Select((y, index) => new
                {
                    Percent = y.Percent,
                    UnitId = y.UnitId,
                    CourseId = y.CourseId,
                    Index = index + 1
                });

                var overallPercent = NumberHelper.ConvertRound(unitResultGroup?.OverallPercent ?? default);
                var result = mockTestGroupResults.FirstOrDefault(x => x.StudentId == item.Id);
                var overallModuleReports = new List<OverallModuleReportSkillModel>();

                for (int i = 1; i <= countUnit; i++)
                {
                    var unitResult = courseUnitResults?.FirstOrDefault(x => x.Index == i);
                    overallModuleReports.Add(new OverallModuleReportSkillModel
                    {
                        Index = i,
                        DisplayOrder = request.CourseType == EnumCourseType.Ielts && i == CourseProgressValue.MockTestPosition ? i + 1 : i,
                        Percent = unitResult?.Percent,
                        Type = nameof(Domain.Entities.Unit)
                    });
                    if (request.CourseType == EnumCourseType.Ielts)
                    {
                        var skillScores = result?.SkillMockTestResults.FirstOrDefault(x => x.UnitId == unitResult?.UnitId && x.CourseId == unitResult?.CourseId)?.SkillScores;
                        overallModuleReports.Add(new OverallModuleReportSkillModel
                        {
                            Index = i,
                            DisplayOrder = i == CourseProgressValue.MockTestPosition ? i + 1 : i,
                            Score = skillScores != null && skillScores.Any() ? NumberHelper.RoundNumberDouble(skillScores.Average(x => x.Scores)) : null,
                            Type = nameof(EnumMockTestType.SkillMockTest)
                        });
                    }
                }
                if (request.CourseType == EnumCourseType.Academic)
                {
                    var finalTestResult = finalTestResults.FirstOrDefault(x => x.StudentId == item.Id);
                    int displayOrder = overallModuleReports.Count + 1;
                    overallModuleReports.Add(new OverallModuleReportSkillModel
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
                        overallModuleReports.Add(new OverallModuleReportSkillModel
                        {
                            Index = mockTestResult?.Index ?? i,
                            DisplayOrder = CourseProgressValue.MockTestPosition * i,
                            Score = skillScores != null && skillScores.Any() ? NumberHelper.RoundNumberDouble(skillScores.Average(x => x.Scores)) : null,
                            Type = nameof(EnumMockTestType.FullMockTest),
                            SkillScores = skillScores
                        });
                    }
                }
                var learningResult = new LearningResultReportModel
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
                    OverallPercent = overallPercent,
                    OverallModuleReports = overallModuleReports.OrderBy(x => x.DisplayOrder).ToList(),
                };
                datas.Add(learningResult);
            }
            methodResult.Result = datas;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
