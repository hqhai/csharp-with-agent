// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.CourseQuery.V1i1
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.QueryModels.Courses;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchCourseQuery : SearchCourseQueryModel, IRequest<MethodResult<PagingItemsModel<CourseSearchModel>>>
    {
    }

    public class SearchCourseQueryHandler : IRequestHandler<SearchCourseQuery, MethodResult<PagingItemsModel<CourseSearchModel>>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILevelRepository _levelRepository;

        public SearchCourseQueryHandler(ICourseRepository courseRepository, ICategoryRepository categoryRepository, ILevelRepository levelRepository)
        {
            _courseRepository = courseRepository;
            _categoryRepository = categoryRepository;
            _levelRepository = levelRepository;
        }

        public async Task<MethodResult<PagingItemsModel<CourseSearchModel>>> Handle(SearchCourseQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<CourseSearchModel>>();

            var courseQuery = from course in _courseRepository.Queryable
                              join p in _categoryRepository.Queryable.Include(p => p.CategoryParent) on course.ProgramId equals p.Id
                              join l in _levelRepository.Queryable on course.LevelId equals l.Id
                              where !course.IsArchive && course.Status == EnumCourseStatus.Active && course.VersionStatus == EnumVersionStatus.LastVersion
                              select new CourseSearchModel
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
                                  CourseType = course.CourseType,
                                  Program = p.Name,
                                  ProgramId = p.Id,
                                  LevelName = l.Name,
                                  LevelId = l.Id,
                                  Subject = p.CategoryParent != null ? p.CategoryParent.Name : null,
                                  SubjectId = p.CategoryParent != null ? p.CategoryParent.Id : null,
                              };

            request.Keyword = request.Keyword?.Trim().ToLower(System.Globalization.CultureInfo.CurrentCulture);
            if (!string.IsNullOrEmpty(request.Keyword))
            {
                courseQuery = courseQuery.Where(m => !string.IsNullOrEmpty(m.Code) && m.Code.Contains(request.Keyword) || !string.IsNullOrEmpty(m.Name) && m.Name.Contains(request.Keyword));
            }

            if (request.ProgramIds != null && request.ProgramIds.Count > 0)
            {
                courseQuery = courseQuery.Where(m => m.ProgramId.HasValue && request.ProgramIds.Contains(m.ProgramId.Value));
            }

            if (request.LevelIds != null && request.LevelIds.Count > 0)
            {
                courseQuery = courseQuery.Where(m => m.LevelId.HasValue && request.LevelIds.Contains(m.LevelId.Value));
            }

            int totalItem = await courseQuery.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await courseQuery
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            methodResult.Result = new PagingItemsModel<CourseSearchModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
