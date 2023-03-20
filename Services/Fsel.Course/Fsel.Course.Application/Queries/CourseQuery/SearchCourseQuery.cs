// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Core.Base.BaseModels;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.EntityModels;
using Fsel.Course.Domain.Models.QueryModels.Courses;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Application.Queries.CourseQuery
{
    public class SearchCourseQuery : SearchCourseQueryModel, IRequest<MethodResult<PagingItemsModel<CourseModel>>>
    {
    }

    public class SearchCourseQueryHandler : IRequestHandler<SearchCourseQuery, MethodResult<PagingItemsModel<CourseModel>>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IMapper _mapper;

        public SearchCourseQueryHandler(IMapper mapper, ICourseRepository courseRepository)
        {
            _courseRepository = courseRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<PagingItemsModel<CourseModel>>> Handle(SearchCourseQuery request, CancellationToken cancellationToken)
        {
            MethodResult<PagingItemsModel<CourseModel>> methodResult = new MethodResult<PagingItemsModel<CourseModel>>();

            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }

            var courseQuery = _courseRepository.Queryable
                              .Include(course => course.CourseTeachers)
                              .Where(x => request.CourseLevel == null ? true : x.CourseLevel == request.CourseLevel)
                              .Where(x => request.TeacherId == null || x.CourseTeachers.Select(n => n.TeacherId).Contains(request.TeacherId.Value))
                              .Select(course => new CourseModel
                              {
                                  Id = course.Id,
                                  Name = course.Name,
                                  NumberOfLessons = course.NumberOfLessons,
                                  NumberOfUnits = course.NumberOfUnits,
                                  Status = course.Status,
                                  CourseLevel = course.CourseLevel,
                                  CreatedDate = course.CreatedDate,
                                  CreatedUserId = course.CreatedUserId,
                                  CreatedFullName = course.CreatedFullName,
                                  UpdatedDate = course.UpdatedDate,
                                  UpdatedUserId = course.UpdatedUserId,
                                  UpdatedFullName = course.UpdatedFullName,
                                  TeacherId = course.CourseTeachers.Select(x => x.TeacherId).FirstOrDefault(),
                              });

            /*  select new CourseModel
              {
                  Id = i.Id,
                  Name = i.Name,
                  NumberOfLessons = i.NumberOfLessons,
                  NumberOfUnits = i.NumberOfUnits,
                  Status = i.Status,
                  CourseLevel = i.CourseLevel,
                  CreatedDate = i.CreatedDate,
                  CreatedUserId = i.CreatedUserId,
                  CreatedFullName = i.CreatedFullName,
                  UpdatedDate = i.UpdatedDate,
                  UpdatedUserId = i.UpdatedUserId,
                  UpdatedFullName = i.UpdatedFullName,
                  TeacherId = i.CourseTeachers.Select(x => x.TeacherId).FirstOrDefault(),
              };*/
            //Keyword
            if (!string.IsNullOrEmpty(request.Keyword))
            {
                courseQuery = courseQuery.Where(m => m.Id.ToString() == request.Keyword || (m.Name ?? string.Empty).Contains(request.Keyword));
            }

            int totalItem = await courseQuery.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await courseQuery.OrderByDescending(x => x.Id)
                    .Skip((request.Page - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            methodResult.Result = new PagingItemsModel<CourseModel>
            {
                Items = _mapper.Map<IEnumerable<CourseModel>>(lists),
                PagingInfo = new PagingInfoModel { Page = request.Page, PageSize = request.PageSize, TotalItems = totalItem }
            };

            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
