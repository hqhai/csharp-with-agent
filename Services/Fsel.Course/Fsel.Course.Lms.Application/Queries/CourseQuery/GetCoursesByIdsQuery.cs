// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.CourseQuery
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
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
        private readonly ICurriculumRepository _curriculumRepository;

        public GetCoursesByIdsQueryHandler(ICourseRepository courseRepository,
            ICurriculumRepository curriculumRepository)
        {
            _courseRepository = courseRepository;
            _curriculumRepository = curriculumRepository;
        }

        public async Task<MethodResult<IList<CourseModel>>> Handle(GetCoursesByIdsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<CourseModel>> methodResult = new MethodResult<IList<CourseModel>>();

            if (request.CourseIds == null || request.CourseIds.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.CourseIds));
                return methodResult;
            }

            var query = from c in _courseRepository.Queryable.WhereBulkContains(request.CourseIds, x => x.Id)
                        join cu in _curriculumRepository.Queryable on c.Id equals cu.CourseCloneId into g
                        from cu in g.DefaultIfEmpty()
                        select new CourseModel
                        {
                            Id = c.Id,
                            Code = c.Code,
                            Name = cu != null ? cu.CurriculumName : c.Name,
                            CourseLevel = c.CourseLevel,
                            CourseType = c.CourseType,
                            Status = c.Status,
                            CreatedDate = c.CreatedDate,
                            UpdatedDate = c.UpdatedDate,
                        };
            var courses = await query.OrderByDescending(x => x.UpdatedDate ?? x.CreatedDate)
                                     .ToListAsync(cancellationToken);
            methodResult.Result = courses;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
