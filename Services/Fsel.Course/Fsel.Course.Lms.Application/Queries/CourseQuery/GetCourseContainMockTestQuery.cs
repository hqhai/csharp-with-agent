// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.CourseQuery
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetCourseContainMockTestQuery : BaseQueryModel, IRequest<MethodResult<IList<CourseModel>>>
    {
    }
    public class GetCourseContainMockTestQueryHandler : IRequestHandler<GetCourseContainMockTestQuery, MethodResult<IList<CourseModel>>>
    {
        private readonly ICourseRepository _courseRepository;

        public GetCourseContainMockTestQueryHandler(ICourseRepository courseRepository)
        {
            _courseRepository = courseRepository;
        }
        public async Task<MethodResult<IList<CourseModel>>> Handle(GetCourseContainMockTestQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<CourseModel>>();

            var course = await _courseRepository.Queryable
                                .Include(x => x.MockTestResults)
                                .Where(x => x.MockTestResults.FirstOrDefault() != null)
                                .Select(x => new CourseModel
                                {
                                    Id = x.Id,
                                    Name = x.Name,
                                    CreatedDate = x.CreatedDate,
                                }).ApplySort(request).ToListAsync(cancellationToken);

            methodResult.Result = course;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
