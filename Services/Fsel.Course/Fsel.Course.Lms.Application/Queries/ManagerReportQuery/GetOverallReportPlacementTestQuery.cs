// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ManagerReportQuery
{
    using System;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels.ManagerReportModels;
    using Fsel.Course.Domain.Models.QueryModels.ManagerReports;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetOverallReportPlacementTestQuery : SearchReportPlacementTestQueryModel, IRequest<MethodResult<OverallReportPlacementTestModel>>
    {
        public GetOverallReportPlacementTestQuery()
        {
            ManagerReportType = EnumManagerReportType.ReportManagerPT;
        }

        public GetOverallReportPlacementTestQuery(SearchReportPlacementTestQueryModel source)
        {
            ManagerReportType = EnumManagerReportType.ReportManagerPT;
            CopyFrom(source);
        }
    }

    public class GetOverallReportPlacementTestQueryHandler : IRequestHandler<GetOverallReportPlacementTestQuery, MethodResult<OverallReportPlacementTestModel>>
    {
        private readonly IPlacementTestGroupResultRepository _placementTestGroupResultRepository;
        private readonly IMediator _mediator;

        public GetOverallReportPlacementTestQueryHandler(
            IPlacementTestGroupResultRepository placementTestGroupResultRepository,
            IMediator mediator)
        {
            _placementTestGroupResultRepository = placementTestGroupResultRepository;
            _mediator = mediator;
        }

        public async Task<MethodResult<OverallReportPlacementTestModel>> Handle(GetOverallReportPlacementTestQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<OverallReportPlacementTestModel>();
            var userResults = await _mediator.Send(new GetStudentReportQuery(request), cancellationToken);
            if (!userResults.IsOK)
            {
                methodResult.AddError(userResults.ErrorMessages);
                return methodResult;
            }
            var students = userResults?.Result;
            var studentIds = students?.Select(x => x.Id).ToList() ?? new List<Guid>();
            var placementTestGroupResults = await _placementTestGroupResultRepository.Queryable.WhereBulkContains(studentIds, x => x.StudentId).Where(x => x.CompletionLevel != EnumPlacementTestLevel.IELTS)
                .Select(x => new
                {
                    x.Status,
                    x.StudentId,
                    x.SuggetLevel,
                    x.CurrentLevel
                })
                .ToListAsync(cancellationToken);

            var overallReportPlacementTest = new OverallReportPlacementTestModel
            {
                TotalStudent = studentIds.Count,
                TotalPlacementTest = placementTestGroupResults.Count,
                TotalCompletePlacementTest = placementTestGroupResults.Where(x => x.Status == EnumResultStatus.Done).Count(),
            };

            if (request.CompletionStatuses != null && request.CompletionStatuses.Any())
            {
                var isCompleted = request.CompletionStatuses.Contains(EnumCompletionStatus.Completed);
                var isInProgress = request.CompletionStatuses.Contains(EnumCompletionStatus.InProgress);

                overallReportPlacementTest.TotalPlacementTest = placementTestGroupResults
                        .Where(x => (isCompleted && x.Status == EnumResultStatus.Done) || (isInProgress && x.Status != EnumResultStatus.Done))
                        .Select(x => x.StudentId)
                        .Distinct()
                        .Count();
            }

            var testGroupResults = placementTestGroupResults.Where(x => x.Status == EnumResultStatus.Done && x.CurrentLevel.HasValue)
                                                            .GroupBy(x => x.SuggetLevel)
                                                            .Select(x => new
                                                            {
                                                                CurrentLevel = x.Key,
                                                                CountStudent = x.Count()
                                                            })
                                                            .ToList();

            //foreach (var item in courseLevels)
            //{
            //    var studentLevel = testGroupResults.FirstOrDefault(x => x.CurrentLevel == item);
            //    overallReportPlacementTest.CourseLevelProgresses.Add(new CourseLevelProgressModel
            //    {
            //        CourseLevel = item,
            //        TotalStudent = studentLevel?.CountStudent ?? default
            //    });
            //}
            methodResult.Result = overallReportPlacementTest;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
