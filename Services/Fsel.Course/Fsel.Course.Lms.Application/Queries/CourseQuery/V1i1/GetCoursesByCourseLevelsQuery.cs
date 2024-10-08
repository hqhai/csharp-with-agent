// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.CourseQuery.V1i1
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json.Serialization;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetCoursesByCourseLevelsQuery : IRequest<MethodResult<IList<CourseModel>>>
    {
        public string? CourseLevels { get; set; }

        [JsonIgnore]
        public IList<EnumCourseLevel>? ListCourseLevel
        {
            get
            {
                return CourseLevels.ToList<EnumCourseLevel>();
            }
        }
    }

    public class GetCoursesByCourseLevelsQueryHandler : IRequestHandler<GetCoursesByCourseLevelsQuery, MethodResult<IList<CourseModel>>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IMapper _mapper;

        public GetCoursesByCourseLevelsQueryHandler(ICourseRepository courseRepository, IMapper mapper)
        {
            _courseRepository = courseRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<CourseModel>>> Handle(GetCoursesByCourseLevelsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<CourseModel>>();
            if (request.ListCourseLevel == null || !request.ListCourseLevel.Any())
            {
                return methodResult;
            }
            var courses = await _courseRepository.Queryable.Where(x => request.ListCourseLevel.Contains(x.CourseLevel))
                                                           .Where(x => x.Status != EnumCourseStatus.New && x.Status != EnumCourseStatus.Clone)
                                                           .ToListAsync(cancellationToken);
            courses = courses.GroupBy(x => x.CourseLevel).Select(x => x.OrderBy(x => x.Status).ThenByDescending(x => x.UpdatedDate).ThenByDescending(x => x.CreatedDate).FirstOrDefault() ?? new Domain.Entities.Course()).ToList();
            methodResult.Result = _mapper.Map<IList<CourseModel>>(courses);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
