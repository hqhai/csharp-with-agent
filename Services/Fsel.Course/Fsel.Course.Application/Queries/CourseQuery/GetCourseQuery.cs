// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Course.Domain.Enums.ErrorCodes;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.EntityModels;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Fsel.Course.Application.Queries.CourseQuery
{
    public class GetCourseQuery : IRequest<MethodResult<CourseModel>>
    {
        public Guid Id { get; set; }
    }

    public class GetCourseQueryHandler : IRequestHandler<GetCourseQuery, MethodResult<CourseModel>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IMapper _mapper;

        public GetCourseQueryHandler(IMapper mapper, ICourseRepository courseRepository)
        {
            _courseRepository = courseRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<CourseModel>> Handle(GetCourseQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<CourseModel> methodResult = new MethodResult<CourseModel>();

            var course = await _courseRepository.GetIncludeByIdAsync(request.Id);

            if (course == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumCourseErrorCode.CourseNotExist), nameof(request.Id), request.Id);
                return methodResult;
            }

            methodResult.Result = _mapper.Map<CourseModel>(course);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
