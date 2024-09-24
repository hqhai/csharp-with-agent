// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.CourseTargetConfigQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using Fsel.System.Domain.IRepositories;
    using global::System.Collections.Generic;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetCourseTargetConfigQuery : IRequest<MethodResult<IList<CourseTargetConfigModel>>>
    {
        public EnumCourseType CourseType { get; set; }
    }

    public class GetCourseTargetConfigQueryHandler : IRequestHandler<GetCourseTargetConfigQuery, MethodResult<IList<CourseTargetConfigModel>>>
    {
        private readonly ICourseTargetConfigRepository _courseTargetConfigRepository;
        private readonly IMapper _mapper;

        public GetCourseTargetConfigQueryHandler(ICourseTargetConfigRepository courseTargetConfigRepository,
                                                 IMapper mapper)
        {
            _courseTargetConfigRepository = courseTargetConfigRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<CourseTargetConfigModel>>> Handle(GetCourseTargetConfigQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<CourseTargetConfigModel>> methodResult = new MethodResult<IList<CourseTargetConfigModel>>();

            var courseTargetConfigs = await _courseTargetConfigRepository.Queryable
                                                                         .Where(x => x.CourseType == request.CourseType)
                                                                         .ToListAsync(cancellationToken);

            if (courseTargetConfigs == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.CourseType), nameof(request.CourseType));
                return methodResult;
            }

            methodResult.Result = _mapper.Map<IList<CourseTargetConfigModel>>(courseTargetConfigs);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
