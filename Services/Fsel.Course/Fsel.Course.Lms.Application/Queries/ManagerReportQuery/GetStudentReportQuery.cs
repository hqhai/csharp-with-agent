// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ManagerReportQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.QueryModels.ManagerReports;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.SystemService.QueryModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.QueryModels;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.ShareModels.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetStudentReportQuery : SearchStudentReportQueryModel, IRequest<MethodResult<IList<StudentDtoModel>>>
    {
        public bool IsSearchReport { get; set; }
        public bool IsListGuid { get; set; }
        public EnumManagerReportType ManagerReportType { get; set; }
    }

    public class GetStudentReportQueryHandler : IRequestHandler<GetStudentReportQuery, MethodResult<IList<StudentDtoModel>>>
    {
        private readonly IUserService _userService;
        private readonly ISystemService _systemService;
        private readonly IPlacementTestResultRepository _placementTestResultRepository;
        private readonly IPlacementTestGroupResultRepository _placementTestGroupResultRepository;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly IUnitResultRepository _unitResultRepository;
        private readonly ICourseUnitMockTestRepository _courseUnitMockTestRepository;

        public GetStudentReportQueryHandler(IUserService userService,
            ISystemService systemService,
            IPlacementTestResultRepository placementTestResultRepository,
            IPlacementTestGroupResultRepository placementTestGroupResultRepository,
            ICourseResultRepository courseResultRepository,
            IUnitResultRepository unitResultRepository,
            ICourseUnitMockTestRepository courseUnitMockTestRepository)
        {
            _userService = userService;
            _systemService = systemService;
            _placementTestResultRepository = placementTestResultRepository;
            _placementTestGroupResultRepository = placementTestGroupResultRepository;
            _courseResultRepository = courseResultRepository;
            _unitResultRepository = unitResultRepository;
            _courseUnitMockTestRepository = courseUnitMockTestRepository;
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
                CourseLevel = request.CourseLevel,
                CourseType = request.CourseType,
                Status = request.Status,
                ListSchoolId = schoolIds != null && schoolIds.Any() ? string.Join(",", schoolIds) : null,
            };
            List<Guid> studentIds = new List<Guid>();
            switch (request.ManagerReportType)
            {
                case EnumManagerReportType.ReportManagerPT:
                    bool isCheckDate = request.StartDate.HasValue || request.EndDate.HasValue;
                    if (isCheckDate)
                    {
                        studentIds = await _placementTestResultRepository.GetStudentPtIdsAsync(request.StartDate, request.EndDate);
                    }
                    var studentPtGroups = await _placementTestGroupResultRepository.GetStudentIdsAsync(request.Status, studentIds, isCheckDate, request.CurrentLevel, request.CourseLevel);
                    studentIds = studentPtGroups.ToList();
                    searchQuery.IsCheckDate = isCheckDate;
                    break;

                case EnumManagerReportType.ReportLearningProgress:
                    searchQuery.IsLearning = true;
                    break;

                case EnumManagerReportType.ReportLearningResults:
                    var userSchoolResults = await _userService.GetStudentsToAdminSchoolAsync();
                    studentIds = userSchoolResults.Content?.Result?.Select(x => x.Id).ToList() ?? new List<Guid>();
                    if (request.OverallScore.HasValue)
                    {
                        var unitResultGroups = await (from baseQ in _courseResultRepository.Queryable
                                                      join cum in _courseUnitMockTestRepository.Queryable on baseQ.CourseId equals cum.CourseId
                                                      join ur in _unitResultRepository.Queryable on new { baseQ.StudentId, baseQ.CourseId, UnitId = cum.UnitId } equals new { ur.StudentId, ur.CourseId, UnitId = (Guid?)ur.UnitId } into unitGroup
                                                      from ur in unitGroup.DefaultIfEmpty()
                                                      where studentIds.Contains(baseQ.StudentId) && baseQ.WorkingStatus == EnumWorkingStatus.Active &&
                                                      (!request.EndDate.HasValue || (ur.UpdatedDate ?? ur.CreatedDate).Date <= request.EndDate.Value.Date) && ur.Status == EnumResultStatus.Done
                                                      group new { baseQ, ur }
                                                      by new { baseQ.CourseId, baseQ.StudentId } into g
                                                      select new
                                                      {
                                                          StudentId = g.Key.StudentId,
                                                          OverallPercent = g.Select(x => x.ur).Any() ? g.Select(x => x.ur).Average(x => x.Percent) : default,
                                                      }).ToListAsync(cancellationToken);
                        unitResultGroups = unitResultGroups.Where(x =>
                            (request.OverallScore == EnumOverallScore.Accuracy75OrMore ? NumberHelper.ConvertRound(x.OverallPercent) > (int)EnumOverallScore.Accuracy75OrMore : NumberHelper.ConvertRound(x.OverallPercent) < (int)EnumOverallScore.Accuracy75OrMore))
                            .ToList();
                        studentIds = unitResultGroups.Select(x => x.StudentId).ToList();
                    }
                    searchQuery.IsLearning = true;
                    break;
            }
            searchQuery.ListStudentId = studentIds != null && studentIds.Any() ? string.Join(",", studentIds.Distinct().ToList()) : null;
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
