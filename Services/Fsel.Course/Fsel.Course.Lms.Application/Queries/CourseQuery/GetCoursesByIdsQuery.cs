// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.CourseQuery
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetCoursesByIdsQuery : IRequest<MethodResult<IList<CourseModel>>>
    {
        public IList<Guid>? CourseIds { get; set; }
    }

    public class GetCoursesByIdsQueryHandler : IRequestHandler<GetCoursesByIdsQuery, MethodResult<IList<CourseModel>>>
    {
        private readonly ICourseRepository _courseRepository;

        public GetCoursesByIdsQueryHandler(ICourseRepository courseRepository)
        {
            _courseRepository = courseRepository;
        }

        public async Task<MethodResult<IList<CourseModel>>> Handle(GetCoursesByIdsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<CourseModel>> methodResult = new MethodResult<IList<CourseModel>>();

            if (request.CourseIds == null || request.CourseIds.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumCourseErrorCode.CourseIdsNull));
                return methodResult;
            }

            var courses = await _courseRepository.Queryable.Where(p => request.CourseIds.Contains(p.Id)).Select(x => new CourseModel
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                CourseLevel = x.CourseLevel,
                CourseType = x.CourseType,
                InstructionContent = x.InstructionContent,
                Status = x.Status,
            }).ToListAsync(cancellationToken);

            methodResult.Result = courses;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
