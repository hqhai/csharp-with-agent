// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ManagerReportQuery
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchReportPlacementTestByAdminQuery : BaseQueryModel, IRequest<MethodResult<PagingItemsModel<PlacementTestResultExportModel>>>
    {
        public IList<Guid>? SchoolIds { get; set; }
        public IList<Guid>? ProvinceIds { get; set; }
        public IList<Guid>? DistrictIds { get; set; }
        public EnumCompletionStatus? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class SearchReportPlacementTestByAdminQueryHandler : IRequestHandler<SearchReportPlacementTestByAdminQuery, MethodResult<PagingItemsModel<PlacementTestResultExportModel>>>
    {
        private readonly IUserService _userService;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly IPlacementTestResultRepository _placementTestResultRepository;
        private readonly ISystemService _systemService;

        public SearchReportPlacementTestByAdminQueryHandler(
            IUserService userService,
            ICourseResultRepository courseResultRepository,
            IPlacementTestResultRepository placementTestResultRepository,
            ISystemService systemService)
        {
            _userService = userService;
            _courseResultRepository = courseResultRepository;
            _placementTestResultRepository = placementTestResultRepository;
            _systemService = systemService;
        }

        public async Task<MethodResult<PagingItemsModel<PlacementTestResultExportModel>>> Handle(SearchReportPlacementTestByAdminQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<PlacementTestResultExportModel>>();
            var query = _placementTestResultRepository.Queryable
                         .GroupBy(x => x.StudentId)
                         .Select(g => new
                         {
                             StudentId = g.Key,
                             CompletionDate = g.Max(x => x.UpdatedDate ?? x.CreatedDate),
                             FirstResult = g.OrderBy(x => x.CreatedDate).FirstOrDefault(),
                             LastResult = g.OrderByDescending(x => x.UpdatedDate).ThenByDescending(x => x.CreatedDate).FirstOrDefault()
                         });

            if (!string.IsNullOrEmpty(request.Keyword))
            {
            }
            if (request.StartDate.HasValue)
            {
                query = query.Where(m => m.CompletionDate >= request.StartDate.Value.ConvertTimeToUtc(EnumCountryKey.Vietnam));
            }
            if (request.EndDate.HasValue)
            {
                query = query.Where(m => m.CompletionDate <= request.EndDate.Value.ConvertTimeToUtc(EnumCountryKey.Vietnam));
            }
            int totalItem = await query.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await query
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            var placementTestResultFirsts = lists.Select(x => x.FirstResult).ToList();
            var placementTestResultLasts = lists.Select(x => x.LastResult).ToList();
            var studentIds = lists.Select(x => x.StudentId).ToList();

            var courseResults = await _courseResultRepository.Queryable.Include(x => x.Course)
                                     .Where(x => studentIds.Contains(x.StudentId) && x.WorkingStatus == EnumWorkingStatus.Active)
                                     .ToListAsync(cancellationToken);

            var studentResult = await _userService.GetStudentsByStudentIdsAsync(studentIds);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddError(studentResult.Error);
                return methodResult;
            }
            var students = studentResult.Content?.Result?.ToList();
            var schoolIds = students?.Where(x => x.SchoolId.HasValue).Select(x => x.SchoolId.GetValueOrDefault()).ToList();
            var schoolResults = await _systemService.GetSchoolsAsync(schoolIds);
            if (!schoolResults.IsSuccessStatusCode)
            {
                methodResult.AddError(schoolResults.Error);
                return methodResult;
            }

            var schools = schoolResults.Content?.Result;
            //foreach (var item in students)
            //{
            //    if (item == null)
            //    {
            //        continue;
            //    }
            //    var courseResult = courseResults.FirstOrDefault(x => x.StudentId == item.Id);
            //    var placementTestResultExport = new PlacementTestResultExportModel
            //    {
            //        Name = item.Human?.FullName,
            //        Birthday = item.Human?.Birthday,
            //        PhoneNumber = item.Human?.PhoneNumber,
            //        SchoolClass = item.SchoolClass,
            //        SchoolGrade = item.SchoolGrade,
            //        Email = item.Human?.Email,
            //        SchoolName = item.School ?? schools?.FirstOrDefault(x => x.Id == item.SchoolId)?.Name,
            //        CourseName = courseResult?.Course?.Name,
            //    };

            //    var placementTestResultLast = placementTestResultLasts.FirstOrDefault(x => x.StudentId == item.Id);
            //    if (placementTestResultLast != null)
            //    {
            //        var placementTestResultFirst = placementTestResultFirsts.FirstOrDefault(x => x.StudentId == item.Id);
            //        int age = Shared.Helpers.DateTimeHelper.GetYearOld(item.Human?.Birthday);
            //        var (levelCompleted, isLock) = placementTestResultLast.Level.GetLevelInScore(placementTestResultLast.Percent, IeltsScoreHelper.GetInitialAge(placementTestResultFirst?.Level, age));

            //        placementTestResultExport.CourseLevel = placementTestResultLast.Level.GetCourseLevelByPlacementTestLevel();
            //        placementTestResultExport.CurrentLevel = isLock ? item.CourseLevel : null;
            //        placementTestResultExport.LevelCompleted = placementTestResultLast.Status == EnumResultStatus.Done ? levelCompleted : placementTestResultExport.CourseLevel;
            //        placementTestResultExport.Percent = placementTestResultLast.Percent;
            //        placementTestResultExport.IsPTdone = isLock;
            //        placementTestResultExport.UpdatedDate = placementTestResultLast.UpdatedDate.HasValue ? placementTestResultLast.UpdatedDate.Value : null;
            //    }
            //    placementTestResultExports.Add(placementTestResultExport);
            //}
            //methodResult.Result = new PagingItemsModel<FinalTestSearchModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
