// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.CourseQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Shared.Enums;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetCoursesByLevelQuery : IRequest<MethodResult<IList<CourseModel>>>
    {
        public EnumCourseLevel? CourseLevel { get; set; }
    }

    public class GetCoursesByLevelQueryHandler : IRequestHandler<GetCoursesByLevelQuery, MethodResult<IList<CourseModel>>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IMapper _mapper;

        public GetCoursesByLevelQueryHandler(IMapper mapper
            , ICourseRepository courseRepository)
        {
            _courseRepository = courseRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<CourseModel>>> Handle(GetCoursesByLevelQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<CourseModel>> methodResult = new MethodResult<IList<CourseModel>>();
            var courseQuery = await _courseRepository.Queryable
                              .Include(course => course.CourseTeachers.Where(y => !y.IsDeleted))
                              .Where(x => request.CourseLevel == null || x.CourseLevel == request.CourseLevel)
                              .Where(x => x.Status == EnumCourseStatus.Active)
                              .AsNoTracking()
                              .Select(course => new CourseModel
                              {
                                  Id = course.Id,
                                  Name = course.Name,
                                  Code = course.Code,
                                  Status = course.Status,
                                  CourseLevel = course.CourseLevel,
                                  CreatedDate = course.CreatedDate,
                                  CreatedUserId = course.CreatedUserId,
                                  CreatedFullName = course.CreatedFullName,
                                  UpdatedDate = course.UpdatedDate,
                                  UpdatedUserId = course.UpdatedUserId,
                                  UpdatedFullName = course.UpdatedFullName,
                                  CourseTeachers = _mapper.Map<IList<CourseTeacherModel>>(course.CourseTeachers)
                              }).ToListAsync(cancellationToken: cancellationToken);
            if (courseQuery.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumCourseErrorCode.CoursesNotExist));
                return methodResult;
            }
            methodResult.Result = courseQuery;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
