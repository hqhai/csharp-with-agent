// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.TokenConfigQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using Fsel.System.Domain.Entities;
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
        private readonly IFocusTimeConfigRepository _focusTimeConfigRepository;

        public GetTokenConfigsByAdminQueryHandler(ITokenConfigRepository tokenConfigRepository, IMapper mapper, IFocusTimeConfigRepository focusTimeConfigRepository)
        {
            _tokenConfigRepository = tokenConfigRepository;
            _mapper = mapper;
            _focusTimeConfigRepository = focusTimeConfigRepository;
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
            var tokenConfigModels = new List<TokenConfigModel>();
            if (request.Feature == EnumTokenFeature.FocusMode)
            {
                var focusTimeConfigs = await _focusTimeConfigRepository.Queryable.ToListAsync(cancellationToken);
                foreach (var tokenConfig in tokenConfigs)
                {
                    tokenConfigModels.Add(GetTokenConfigToFocusMode(tokenConfig, focusTimeConfigs));
                }
            }
            else
            {
                _mapper.Map(tokenConfigs, tokenConfigModels);
            }

            methodResult.Result = tokenConfigModels;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private TokenConfigModel GetTokenConfigToFocusMode(TokenConfig tokenConfig, IList<FocusTimeConfig>? focusTimeConfigs)
        {
            if (tokenConfig.Mission == EnumTokenMission.FocusMode)
            {
                var tokenConfigFocusModes = tokenConfig.Config.Deserialize<IList<TokenConfigFocusModes>>();
                if (tokenConfigFocusModes != null)
                {
                    foreach (var item in tokenConfigFocusModes)
                    {
                        var focusTimeConfig = focusTimeConfigs?.FirstOrDefault(x => x.Id == item.FocusTimeId);
                        if (focusTimeConfig != null)
                        {
                            item.TargetTime = focusTimeConfig.TargetTime;
                        }
                    }
                    tokenConfig.Config = tokenConfigFocusModes;
                }
            }
            return _mapper.Map<TokenConfigModel>(tokenConfig);
        }
    }
}
