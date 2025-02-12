// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ManagerReportQuery
{
    using System.Collections.Generic;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.QueryModels.ManagerReports;
    using Fsel.Course.Infrastructure.Common;
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
        public EnumManagerReportType ManagerReportType { get; set; }
    }

    public class GetStudentReportQueryHandler : IRequestHandler<GetStudentReportQuery, MethodResult<IList<StudentDtoModel>>>
    {
        private readonly IUserService _userService;
        private readonly IPlacementTestResultRepository _placementTestResultRepository;
        private readonly IPlacementTestGroupResultRepository _placementTestGroupResultRepository;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly IUnitResultRepository _unitResultRepository;
        private readonly ICourseUnitMockTestRepository _courseUnitMockTestRepository;
        private readonly ManagerProgressHelper _managerProgressHelper;

        public GetStudentReportQueryHandler(IUserService userService,
            IPlacementTestResultRepository placementTestResultRepository,
            IPlacementTestGroupResultRepository placementTestGroupResultRepository,
            ICourseResultRepository courseResultRepository,
            IUnitResultRepository unitResultRepository,
            ICourseUnitMockTestRepository courseUnitMockTestRepository,
            ManagerProgressHelper managerProgressHelper)
        {
            _userService = userService;
            _placementTestResultRepository = placementTestResultRepository;
            _placementTestGroupResultRepository = placementTestGroupResultRepository;
            _courseResultRepository = courseResultRepository;
            _unitResultRepository = unitResultRepository;
            _courseUnitMockTestRepository = courseUnitMockTestRepository;
            _managerProgressHelper = managerProgressHelper;
        }

        public async Task<MethodResult<IList<StudentDtoModel>>> Handle(GetStudentReportQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<StudentDtoModel>>();
            var searchQuery = new SearchStudentSchoolQueryModel
            {
                SchoolGrade = request.SchoolGrade,
                SchoolClass = request.SchoolClass,
                ListDistrict = request.ListDistrict,
                ListProvince = request.ListProvince,
                ListSchool = request.ListSchool,
                Keyword = request.Keyword,
                LearningStatus = request.LearningStatus,
                CourseLevel = request.CourseLevel,
                CourseType = request.CourseType,
                Status = request.Status,
                IsLearning = request.ManagerReportType != EnumManagerReportType.ReportManagerPT ? true : null,
            };
            var userResults = await _userService.GetStudentsSchoolAsync(searchQuery);
            if (!userResults.IsSuccessStatusCode)
            {
                methodResult.AddError(userResults.Error);
                return methodResult;
            }
            var students = userResults.Content?.Result;
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

                    if (isCheckDate || (request.Status.HasValue && request.Status == EnumCompletionStatus.InProgress) || (request.CurrentLevel.HasValue || request.CourseLevel.HasValue))
                    {
                        students = students?.Where(x => studentPtGroups.Contains(x.Id)).ToList();
                    }
                    break;

                case EnumManagerReportType.ReportLearningProgress:
                    var courseResults = students?.Select(x => new CourseResultModel { CourseId = x.CourseId.GetValueOrDefault(), StudentId = x.Id }).ToList();
                    if (!request.SortBy.Any())
                    {
                        var courseLearns = await _managerProgressHelper.GetCourseLearnsAsync(courseResults);
                        students = students?.Where(x => courseLearns.Select(y => y.StudentId).Contains(x.Id)).ToList();
                    }
                    else if (request.IsSearchReport && request.SortBy.Any())
                    {
                        var courseCompletes = await _managerProgressHelper.GetCourseCompletesAsync(courseResults, request, request.EndDate);
                        students = students?.Where(x => courseCompletes.Select(y => y.StudentId).Contains(x.Id)).OrderBy(x => courseCompletes.Select(y => y.StudentId).ToList().IndexOf(x.Id)).ToList();
                    }
                    break;

                case EnumManagerReportType.ReportLearningResults:
                    studentIds = students?.Select(x => x.Id).ToList() ?? new List<Guid>();
                    var query = (from baseQ in _courseResultRepository.Queryable.WhereBulkContains(studentIds, x => x.StudentId)
                                 join cum in _courseUnitMockTestRepository.Queryable on baseQ.CourseId equals cum.CourseId
                                 join ur in _unitResultRepository.Queryable on new { baseQ.StudentId, baseQ.CourseId, UnitId = cum.UnitId } equals new { ur.StudentId, ur.CourseId, UnitId = (Guid?)ur.UnitId } into unitGroup
                                 from ur in unitGroup.DefaultIfEmpty()
                                 where baseQ.WorkingStatus == EnumWorkingStatus.Active &&
                                 (!request.EndDate.HasValue || (ur.UpdatedDate ?? ur.CreatedDate).Date <= request.EndDate.Value.Date)
                                 group new { baseQ, ur }
                                 by new { baseQ.CourseId, baseQ.StudentId } into g
                                 select new
                                 {
                                     StudentId = g.Key.StudentId,
                                     OverallPercent = g.Select(x => x.ur).Where(x => x.Status == EnumResultStatus.Done).Any() ? Math.Round(g.Select(x => x.ur).Where(x => x.Status == EnumResultStatus.Done).Average(x => x.Percent)) : default,
                                 }).Where(x => !request.OverallScore.HasValue || (request.OverallScore == EnumOverallScore.Accuracy75OrMore ? x.OverallPercent >= (int)EnumOverallScore.Accuracy75OrMore : x.OverallPercent < (int)EnumOverallScore.Accuracy75OrMore));

                    if (request.SortBy.Any())
                    {
                        query = query.ApplySortAndPaging(request);
                    }
                    var unitResultGroups = await query.ToListAsync(cancellationToken);
                    students = students?.Where(x => unitResultGroups.Select(x => x.StudentId).Contains(x.Id)).OrderBy(x => unitResultGroups.Select(y => y.StudentId).ToList().IndexOf(x.Id)).ToList();
                    break;
            }
            if (!request.SortBy.Any())
            {
                students = students?.OrderBy(x => int.TryParse(x.SchoolGrade, out int graded) ? graded : 0).ThenBy(x => x.SchoolClass).ThenBy(x => x.FullName).ToList();
                if (request.IsSearchReport)
                {
                    students = students?.ApplyPaging(request).ToList();
                }
            }

            methodResult.Result = students;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
