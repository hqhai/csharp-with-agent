// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.ActionResults;
using Fsel.Core.Base.BaseModels;
using Fsel.Core.Extensions;
using Fsel.Course.Application.Services.UserServices;
using Fsel.Course.Application.Services.UserServices.Models;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.EntityModels;
using Fsel.Course.Domain.Models.QueryModels.Courses;
using Fsel.Shared.Helpers;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Application.Queries.CourseQuery
{
    public class SearchCourseQuery : SearchCourseQueryModel, IRequest<MethodResult<PagingItemsModel<CourseSearchModel>>>
    {
    }

    public class SearchCourseQueryHandler : IRequestHandler<SearchCourseQuery, MethodResult<PagingItemsModel<CourseSearchModel>>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IUserService _userService;

        public SearchCourseQueryHandler(ICourseRepository courseRepository, IUserService userService)
        {
            _courseRepository = courseRepository;
            _userService = userService;
        }

        public async Task<MethodResult<PagingItemsModel<CourseSearchModel>>> Handle(SearchCourseQuery request, CancellationToken cancellationToken)
        {
            MethodResult<PagingItemsModel<CourseSearchModel>> methodResult = new MethodResult<PagingItemsModel<CourseSearchModel>>();
            ArgumentNullException.ThrowIfNull(request);
            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }

            var courseQuery = _courseRepository.Queryable.Where(p => !p.IsArchive)
                              .Include(course => course.CourseTeachers.Where(n => !n.IsDeleted))
                              .Where(x => !x.ParentCourseId.HasValue)
                              .Select(course => new CourseSearchModel
                              {
                                  Id = course.Id,
                                  Name = course.Name,
                                  Code = course.Code,
                                  InstructionContent = course.InstructionContent,
                                  Status = course.Status,
                                  CourseLevel = course.CourseLevel,
                                  CreatedDate = course.CreatedDate,
                                  CreatedUserId = course.CreatedUserId,
                                  CreatedFullName = course.CreatedFullName,
                                  UpdatedDate = course.UpdatedDate,
                                  UpdatedUserId = course.UpdatedUserId,
                                  UpdatedFullName = course.UpdatedFullName,
                                  TeacherIds = course.CourseTeachers.Where(n => !n.IsDeleted).Select(x => x.TeacherId).Distinct().ToList(),
                              });

            request.Keyword = request.Keyword?.Trim().ToLower(System.Globalization.CultureInfo.CurrentCulture);
            if (!string.IsNullOrEmpty(request.Keyword))
            {
                if (Guid.TryParse(request.Keyword, out var guid))
                {
                    courseQuery = courseQuery.Where(m => m.Id == guid);
                }
                else
                {
                    courseQuery = courseQuery.Where(m => m.Code != null && m.Code.Contains(request.Keyword));
                }
            }

            if (request.CourseLevel != null)
            {
                courseQuery = courseQuery.Where(m => m.CourseLevel == request.CourseLevel);
            }

            if (request.TeacherId != null)
            {
                courseQuery = courseQuery.Where(m => m.TeacherIds!.Any(x => x == request.TeacherId));
            }

            int totalItem = await courseQuery.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await courseQuery
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            var teacherResults = await _userService.GetTeacherByIdsAsync(new GetTeacherByIdsQueryModel { Ids = courseQuery.SelectMany(x => x.TeacherIds!).Distinct().ToList() });
            if (teacherResults.IsSuccessStatusCode)
            {
                var teachers = teacherResults.Content?.Result;
                foreach (var item in lists)
                {
                    item.TeacherNames = teachers?.Where(x => item.TeacherIds!.Contains(x.Id)).Select(x => x.User?.FullName ?? string.Empty).ToList();
                }
            }

            methodResult.Result = new PagingItemsModel<CourseSearchModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
