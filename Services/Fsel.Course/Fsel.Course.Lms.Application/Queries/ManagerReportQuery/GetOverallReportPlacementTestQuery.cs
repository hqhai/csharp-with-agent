// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ManagerReportQuery
{
    using System;
    using System.Threading.Tasks;
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
                SchoolClass = request.SchoolClass,
                SchoolGrade = request.SchoolGrade,
                EndDate = request.EndDate,
                Keyword = request.Keyword,
                Status = request.Status,
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
            if (students == null)
            {
                return methodResult;
            }
            var studentIds = students.Select(x => x.Id).ToList();
            var placementTestGroupResults = await _placementTestGroupResultRepository.Queryable.Where(x => studentIds.Any(y => y == x.StudentId)).ToListAsync(cancellationToken);
            var overallReportPlacementTest = new OverallReportPlacementTestModel
            {
                TotalStudent = studentIds.Count,
                TotalPlacementTest = placementTestGroupResults.Count,
                TotalCompletePlacementTest = placementTestGroupResults.Where(x => x.Status == EnumResultStatus.Done).Count(),
            };
            if (request.Status.HasValue)
            {
                switch (request.Status.Value)
                {
                    case EnumCompletionStatus.Completed:
                    case EnumCompletionStatus.InProgress:
                        overallReportPlacementTest.TotalPlacementTest = overallReportPlacementTest.TotalPlacementTest;
                        break;

                    default:
                        overallReportPlacementTest.TotalPlacementTest = studentIds.Where(x => !placementTestGroupResults.Any(y => y.StudentId == x)).Count();
                        break;
                }
            }

            GetTotalCourseLevel(overallReportPlacementTest, placementTestGroupResults.Where(x => x.Status == EnumResultStatus.Done).ToList());
            methodResult.Result = overallReportPlacementTest;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private static void GetTotalCourseLevel(OverallReportPlacementTestModel overallReportPlacementTest, IList<PlacementTestGroupResult> placementTestGroupResults)
        {
            var courseLevels = EnumCourseLevelHelper.GetEnumCourseLevels(EnumCourseType.Academic);
            overallReportPlacementTest.CourseLevelProgresses = new List<CourseLevelProgressModel>();
            foreach (var item in courseLevels)
            {
                overallReportPlacementTest.CourseLevelProgresses.Add(new CourseLevelProgressModel
                {
                    CourseLevel = item,
                    TotalStudent = placementTestGroupResults.Where(x => x.CompletionLevel.HasValue && x.CompletionLevel.Value.GetCourseLevelByPlacementTestLevel() == item).Count()
                });
            }
        }
    }
}
