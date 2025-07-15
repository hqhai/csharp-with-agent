// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ManagerReportQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels.ManagerReportModels;
    using Fsel.Course.Domain.Models.QueryModels.ManagerReports;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetReportPlacementTestsQuery : SearchReportPlacementTestQueryModel, IRequest<MethodResult<IList<PlacementTestReportModel>>>
    {
    }

    public class GetReportPlacementTestsQueryHandler : IRequestHandler<GetReportPlacementTestsQuery, MethodResult<IList<PlacementTestReportModel>>>
    {
        private readonly IPlacementTestGroupResultRepository _placementTestGroupResultRepository;
        private readonly IPlacementTestResultRepository _placementTestResultRepository;
        private readonly IMediator _mediator;

        public GetReportPlacementTestsQueryHandler(
            IPlacementTestGroupResultRepository placementTestGroupResultRepository,
            IPlacementTestResultRepository placementTestResultRepository,
            IMediator mediator)
        {
            _placementTestGroupResultRepository = placementTestGroupResultRepository;
            _placementTestResultRepository = placementTestResultRepository;
            _mediator = mediator;
        }

        public async Task<MethodResult<IList<PlacementTestReportModel>>> Handle(GetReportPlacementTestsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<PlacementTestReportModel>>();
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
            if (students == null || !students.Any())
            {
                return methodResult;
            }
            var studentIds = students.Select(x => x.Id).ToList();
            var placementTestGroupResults = await _placementTestGroupResultRepository.Queryable.WhereBulkContains(studentIds, x => x.StudentId).ToListAsync(cancellationToken: cancellationToken);
            var placementTestResults = (await _placementTestResultRepository.Queryable.WhereBulkContains(studentIds, x => x.StudentId).ToListAsync(cancellationToken))
                                        .GroupBy(x => x.StudentId)
                                        .Select(x => x.OrderByDescending(x => x.UpdatedDate ?? x.CreatedDate).FirstOrDefault())
                                        .ToList();

            var data = new List<PlacementTestReportModel>();
            foreach (var item in students)
            {
                var placementTestGroupResult = placementTestGroupResults.FirstOrDefault(x => x.StudentId == item.Id);
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
                    UserName = item.UserName,
                    Status = GetStatus(placementTestGroupResult),
                    ChooseLevel = placementTestGroupResult?.ChooseLevel,
                    CurrentLevel = placementTestGroupResult?.SuggetLevel,
                    ExpiredPTDate = placementTestResult?.UpdatedDate ?? placementTestResult?.CreatedDate,
                });
            }
            methodResult.Result = data;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private static EnumCompletionStatus GetStatus(PlacementTestGroupResult? placementTestGroupResult)
        {
            return placementTestGroupResult != null && placementTestGroupResult.Status == EnumResultStatus.Done ? EnumCompletionStatus.Completed : EnumCompletionStatus.InProgress;
        }
    }
}
