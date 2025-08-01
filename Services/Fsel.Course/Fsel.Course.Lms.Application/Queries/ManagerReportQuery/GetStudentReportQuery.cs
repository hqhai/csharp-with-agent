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
            var students = await GetInitialStudentsAsync(methodResult, request);
            if (!methodResult.IsOK)
            {
                return methodResult;
            }
            switch (request.ManagerReportType)
            {
                case EnumManagerReportType.ReportManagerPT:
                    students = await FilterPlacementTestStudentsAsync(request, students, cancellationToken);
                    break;

                case EnumManagerReportType.ReportLearningProgress:
                    students = await FilterLearningProgressStudentsAsync(request, students);
                    break;

                case EnumManagerReportType.ReportLearningResults:
                    students = await FilterLearningResultStudentsAsync(request, students, cancellationToken);
                    break;
            }

            if (!request.SortBy.Any())
            {
                students = students.OrderBy(x => int.TryParse(x.SchoolGrade, out int graded) ? graded : 0).ThenBy(x => x.SchoolClass).ThenBy(x => x.FullName).ToList();
                if (request.IsSearchReport)
                {
                    students = students.ApplyPaging(request).ToList();
                }
            }

            methodResult.Result = students;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<IList<StudentDtoModel>> FilterPlacementTestStudentsAsync(GetStudentReportQuery request, IList<StudentDtoModel> students, CancellationToken cancellationToken)
        {
            var studentIds = students.Select(x => x.Id).ToList();
            bool isCheckDate = request.StartDate.HasValue || request.EndDate.HasValue;
            bool hasLevelFilter = request.CurrentLevels?.Any() == true || request.CourseLevels?.Any() == true;
            bool hasCompletionFilter = request.CompletionStatuses?.Any() == true;

            if (!hasCompletionFilter && !isCheckDate && !hasLevelFilter)
            {
                return students;
            }

            if (isCheckDate)
            {
                studentIds = await _placementTestResultRepository.GetStudentPtIdsAsync(request.StartDate, request.EndDate, studentIds);
            }

            var studentPtGroups = await _placementTestGroupResultRepository.GetStudentIdsAsync(request.CompletionStatuses, studentIds, request.CurrentLevels, request.CourseLevels);
            var studentPTIds = studentPtGroups.ToHashSet();

            bool filterInProgress = hasCompletionFilter && request.CompletionStatuses != null && request.CompletionStatuses.Contains(EnumCompletionStatus.InProgress);
            if (filterInProgress && !hasLevelFilter && studentIds.Any())
            {
                var studentHasLearned = await _placementTestGroupResultRepository.Queryable
                    .WhereBulkContains(studentIds, x => x.StudentId)
                    .Select(x => x.StudentId)
                    .ToListAsync(cancellationToken);

                var studentNotLearned = studentIds.Except(studentHasLearned).ToList();
                studentPTIds.UnionWith(studentNotLearned);
            }

            return students.Where(x => studentPTIds.Contains(x.Id)).ToList();
        }

        private async Task<IList<StudentDtoModel>> FilterLearningProgressStudentsAsync(GetStudentReportQuery request, IList<StudentDtoModel> students)
        {
            var courseResults = students.Select(x => new CourseResultModel { CourseId = x.CourseId.GetValueOrDefault(), StudentId = x.Id }).ToList();

            if (!request.SortBy.Any())
            {
                var courseLearnIds = (await _managerProgressHelper.GetCourseLearnsAsync(courseResults)).Select(x => x.StudentId).ToHashSet();
                return students.Where(x => courseLearnIds.Contains(x.Id)).ToList();
            }
            else
            {
                IList<CourseCompleteModel> courseCompletes;
                if (request.SortBy.Any(x => x.Property == nameof(CourseCompleteModel.UnitDisplayOrder)))
                {
                    courseCompletes = await _managerProgressHelper.GetCourseCompletesFilterAsync(courseResults, request, request.EndDate, request.IsSearchReport);
                }
                else
                {
                    courseCompletes = await _managerProgressHelper.GetCourseCompletesFilterCountCompleteAsync(courseResults, request, request.EndDate, request.IsSearchReport);
                }

                var studentLearnIds = courseCompletes.Select(x => x.StudentId).ToHashSet();
                return students.Where(x => studentLearnIds.Contains(x.Id)).OrderBy(x => studentLearnIds.ToList().IndexOf(x.Id)).ToList();
            }
        }

        private async Task<IList<StudentDtoModel>> FilterLearningResultStudentsAsync(GetStudentReportQuery request, IList<StudentDtoModel> students, CancellationToken cancellationToken)
        {
            var studentIds = students.Select(x => x.Id).ToList();
            var is75OrMore = request.OverallScores?.Contains(EnumOverallScore.Accuracy75OrMore) == true;
            var isLessThan75 = request.OverallScores?.Contains(EnumOverallScore.AccuracyBelow75) == true;

            var query = (from baseQ in _courseResultRepository.Queryable.WhereBulkContains(studentIds, x => x.StudentId)
                         join cum in _courseUnitMockTestRepository.Queryable on baseQ.CourseId equals cum.CourseId
                         join ur in _unitResultRepository.Queryable on new { baseQ.StudentId, baseQ.CourseId, UnitId = cum.UnitId } equals new { ur.StudentId, ur.CourseId, UnitId = (Guid?)ur.UnitId } into unitGroup
                         from ur in unitGroup.DefaultIfEmpty()
                         where baseQ.WorkingStatus == EnumWorkingStatus.Active &&
                         (!request.EndDate.HasValue || (ur.UpdatedDate ?? ur.CreatedDate).Date <= request.EndDate.Value.Date)
                         group new { baseQ, ur } by new { baseQ.CourseId, baseQ.StudentId } into g
                         select new
                         {
                             StudentId = g.Key.StudentId,
                             OverallPercent = g.Select(x => x.ur).Where(x => x.Status == EnumResultStatus.Done).Any()
                                 ? Math.Round(g.Select(x => x.ur).Where(x => x.Status == EnumResultStatus.Done).Average(x => x.Percent))
                                 : default,
                         })
                        .Where(x => request.OverallScores == null || !request.OverallScores.Any() ||
                               (is75OrMore && x.OverallPercent >= (int)EnumOverallScore.Accuracy75OrMore) ||
                               (isLessThan75 && x.OverallPercent < (int)EnumOverallScore.Accuracy75OrMore));

            if (request.SortBy.Any())
            {
                query = query.ApplySortAndPaging(request);
            }

            var unitResultGroups = await query.ToListAsync(cancellationToken);

            var studentResultIds = unitResultGroups.Select(x => x.StudentId).ToHashSet();
            return students.Where(x => studentResultIds.Contains(x.Id))
                           .OrderBy(x => studentResultIds.ToList().IndexOf(x.Id)).ToList();
        }

        private async Task<IList<StudentDtoModel>> GetInitialStudentsAsync(MethodResult<IList<StudentDtoModel>> methodResult, GetStudentReportQuery request)
        {
            var searchQuery = new SearchStudentSchoolQueryModel
            {
                ListSchoolClass = request.ListSchoolClass,
                ListSchoolGrade = request.ListSchoolGrade,
                ListDistrict = request.ListDistrict,
                ListProvince = request.ListProvince,
                ListSchool = request.ListSchool,
                ListCourseLevel = request.ListCourseLevel,
                Keyword = request.Keyword,
                ListLearningStatus = request.ListLearningStatus,

                CourseType = request.CourseType,
                IsLearning = request.ManagerReportType != EnumManagerReportType.ReportManagerPT ? true : null,
            };

            var userResults = await _userService.GetStudentsSchoolAsync(searchQuery);
            if (!userResults.IsSuccessStatusCode)
            {
                methodResult.AddError(userResults.Error);
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return new List<StudentDtoModel>();
            }

            return userResults.Content?.Result ?? new List<StudentDtoModel>();
        }
    }
}
