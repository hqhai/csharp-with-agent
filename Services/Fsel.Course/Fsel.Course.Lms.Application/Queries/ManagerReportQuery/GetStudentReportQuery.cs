// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ManagerReportQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.QueryModels.ManagerReports;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.SystemService.QueryModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.QueryModels;
    using Fsel.Shared.Models.ShareModels.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class GetStudentReportQuery : GetReportPlacementTestQueryModel, IRequest<MethodResult<IList<StudentDtoModel>>>
    {
        public bool IsSearchReport { get; set; }
    }

    public class GetStudentReportQueryHandler : IRequestHandler<GetStudentReportQuery, MethodResult<IList<StudentDtoModel>>>
    {
        private readonly IUserService _userService;
        private readonly ISystemService _systemService;
        private readonly IPlacementTestResultRepository _placementTestResultRepository;
        private readonly IPlacementTestGroupResultRepository _placementTestGroupResultRepository;

        public GetStudentReportQueryHandler(IUserService userService,
            ISystemService systemService,
            IPlacementTestResultRepository placementTestResultRepository,
            IPlacementTestGroupResultRepository placementTestGroupResultRepository)
        {
            _userService = userService;
            _systemService = systemService;
            _placementTestResultRepository = placementTestResultRepository;
            _placementTestGroupResultRepository = placementTestGroupResultRepository;
        }

        public async Task<MethodResult<IList<StudentDtoModel>>> Handle(GetStudentReportQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<StudentDtoModel>>();
            var schoolIdResults = await _systemService.GetSchoolIdsAsync(new GetSchoolsQueryModel
            {
                DistrictIds = request.DistrictIds,
                ProvinceIds = request.ProvinceIds,
                SchoolIds = request.SchoolIds,
            });
            var schoolIds = schoolIdResults.Content?.Result;
            List<Guid> studentPtIds = new List<Guid>();
            bool isCheckDate = request.StartDate.HasValue || request.EndDate.HasValue;
            if (isCheckDate)
            {
                studentPtIds = await _placementTestResultRepository.GetStudentPtIdsAsync(request.StartDate, request.EndDate);
            }
            if (request.Status.HasValue && request.Status.Value != Shared.Enums.EnumCompletionStatus.NotStarted)
            {
                studentPtIds.AddRange(await _placementTestGroupResultRepository.GetStudentIdsToStatusAsync(request.Status, studentPtIds, isCheckDate));
            }
            var searchQuery = new SearchStudentSchoolQueryModel
            {
                SchoolGrade = request.SchoolGrade,
                Filters = request.Filters,
                IncludePaths = request.IncludePaths,
                Keyword = request.Keyword,
                Page = request.Page,
                PageSize = request.PageSize,
                SortBy = request.SortBy,
                ListSchoolId = schoolIds != null && schoolIds.Any() ? string.Join(",", schoolIds) : null,
                ListStudentId = studentPtIds != null && studentPtIds.Any() ? string.Join(",", studentPtIds.Distinct().ToList()) : null,
                Status = request.Status,
                IsCheckDate = isCheckDate
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
