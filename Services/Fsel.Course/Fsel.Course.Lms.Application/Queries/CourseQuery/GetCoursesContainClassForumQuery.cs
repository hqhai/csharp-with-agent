// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.CourseQuery
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetCoursesContainClassForumQuery : BaseQueryModel, IRequest<MethodResult<IList<CourseModel>>>
    {
    }

    public class GetCoursesContainClassForumQueryHandler : IRequestHandler<GetCoursesContainClassForumQuery, MethodResult<IList<CourseModel>>>
    {
        private readonly ICourseRepository _courseRepository;

        public GetCoursesContainClassForumQueryHandler(ICourseRepository courseRepository)
        {
            _courseRepository = courseRepository;
        }

        public async Task<MethodResult<IList<CourseModel>>> Handle(GetCoursesContainClassForumQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<CourseModel>>();

            var course = await _courseRepository.Queryable
                .Include(x => x.LessonResults)
                .Where(x => x.LessonResults.Select(x => x.ClassForumResults).FirstOrDefault() != null)
                .Select(x => new CourseModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    CreatedDate = x.CreatedDate,
                    CourseType = x.CourseType,
                    LevelId = x.LevelId,
                }).ApplySort(request).ToListAsync(cancellationToken);

            methodResult.Result = course.Where(x => x.CourseType == EnumCourseType.Academic).ToList();
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
