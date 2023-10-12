// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.FocusTimeConfigQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using global::System;
    using global::System.Linq;
    using global::System.Threading.Tasks;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetListFocusTimeConfigQuery : IRequest<MethodResult<IList<FocusTimeConfig>>>
    {
    }

    public class GetListFocusTimeConfigQueryHandler : IRequestHandler<GetListFocusTimeConfigQuery, MethodResult<IList<FocusTimeConfig>>>
    {
        private readonly IFocusTimeConfigRepository _focusTimeConfigRepository;

        public GetListFocusTimeConfigQueryHandler(IFocusTimeConfigRepository focusTimeConfigRepository)
        {
            _focusTimeConfigRepository = focusTimeConfigRepository;
        }

        public async Task<MethodResult<IList<FocusTimeConfig>>> Handle(GetListFocusTimeConfigQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<FocusTimeConfig>> methodResult = new MethodResult<IList<FocusTimeConfig>>();

            var listFocusTime = await _focusTimeConfigRepository.Queryable.ToListAsync(cancellationToken);

            methodResult.Result = listFocusTime;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
