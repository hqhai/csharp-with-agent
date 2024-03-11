// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.FocusTimeConfigQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using global::System;
    using global::System.Threading.Tasks;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetListFocusTimeConfigQuery : IRequest<MethodResult<IList<FocusTimeConfigModel>>>
    {
    }

    public class GetListFocusTimeConfigQueryHandler : IRequestHandler<GetListFocusTimeConfigQuery, MethodResult<IList<FocusTimeConfigModel>>>
    {
        private readonly IFocusTimeConfigRepository _focusTimeConfigRepository;
        private readonly IMapper _mapper;

        public GetListFocusTimeConfigQueryHandler(IFocusTimeConfigRepository focusTimeConfigRepository, IMapper mapper)
        {
            _focusTimeConfigRepository = focusTimeConfigRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<FocusTimeConfigModel>>> Handle(GetListFocusTimeConfigQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<FocusTimeConfigModel>> methodResult = new MethodResult<IList<FocusTimeConfigModel>>();
            var listFocusTime = await _focusTimeConfigRepository.Queryable.ToListAsync(cancellationToken);
            methodResult.Result = _mapper.Map<IList<FocusTimeConfigModel>>(listFocusTime);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
