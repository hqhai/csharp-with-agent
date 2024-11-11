// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.ManagerReportQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Shared.Models.ShareModels.EntityModels;
    using Fsel.System.Application.Queries.SchoolQuery;
    using Fsel.System.Application.Services.UserServices;
    using Fsel.System.Application.Services.UserServices.QueryModels;
    using Fsel.System.Domain.Models.QueryModels.ManagerReports;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class GetStudentReportQuery : SearchStudentReportQueryModel, IRequest<MethodResult<IList<StudentDtoModel>>>
    {
        public bool IsSearchReport { get; set; }
    }

    public class GetStudentReportQueryHandler : IRequestHandler<GetStudentReportQuery, MethodResult<IList<StudentDtoModel>>>
    {
        private readonly IUserService _userService;
        private readonly IMediator _mediator;

        public GetStudentReportQueryHandler(IUserService userService, IMediator mediator)
        {
            _userService = userService;
            _mediator = mediator;
        }

        public async Task<MethodResult<IList<StudentDtoModel>>> Handle(GetStudentReportQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<StudentDtoModel>>();
            var schoolIdResults = await _mediator.Send(new GetSchoolsQuery
            {
                DistrictIds = request.DistrictIds,
                ProvinceIds = request.ProvinceIds,
                SchoolIds = request.SchoolIds,
            }, cancellationToken);
            var schoolIds = schoolIdResults.Result;

            var searchQuery = new SearchStudentSchoolQueryModel
            {
                SchoolGrade = request.SchoolGrade,
                SchoolClass = request.SchoolClass,
                Filters = request.Filters,
                IncludePaths = request.IncludePaths,
                Keyword = request.Keyword,
                Page = request.Page,
                PageSize = request.PageSize,
                SortBy = request.SortBy,
                LearningStatus = request.LearningStatus,
                ListSchoolId = schoolIds != null && schoolIds.Any() ? string.Join(",", schoolIds) : null,
                CourseType = request.CourseType,
                IsLearning = true,
            };
            if (request.IsSearchReport)
            {
                var userResults = await _userService.SearchStudentSchoolAsync(searchQuery);
                if (!userResults.IsSuccessStatusCode)
                {
                    methodResult.AddError(userResults.Error);
                    return methodResult;
                }
                methodResult.Result = userResults.Content?.Result?.Items;
            }
            else
            {
                var userResults = await _userService.GetStudentsSchoolAsync(searchQuery);
                if (!userResults.IsSuccessStatusCode)
                {
                    methodResult.AddError(userResults.Error);
                    return methodResult;
                }
                methodResult.Result = userResults.Content?.Result;
            }
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
