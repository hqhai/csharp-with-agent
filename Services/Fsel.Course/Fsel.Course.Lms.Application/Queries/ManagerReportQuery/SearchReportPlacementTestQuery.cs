// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ManagerReportQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
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
        public SearchReportPlacementTestQuery()
        {
            ManagerReportType = EnumManagerReportType.ReportManagerPT;
        }
    }

    public class SearchReportPlacementTestQueryHandler : IRequestHandler<SearchReportPlacementTestQuery, MethodResult<SearchReportPlacementTestModel>>
    {
        private readonly IPlacementTestGroupResultRepository _placementTestGroupResultRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public SearchReportPlacementTestQueryHandler(
            IPlacementTestGroupResultRepository placementTestGroupResultRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _placementTestGroupResultRepository = placementTestGroupResultRepository;
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
            var dataOverallResult = await _mediator.Send(new GetOverallReportPlacementTestQuery(request), cancellationToken);
            var reportPlacementTest = _mapper.Map<SearchReportPlacementTestModel>(dataOverallResult.Result);
            var userResults = await _mediator.Send(new GetStudentReportQuery(request), cancellationToken);
            if (!userResults.IsOK)
            {
                methodResult.AddError(userResults.ErrorMessages);
                return methodResult;
            }
            var students = userResults?.Result;
            if (students == null || !students.Any())
            {
                methodResult.Result = reportPlacementTest;
                return methodResult;
            }

            var studentIds = students.Select(x => x.Id).ToList();

            var placmentTestGroupResults = await _placementTestGroupResultRepository.Queryable.WhereBulkContains(studentIds, x => x.StudentId)
                .Select(x => new
                {
                    StudentId = x.StudentId,
                    Status = x.Status,
                    ChooseLevel = x.ChooseLevel,
                    CompletionLevel = x.CompletionLevel,
                    SuggetLevel = x.SuggetLevel,
                    x.CompletionDate
                })
                .ToListAsync(cancellationToken);
            var lists = placmentTestGroupResults.ToLookup(x => x.StudentId);

            var datas = students.Select(item =>
            {
                var groupResult = lists[item.Id].FirstOrDefault();
                var placementTestReport = new PlacementTestReportModel
                {
                    StudentId = item.Id,
                    Birthday = item.BirthDay,
                    Email = item.Email,
                    FullName = item.FullName,
                    PhoneNumber = item.PhoneNumber,
                    SchoolClass = item.SchoolClass,
                    SchoolGrade = item.SchoolGrade,
                    SchoolName = item.School,
                    UserName = item.UserName,
                    ExpiredPTDate = groupResult?.CompletionDate,
                    Status = groupResult?.Status == EnumResultStatus.Done ? EnumCompletionStatus.Completed : EnumCompletionStatus.InProgress,
                    ChooseLevel = groupResult?.ChooseLevel,
                    CurrentLevel = groupResult?.SuggetLevel
                };
                return placementTestReport;
            }).ToList();

            reportPlacementTest.PagingItems = new PagingItemsModel<PlacementTestReportModel>(datas, request, reportPlacementTest.TotalStudent);
            methodResult.Result = reportPlacementTest;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
