// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ManagerReportQuery
{
    using System;
    using System.Diagnostics;
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
                Status = request.Status,
                SortBy = request.SortBy,
                StartDate = request.StartDate,
                CourseLevel = request.CourseLevel,
                CurrentLevel = request.CurrentLevel,
                ManagerReportType = EnumManagerReportType.ReportManagerPT,
            }, cancellationToken);
            if (!userResults.IsOK)
            {
                methodResult.AddError(userResults.ErrorMessages);
                return methodResult;
            }
            var students = userResults?.Result;
            var studentIds = students?.Select(x => x.Id).ToList() ?? new List<Guid>();
            var query = _placementTestGroupResultRepository.Queryable.WhereBulkContains(studentIds, x => x.StudentId).Where(x => x.CompletionLevel != EnumPlacementTestLevel.IELTS);
            var overallReportPlacementTest = new OverallReportPlacementTestModel
            {
                TotalStudent = studentIds.Count,
                TotalPlacementTest = await query.CountAsync(cancellationToken),
                TotalCompletePlacementTest = await query.Where(x => x.Status == EnumResultStatus.Done).CountAsync(cancellationToken),
            };
            if (request.Status.HasValue)
            {
                switch (request.Status.Value)
                {
                    case EnumCompletionStatus.Completed:
                        overallReportPlacementTest.TotalPlacementTest = overallReportPlacementTest.TotalPlacementTest;
                        break;

                    case EnumCompletionStatus.InProgress:
                        overallReportPlacementTest.TotalPlacementTest = await query.Where(x => x.Status != EnumResultStatus.Done).Select(x => x.StudentId).CountAsync(cancellationToken);
                        break;
                }
            }
            var courseLevels = EnumCourseLevelHelper.GetEnumCourseLevels(EnumCourseType.Academic);
            var placementTestGroupResults = (await query.Where(x => x.Status == EnumResultStatus.Done && x.CurrentLevel.HasValue).ToListAsync(cancellationToken))
                                                        .GroupBy(x => x.SuggetLevel)
                                                        .Select(x => new
                                                        {
                                                            CurrentLevel = x.Key,
                                                            CountStudent = x.Count()
                                                        })
                                                        .ToList();

            overallReportPlacementTest.CourseLevelProgresses = new List<CourseLevelProgressModel>();
            foreach (var item in courseLevels)
            {
                var studentLevel = placementTestGroupResults.FirstOrDefault(x => x.CurrentLevel == item);
                overallReportPlacementTest.CourseLevelProgresses.Add(new CourseLevelProgressModel
                {
                    CourseLevel = item,
                    TotalStudent = studentLevel?.CountStudent ?? default
                });
            }
            methodResult.Result = overallReportPlacementTest;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
