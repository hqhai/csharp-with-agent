// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.CourseQuery
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchCourseTimeQuery : BaseQueryModel, IRequest<MethodResult<PagingItemsModel<CourseSearchModel>>>
    {
    }

    public class SearchCourseTimeQueryHandler : IRequestHandler<SearchCourseTimeQuery, MethodResult<PagingItemsModel<CourseSearchModel>>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly ISystemService _systemService;

        public SearchCourseTimeQueryHandler(ICourseRepository courseRepository, ISystemService systemService)
        {
            _courseRepository = courseRepository;
            _systemService = systemService;
        }

        public async Task<MethodResult<PagingItemsModel<CourseSearchModel>>> Handle(SearchCourseTimeQuery request, CancellationToken cancellationToken)
        {
            MethodResult<PagingItemsModel<CourseSearchModel>> methodResult = new MethodResult<PagingItemsModel<CourseSearchModel>>();
            ArgumentNullException.ThrowIfNull(request);
            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }

            var courseQuery = _courseRepository.Queryable
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
                              });
            int totalItem = await courseQuery.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await courseQuery
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            var courseTimeResult = await _systemService.GetCourseTimeConfigAsync(courseQuery.Select(x => x.Id).ToList());
            var courseTimes = courseTimeResult.Content?.Result;
            foreach (var item in lists)
            {
                item.DurationMonth = courseTimes!.Select(x => x.DurationMonth).FirstOrDefault();
                item.EnrollmentWeek = courseTimes!.Select(x => x.EnrollmentWeek).FirstOrDefault();
            }
            methodResult.Result = new PagingItemsModel<CourseSearchModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
