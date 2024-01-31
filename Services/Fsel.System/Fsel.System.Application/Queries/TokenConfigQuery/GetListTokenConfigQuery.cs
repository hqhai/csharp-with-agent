// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.TokenConfigQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.IRepositories;
    using global::System;
    using global::System.Collections.Generic;
    using global::System.IO;
    using global::System.Threading.Tasks;
    using MediatR;

    public class GetListTokenConfigQuery : IRequest<MethodResult<IList<TokenConfigModel>>>
    {
    }

    public class GetListTokenConfigQueryHandler : IRequestHandler<GetListTokenConfigQuery, MethodResult<IList<TokenConfigModel>>>
    {
        private readonly ITokenConfigRepository _tokenConfigRepository;
        private readonly IMapper _mapper;

        public GetListTokenConfigQueryHandler(ITokenConfigRepository tokenConfigRepository, IMapper mapper)
        {
            _tokenConfigRepository = tokenConfigRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<TokenConfigModel>>> Handle(GetListTokenConfigQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<TokenConfigModel>>();

            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ResourceSettings.TokenConfig);
            var tokenConfigs = ConvertHelper.DeserializeFromFilePath<IList<TokenConfig>>(path);

            methodResult.Result = _mapper.Map<IList<TokenConfigModel>>(tokenConfigs);
            return methodResult;
        }
    }
}
