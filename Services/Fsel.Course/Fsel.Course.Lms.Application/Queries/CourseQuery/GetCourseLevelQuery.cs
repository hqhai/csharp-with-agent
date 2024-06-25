// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.CourseQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetCourseLevelQuery : IRequest<MethodResult<CourseModel>>
    {
        public Guid CourseId { get; set; }
    }

    public class GetCourseLevelQueryHandler : IRequestHandler<GetCourseLevelQuery, MethodResult<CourseModel>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IMapper _mapper;

        public GetCourseLevelQueryHandler(ICourseRepository courseRepository, IMapper mapper)
        {
            _courseRepository = courseRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<CourseModel>> Handle(GetCourseLevelQuery request, CancellationToken cancellationToken)
        {
            var methodResult = new MethodResult<CourseModel>();
            var course = await _courseRepository.Queryable.Where(x => x.Id == request.CourseId && x.Status != EnumCourseStatus.New).FirstOrDefaultAsync(cancellationToken);
            methodResult.Result = _mapper.Map<CourseModel>(course);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
