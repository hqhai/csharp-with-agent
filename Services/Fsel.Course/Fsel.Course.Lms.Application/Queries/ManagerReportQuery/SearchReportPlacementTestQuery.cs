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
            var dataOverallResult = await _mediator.Send(new GetOverallReportPlacementTestQuery
            {
                ListDistrict = request.ListDistrict,
                ListProvince = request.ListProvince,
                ListSchool = request.ListSchool,
                SchoolGrade = request.SchoolGrade,
                EndDate = request.EndDate,
                Filters = request.Filters,
                IncludePaths = request.IncludePaths,
                Keyword = request.Keyword,
                Page = request.Page,
                PageSize = request.PageSize,
                Status = request.Status,
                StartDate = request.StartDate,
            }, cancellationToken);
            var reportPlacementTest = _mapper.Map<SearchReportPlacementTestModel>(dataOverallResult.Result);
            var userResults = await _mediator.Send(new GetStudentReportQuery
            {
                ListDistrict = request.ListDistrict,
                ListProvince = request.ListProvince,
                ListSchool = request.ListSchool,
                SchoolGrade = request.SchoolGrade,
                EndDate = request.EndDate,
                Filters = request.Filters,
                IncludePaths = request.IncludePaths,
                Keyword = request.Keyword,
                Page = request.Page,
                PageSize = request.PageSize,
                Status = request.Status,
                StartDate = request.StartDate,
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
                return methodResult;
            }
            var studentIds = students.Select(x => x.Id).ToList();
            var lists = await _placementTestGroupResultRepository.Queryable.Where(x => studentIds != null && studentIds.Any(y => y == x.StudentId)).ToListAsync(cancellationToken);

            var data = new List<PlacementTestReportModel>();
            var placementTestResults = await _placementTestResultRepository.Queryable.Where(x => studentIds.Contains(x.StudentId))
                .GroupBy(x => x.StudentId)
                .Select(x => x.Select(x => x).OrderByDescending(x => x.UpdatedDate ?? x.CreatedDate).FirstOrDefault())
                .ToListAsync(cancellationToken);

            foreach (var item in students)
            {
                var placementTestGroupResult = lists.FirstOrDefault(x => x.StudentId == item.Id);
                var placementTestResult = placementTestResults.FirstOrDefault(x => x.StudentId == item.Id);
                data.Add(new PlacementTestReportModel
                {
                    Birthday = item.BirthDay,
                    Email = item.Email,
                    FullName = item.FullName,
                    PhoneNumber = item.PhoneNumber,
                    SchoolClass = item.SchoolClass,
                    SchoolGrade = item.SchoolGrade,
                    SchoolName = item.School,
                    Status = GetStatus(placementTestGroupResult),
                    ChooseLevel = placementTestGroupResult?.ChooseLevel,
                    CurrentLevel = placementTestGroupResult?.SuggetLevel,
                    ExpirePTDate = placementTestResult?.UpdatedDate ?? placementTestResult?.CreatedDate,
                });
            }

            reportPlacementTest.PagingItems = new PagingItemsModel<PlacementTestReportModel>(data, request, reportPlacementTest.TotalStudent);
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
