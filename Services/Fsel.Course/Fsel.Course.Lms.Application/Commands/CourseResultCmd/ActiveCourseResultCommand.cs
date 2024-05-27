// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.CourseResultCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class ActiveCourseResultCommand : IRequest<MethodResult<CourseResultModel>>
    {
        public Guid? CourseResultId { get; set; }
        public EnumCourseLevel? CourseLevel { get; set; }
    }

    public class ActiveCourseResultCommandHandler : IRequestHandler<ActiveCourseResultCommand, MethodResult<CourseResultModel>>
    {
        private readonly IMapper _mapper;
        private readonly ICourseResultRepository _courseResultRepository;

        public ActiveCourseResultCommandHandler(IMapper mapper
            , ICourseResultRepository courseResultRepository)
        {
            _mapper = mapper;
            _courseResultRepository = courseResultRepository;
        }

        public async Task<MethodResult<CourseResultModel>> Handle(ActiveCourseResultCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<CourseResultModel> methodResult = new MethodResult<CourseResultModel>();
            if (request.CourseResultId.HasValue)
            {
                var courseResult = await _courseResultRepository.GetByIdAsync(request.CourseResultId.Value);
                if (courseResult == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(courseResult));
                    return methodResult;
                }
            }

            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
