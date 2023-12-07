// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.TokenConfigQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using global::System;
    using global::System.Collections.Generic;
    using global::System.Linq;
    using global::System.Text;
    using global::System.Threading.Tasks;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

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

            var token = await _tokenConfigRepository.Queryable.ToListAsync(cancellationToken);

            methodResult.Result = _mapper.Map<IList<TokenConfigModel>>(token);
            return methodResult;
        }
    }
}
