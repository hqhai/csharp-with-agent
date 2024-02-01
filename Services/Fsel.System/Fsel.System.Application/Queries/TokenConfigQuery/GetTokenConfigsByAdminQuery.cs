// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.TokenConfigQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using Fsel.System.Domain.IRepositories;
    using global::System;
    using global::System.Collections.Generic;
    using global::System.Linq;
    using global::System.Threading.Tasks;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetTokenConfigsByAdminQuery : IRequest<MethodResult<IList<TokenConfigModel>>>
    {
        public EnumTokenFeature Feature { get; set; }
        public EnumCourseType? CourseType { get; set; }
    }

    public class GetTokenConfigsByAdminQueryHandler : IRequestHandler<GetTokenConfigsByAdminQuery, MethodResult<IList<TokenConfigModel>>>
    {
        private readonly ITokenConfigRepository _tokenConfigRepository;
        private readonly IMapper _mapper;

        public GetTokenConfigsByAdminQueryHandler(ITokenConfigRepository tokenConfigRepository, IMapper mapper)
        {
            _tokenConfigRepository = tokenConfigRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<TokenConfigModel>>> Handle(GetTokenConfigsByAdminQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<TokenConfigModel>>();
            var tokenConfigs = await _tokenConfigRepository.Queryable.Where(x => x.Feature == request.Feature && (!request.CourseType.HasValue || x.CourseType == request.CourseType)).OrderBy(x => x.DisplayOrder).ToListAsync(cancellationToken);
            if (tokenConfigs == null || !tokenConfigs.Any())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(tokenConfigs));
                return methodResult;
            }
            methodResult.Result = _mapper.Map<IList<TokenConfigModel>>(tokenConfigs);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
