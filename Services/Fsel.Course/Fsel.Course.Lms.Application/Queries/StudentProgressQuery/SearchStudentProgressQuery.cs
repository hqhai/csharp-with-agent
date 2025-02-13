// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.StudentProgressQuery
{
    using System.Linq;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.QueryModels.StudentProgress;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.Models;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchStudentProgressQuery : SearchStudentProgressQueryModel, IRequest<MethodResult<PagingItemsModel<StudentProgressModel>>>
    {
    }

    public class SearchStudentProgressQueryHandler : IRequestHandler<SearchStudentProgressQuery, MethodResult<PagingItemsModel<StudentProgressModel>>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly ManagerProgressHelper _managerProgressHelper;
        private readonly IUserService _userService;
        private readonly AuthContext _authContext;

        public SearchStudentProgressQueryHandler(ICourseRepository courseRepository,
            ICourseResultRepository courseResultRepository,
            ManagerProgressHelper managerProgressHelper,
            IUserService userService,
            AuthContext authContext)
        {
            _courseRepository = courseRepository;
            _courseResultRepository = courseResultRepository;
            _managerProgressHelper = managerProgressHelper;
            _userService = userService;
            _authContext = authContext;
        }

        public async Task<MethodResult<PagingItemsModel<StudentProgressModel>>> Handle(SearchStudentProgressQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<StudentProgressModel>>();

            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }

            var query = _courseResultRepository.Queryable.Include(x => x.Course).Where(x => !x.IsDeleted && x.WorkingStatus == EnumWorkingStatus.Active)
                .GroupBy(r => new { r.StudentId, r.CourseId })
                .Select(group => new CourseResultModel
                {
                    StudentId = group.Key.StudentId,
                    CourseId = group.Key.CourseId,
                    CourseType = group.Select(x => x.Course).FirstOrDefault(c => c!.Id == group.Key.CourseId)!.CourseType,
                    CourseLevel = group.Select(x => x.Course).FirstOrDefault(c => c!.Id == group.Key.CourseId)!.CourseLevel,
                    CreatedDate = group.Max(r => r.CreatedDate),
                    UpdatedDate = group.Max(r => r.UpdatedDate),
                });

            if (_authContext.Roles != null && _authContext.Roles.Contains(EnumRole.AdminSchool.ToString()))
            {
                var studentSchoolResult = await _userService.GetStudentsToAdminSchoolAsync();
                if (!studentSchoolResult.IsSuccessStatusCode)
                {
                    methodResult.AddError(studentSchoolResult.Error);
                    return methodResult;
                }
                var studentIds = studentSchoolResult.Content?.Result?.Select(x => x.Id).ToList() ?? new List<Guid>();
                query = query.WhereBulkContains(studentIds, x => x.StudentId);
            }

            if (request.CourseType.HasValue)
            {
                query = query.Where(x => x.CourseLevel.HasValue && request.CourseType.GetEnumCourseLevels().Contains(x.CourseLevel.Value));
            }
            if (request.Level.HasValue)
            {
                query = query.Where(m => m.CourseLevel == request.Level);
            }
            if (!string.IsNullOrEmpty(request.Keyword))
            {
                var studentKeyResult = await _userService.SearchStudentAsync(new BaseQueryModel { Keyword = request.Keyword });
                if (studentKeyResult.IsSuccessStatusCode)
                {
                    var studentIds = studentKeyResult.Content?.Result?.Items?.Select(x => x.Id).ToList() ?? new List<Guid>();
                    query = query.WhereBulkContains(studentIds, x => x.StudentId);
                }
            }

            var totalItem = await query.CountAsync(cancellationToken);
            var lists = await query.OrderByDescending(x => x.UpdatedDate)
                               .ThenByDescending(x => x.CreatedDate)
                               .ApplyPaging(request)
                               .AsNoTracking()
                               .ToListAsync(cancellationToken: cancellationToken)
                               .ConfigureAwait(false);
            var studentResults = await _userService.GetStudentsByStudentIdsAsync(lists.Select(x => x.StudentId).Distinct().ToList());
            var students = studentResults.Content?.Result;
            var studentProgress = new List<StudentProgressModel>();
            foreach (var courseResult in lists)
            {
                var student = students?.FirstOrDefault(x => x.Id == courseResult.StudentId);
                var (currentProgress, progress) = await _managerProgressHelper.GetCompleteCourseAsync(courseResult);
                var (displayOrderUnit, displayOrderLesson) = await _courseRepository.GetDisplayOrder(courseResult);
                StudentProgressModel studentProgressModel = new StudentProgressModel
                {
                    StudentId = courseResult.StudentId,
                    FullName = student?.Human?.FullName,
                    Email = student?.Human?.Email,
                    Level = courseResult.CourseLevel ?? default,
                    CourseType = courseResult.CourseType ?? default,
                    CourseId = courseResult.CourseId,
                    CreatedDate = courseResult.CreatedDate ?? default,
                    UpdatedDate = courseResult.UpdatedDate ?? default,
                    DisplayOrderLesson = displayOrderLesson,
                    DisplayOrderUnit = displayOrderUnit,
                    ContentProgress = string.Format("{0} / {1}", currentProgress, progress),
                };
                studentProgress.Add(studentProgressModel);
            }
            methodResult.Result = new PagingItemsModel<StudentProgressModel>(studentProgress, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
