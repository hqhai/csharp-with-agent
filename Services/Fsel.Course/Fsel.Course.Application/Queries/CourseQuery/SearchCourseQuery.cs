// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Core.Base.BaseModels;
using Fsel.Course.Application.Services.UserServices;
using Fsel.Course.Application.Services.UserServices.Models;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.EntityModels;
using Fsel.Course.Domain.Models.QueryModels.Courses;
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
        private readonly IMapper _mapper;
        private readonly IUserService _userService;

        public SearchCourseQueryHandler(IMapper mapper, ICourseRepository courseRepository, IUserService userService)
        {
            _courseRepository = courseRepository;
            _userService = userService;
            _mapper = mapper;
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

            var courseQuery = _courseRepository.Queryable
                              .Include(course => course.CourseTeachers)
                              .Where(x => request.CourseLevel == null || x.CourseLevel == request.CourseLevel)
                              .Where(x => request.TeacherId == null || x.CourseTeachers.Select(n => n.TeacherId).Contains(request.TeacherId.Value))
                              .Select(course => new CourseSearchModel
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

            //Keyword
            if (!string.IsNullOrEmpty(request.Keyword))
            {
                courseQuery = courseQuery.Where(m => m.Id.ToString() == request.Keyword || (m.Name ?? string.Empty).Contains(request.Keyword));
            }

            int totalItem = await courseQuery.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await courseQuery.OrderByDescending(x => x.CreatedDate)
                    .Skip((request.Page - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            var teachers = await _userService.GetTeacherByIdsAsync(new GetTeacherByIdsQueryModel { Ids = courseQuery.Select(x => x.TeacherId ?? Guid.Empty).ToList() });
            if (teachers.IsSuccessStatusCode)
            {
                foreach (var item in lists)
                {
                    item.TeacherName = teachers.Content?.Result?.FirstOrDefault(x => x.Id == item.TeacherId)?.Human?.FullName;
                }
            }

            methodResult.Result = new PagingItemsModel<CourseSearchModel>
            {
                Items = _mapper.Map<IEnumerable<CourseSearchModel>>(lists),
                PagingInfo = new PagingInfoModel { Page = request.Page, PageSize = request.PageSize, TotalItems = totalItem }
            };

            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        } 
    }
}
