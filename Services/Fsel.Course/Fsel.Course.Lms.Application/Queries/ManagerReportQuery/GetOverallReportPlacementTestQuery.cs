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
            var query = _placementTestGroupResultRepository.Queryable.Where(x => x.CompletionLevel != EnumPlacementTestLevel.IELTS)
                .Where(x => studentIds.Any(y => y == x.StudentId));
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
                        overallReportPlacementTest.TotalPlacementTest = await query.Where(x => x.Status == EnumResultStatus.Process).CountAsync(cancellationToken);
                        break;

                    case EnumCompletionStatus.NotStarted:
                        var studentPTIds = await query.Select(x => x.StudentId).ToListAsync(cancellationToken);
                        overallReportPlacementTest.TotalPlacementTest = studentIds.Except(studentPTIds).Count();
                        break;
                }
            }
            var courseLevels = EnumCourseLevelHelper.GetEnumCourseLevels(EnumCourseType.Academic);
            var placementTestGroupResults = await query.Where(x => x.Status == EnumResultStatus.Done && x.CompletionLevel.HasValue)
                .GroupBy(x => x.CompletionLevel)
                .Select(x => new
                {
                    CompletionLevel = x.Key,
                    CountStudent = x.Count()
                })
                .ToListAsync(cancellationToken);

            overallReportPlacementTest.CourseLevelProgresses = new List<CourseLevelProgressModel>();
            foreach (var item in courseLevels)
            {
                var studentLevel = placementTestGroupResults.FirstOrDefault(x => x.CompletionLevel.GetValueOrDefault().GetCourseLevelByPlacementTestLevel() == item);
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
