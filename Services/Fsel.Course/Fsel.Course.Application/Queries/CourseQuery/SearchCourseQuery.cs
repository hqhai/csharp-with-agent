// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.ActionResults;
using Fsel.Common.Enums;
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
        private readonly ILevelRepository _levelRepository;

        public SearchCourseQueryHandler(ICourseRepository courseRepository,
                                        IUserService userService,
                                        ILevelRepository levelRepository)
        {
            _courseRepository = courseRepository;
            _userService = userService;
            _levelRepository = levelRepository;
        }

        public async Task<MethodResult<PagingItemsModel<CourseSearchModel>>> Handle(SearchCourseQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<PagingItemsModel<CourseSearchModel>> methodResult = new MethodResult<PagingItemsModel<CourseSearchModel>>();

            var courseQuery = _courseRepository.Queryable
                                               .Where(p => !p.IsArchive && p.VersionStatus == EnumVersionStatus.LastVersion)
                                               .Include(course => course.CourseTeachers)
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
                                                   LevelId = course.LevelId,
                                                   OriginalId = course.OriginalId,
                                                   ProgramId = course.ProgramId
                                               });

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                courseQuery = courseQuery.Where(m => m.Code != null && m.Code.Contains(request.Keyword));
            }

            if (request.LevelId.HasValue)
            {
                courseQuery = courseQuery.Where(m => m.LevelId == request.LevelId);
            }

            if (request.TeacherId.HasValue)
            {
                courseQuery = courseQuery.Where(m => m.TeacherIds!.Any(x => x == request.TeacherId));
            }

            if (request.ProgramId.HasValue)
            {
                courseQuery = courseQuery.Where(m => m.ProgramId == request.ProgramId);
            }

            int totalItem = await courseQuery.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

            var lists = await courseQuery.ApplySortAndPaging(request)
                                         .AsNoTracking()
                                         .ToListAsync(cancellationToken: cancellationToken)
                                         .ConfigureAwait(false);

            await SetFieldAsync(lists, cancellationToken);

            methodResult.Result = new PagingItemsModel<CourseSearchModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task SetFieldAsync(IList<CourseSearchModel> courses, CancellationToken cancellationToken)
        {
            var teacherResults = await _userService.GetTeacherByIdsAsync(new GetTeacherByIdsQueryModel { Ids = courses.SelectMany(x => x.TeacherIds!).Distinct().ToList() });
            if (teacherResults.IsSuccessStatusCode)
            {
                var teachers = teacherResults.Content?.Result;
                foreach (var item in courses)
                {
                    item.TeacherNames = teachers?.Where(x => item.TeacherIds!.Contains(x.Id)).Select(x => x.Human?.FullName ?? string.Empty).ToList();
                }
            }

            var levelIds = courses.Where(x => x.LevelId.HasValue).Select(x => x.LevelId!).Distinct().ToList() ?? new List<Guid?>();
            var levels = await _levelRepository.Queryable.WhereBulkContains(levelIds, x => x.Id).AsNoTracking().ToListAsync(cancellationToken).ConfigureAwait(false);

            courses.ForEach(x =>
            {
                var level = levels.FirstOrDefault(c => c.Id == x.LevelId);
                x.LevelName = level?.Name;
            });
        }
    }
}
