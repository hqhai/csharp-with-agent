// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.CourseQuery
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetCourseByCodeQuery : IRequest<MethodResult<CourseModel>>
    {
        public string? CourseCode { get; set; }
    }

    public class GetCourseByCodeQueryHandler : IRequestHandler<GetCourseByCodeQuery, MethodResult<CourseModel>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IMapper _mapper;

        public GetCourseByCodeQueryHandler(ICourseRepository courseRepository, IMapper mapper)
        {
            _courseRepository = courseRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<CourseModel>> Handle(GetCourseByCodeQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<CourseModel>();

            var course = await _courseRepository.Queryable.FirstOrDefaultAsync(p => p.Code == request.CourseCode, cancellationToken);

            if (course == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(course));
                return methodResult;
            }

            methodResult.Result = _mapper.Map<CourseModel>(course);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
