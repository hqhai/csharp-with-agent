// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ManagerReportQuery
{
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

    public class SearchReportPlacementTestQuery : SearchReportPlacementTestQueryModel, IRequest<MethodResult<SearchReportPlacementTestModel>>
    {
    }

    public class SearchReportPlacementTestQueryHandler : IRequestHandler<SearchReportPlacementTestQuery, MethodResult<SearchReportPlacementTestModel>>
    {
        private readonly IPlacementTestGroupResultRepository _placementTestGroupResultRepository;
        private readonly IPlacementTestResultRepository _placementTestResultRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public SearchReportPlacementTestQueryHandler(
            IPlacementTestGroupResultRepository placementTestGroupResultRepository,
            IPlacementTestResultRepository placementTestResultRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _placementTestGroupResultRepository = placementTestGroupResultRepository;
            _placementTestResultRepository = placementTestResultRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<MethodResult<SearchReportPlacementTestModel>> Handle(SearchReportPlacementTestQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<SearchReportPlacementTestModel>();
            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }
            var dataOverallResult = await _mediator.Send(new GetOverallReportPlacementTestQuery
            {
                ListDistrict = request.ListDistrict,
                ListProvince = request.ListProvince,
                ListSchool = request.ListSchool,
                SchoolGrade = request.SchoolGrade,
                EndDate = request.EndDate,
                Keyword = request.Keyword,
                Status = request.Status,
                StartDate = request.StartDate,
                CourseLevel = request.CourseLevel,
                CurrentLevel = request.CurrentLevel,
            }, cancellationToken);
            var reportPlacementTest = _mapper.Map<SearchReportPlacementTestModel>(dataOverallResult.Result);
            var userResults = await _mediator.Send(new GetStudentReportQuery
            {
                ListDistrict = request.ListDistrict,
                ListProvince = request.ListProvince,
                ListSchool = request.ListSchool,
                SchoolGrade = request.SchoolGrade,
                SchoolClass = request.SchoolClass,
                Filters = request.Filters,
                IncludePaths = request.IncludePaths,
                Keyword = request.Keyword,
                Page = request.Page,
                PageSize = request.PageSize,
                Status = request.Status,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                CourseLevel = request.CourseLevel,
                CurrentLevel = request.CurrentLevel,
                ManagerReportType = EnumManagerReportType.ReportManagerPT,
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
                methodResult.Result = reportPlacementTest;
                return methodResult;
            }
            var studentIds = students.Select(x => x.Id).ToList();
            var lists = await _placementTestGroupResultRepository.Queryable.Where(x => studentIds.Any(y => y == x.StudentId)).ToListAsync(cancellationToken);

            var placementTestResults = await _placementTestResultRepository.Queryable.Where(x => studentIds.Contains(x.StudentId))
                .GroupBy(x => x.StudentId)
                .Select(x => x.Select(x => x).OrderByDescending(x => x.UpdatedDate ?? x.CreatedDate).FirstOrDefault())
                .ToListAsync(cancellationToken);

            var datas = students.Select(item =>
            {
                var placementTestGroupResult = lists.FirstOrDefault(x => x.StudentId == item.Id);
                var placementTestResult = placementTestResults.FirstOrDefault(x => x != null && x.StudentId == item.Id);
                var placementTestReport = new PlacementTestReportModel
                {
                    Birthday = item.BirthDay,
                    Email = item.Email,
                    FullName = item.FullName,
                    PhoneNumber = item.PhoneNumber,
                    SchoolClass = item.SchoolClass,
                    SchoolGrade = item.SchoolGrade,
                    SchoolName = item.School,
                    ExpiredPTDate = placementTestResult?.UpdatedDate ?? placementTestResult?.CreatedDate
                };
                if (placementTestGroupResult != null)
                {
                    placementTestReport.Status = GetStatus(placementTestGroupResult);
                    placementTestReport.ChooseLevel = placementTestGroupResult.ChooseLevel;
                    placementTestReport.CurrentLevel = placementTestGroupResult.CompletionLevel.HasValue ? placementTestGroupResult.CompletionLevel.Value.GetCourseLevelByPlacementTestLevel() : null;
                }
                return placementTestReport;
            }).ToList();

            reportPlacementTest.PagingItems = new PagingItemsModel<PlacementTestReportModel>(datas, request, reportPlacementTest.TotalStudent);
            methodResult.Result = reportPlacementTest;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private static EnumCompletionStatus GetStatus(PlacementTestGroupResult? placementTestGroupResult)
        {
            return placementTestGroupResult == null ? EnumCompletionStatus.NotStarted : placementTestGroupResult.Status == EnumResultStatus.Done ? EnumCompletionStatus.Completed : EnumCompletionStatus.NotStarted;
        }
    }
}
