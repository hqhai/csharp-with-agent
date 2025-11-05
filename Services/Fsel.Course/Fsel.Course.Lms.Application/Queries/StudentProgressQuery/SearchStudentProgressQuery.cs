// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.StudentProgressQuery
{
    using System.Linq;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.QueryModels.StudentProgress;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.Models;
    using Fsel.Course.Lms.Application.Services.UserServices.QueryModels;
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
            long totalItem = default;
            var studentProgress = new List<StudentProgressModel>();
            var searchField = request.Serialize().Deserialize<SearchStudentsQueryModel>();

            if (!string.IsNullOrEmpty(request.Keyword) && searchField != null)
            {
                searchField.CourseLevel = request.Level;
                searchField.IsCourseProcess = true;
                var studentKeyResult = await _userService.SearchStudentAsync(searchField);

                var students = studentKeyResult.Content?.Result?.Items ?? new List<StudentSearchAdminModel>();
                totalItem = studentKeyResult.Content?.Result?.PagingInfo?.TotalItems ?? default;

                var courseResults = await _managerProgressHelper.GetProgressCompleteModuleAsync(students.Where(x => x.CourseId.HasValue).Select(x => new CourseResultModel
                {
                    CourseId = x.CourseId ?? default,
                    StudentId = x.Id
                }).ToList());
                foreach (var student in students)
                {
                    if (student == null)
                    {
                        continue;
                    }
                    var courseResult = courseResults.FirstOrDefault(x => x.StudentId == student.Id);
                    StudentProgressModel studentProgressModel = new StudentProgressModel
                    {
                        StudentId = student.Id,
                        FullName = student.FullName,
                        Email = student.Email,
                        Level = student.CourseLevel ?? default,
                        CourseType = student.CourseLevel.GetEnumCourseType(),
                        CourseId = student.CourseId,
                        DisplayOrderLesson = courseResult?.LessonDisplayOrder ?? default,
                        DisplayOrderUnit = courseResult?.UnitDisplayOrder ?? default,
                        ContentProgress = string.Format("{0} / {1}", courseResult?.CountComplete, courseResult?.TotalComplete),
                    };
                    studentProgress.Add(studentProgressModel);
                }
            }
            else
            {
                var query = from baseQ in _courseResultRepository.Queryable
                            join c in _courseRepository.Queryable on baseQ.CourseId equals c.Id
                            where !baseQ.IsDeleted && baseQ.WorkingStatus == EnumWorkingStatus.Active
                            select new CourseResultModel
                            {
                                StudentId = baseQ.StudentId,
                                CourseId = baseQ.CourseId,
                                CourseLevel = c.CourseLevel,
                                CreatedDate = baseQ.CreatedDate,
                                UpdatedDate = baseQ.UpdatedDate,
                            };
                var targetRoles = new List<string> { EnumRole.AdminSchool.ToString(), EnumRole.TeacherCampus.ToString(), EnumRole.AdminCampus.ToString() };
                var hasMatchedRole = _authContext.Roles != null && _authContext.Roles.Any(r => targetRoles.Contains(r));
                if (hasMatchedRole)
                {
                    var studentSchoolResult = await _userService.GetStudentsToAdminSchoolAsync();
                    if (!studentSchoolResult.IsSuccessStatusCode)
                    {
                        methodResult.AddError(studentSchoolResult.Error);
                        return methodResult;
                    }
                    var studentIds = studentSchoolResult.Content?.Result?.Select(x => x.Id).ToList() ?? new List<Guid>();
                    query = query.Where(x => studentIds.Contains(x.StudentId));
                }

                if (request.CourseType.HasValue)
                {
                    var courseLevels = request.CourseType.GetEnumCourseLevels();
                    query = query.Where(x => x.CourseLevel.HasValue && courseLevels.Contains(x.CourseLevel.Value));
                }
                if (request.Level.HasValue)
                {
                    query = query.Where(m => m.CourseLevel == request.Level);
                }

                totalItem = await query.CountAsync(cancellationToken);
                var lists = await query.ApplySortAndPaging(request)
                                 .AsNoTracking()
                                 .ToListAsync(cancellationToken: cancellationToken)
                                 .ConfigureAwait(false);
                var studentResults = await _userService.GetStudentsByStudentIdsAsync(lists.Select(x => x.StudentId).ToList());
                var students = studentResults.Content?.Result;

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
                        CourseType = courseResult.CourseLevel.GetEnumCourseType(),
                        CourseId = courseResult.CourseId,
                        CreatedDate = courseResult.CreatedDate ?? default,
                        UpdatedDate = courseResult.UpdatedDate ?? default,
                        DisplayOrderLesson = displayOrderLesson,
                        DisplayOrderUnit = displayOrderUnit,
                        ContentProgress = string.Format("{0} / {1}", currentProgress, progress),
                    };
                    studentProgress.Add(studentProgressModel);
                }
            }

            methodResult.Result = new PagingItemsModel<StudentProgressModel>(studentProgress, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
