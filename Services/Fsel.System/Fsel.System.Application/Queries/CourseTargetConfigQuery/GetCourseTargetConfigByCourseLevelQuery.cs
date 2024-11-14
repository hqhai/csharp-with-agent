// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.CourseTargetConfigQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.ShareModels;
    using Fsel.System.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetCourseTargetConfigByCourseLevelQuery : IRequest<MethodResult<IList<CourseTargetConfigModel>>>
    {
        public EnumCourseLevel CourseLevel { get; set; }

        public int TotalLesson { get; set; }
    }

    public class GetCourseTargetConfigByCourseLevelQueryHandler : IRequestHandler<GetCourseTargetConfigByCourseLevelQuery, MethodResult<IList<CourseTargetConfigModel>>>
    {
        private readonly ICourseTargetConfigRepository _courseTargetConfigRepository;
        private readonly IMapper _mapper;

        public GetCourseTargetConfigByCourseLevelQueryHandler(ICourseTargetConfigRepository courseTargetConfigRepository,
                                                              IMapper mapper)
        {
            _courseTargetConfigRepository = courseTargetConfigRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<CourseTargetConfigModel>>> Handle(GetCourseTargetConfigByCourseLevelQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<CourseTargetConfigModel>> methodResult = new MethodResult<IList<CourseTargetConfigModel>>();

            var courseTargetConfigs = await _courseTargetConfigRepository.Queryable
                                                                         .Where(x => x.CourseLevel == request.CourseLevel)
                                                                         .ToListAsync(cancellationToken);

            if (courseTargetConfigs == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.CourseLevel), nameof(request.CourseLevel));
                return methodResult;
            }

            methodResult.Result = _mapper.Map<IList<CourseTargetConfigModel>>(courseTargetConfigs);

            methodResult.Result.CourseTargetConfig(request.TotalLesson);

            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
