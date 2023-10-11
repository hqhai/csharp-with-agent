// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.ForbiddenWordQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.System.Application.Querys.ForbiddenWordQuery;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using global::System;
    using global::System.Collections.Generic;
    using global::System.Linq;
    using global::System.Text;
    using global::System.Threading.Tasks;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetListForbiddenWordQuery: IRequest<MethodResult<IList<ForbiddenWordModel>>>
    {

    }
    public class GetListForbiddenWordQueryHandler : IRequestHandler<GetListForbiddenWordQuery, MethodResult<IList<ForbiddenWordModel>>>
    {
        private readonly IForbiddenWordRepository _forbiddenWordRepository;

        public GetListForbiddenWordQueryHandler(IForbiddenWordRepository forbiddenWordRepository)
        {
            _forbiddenWordRepository = forbiddenWordRepository;
        }

        public async Task<MethodResult<IList<ForbiddenWordModel>>> Handle(GetListForbiddenWordQuery request, CancellationToken cancellationToken)
        {
            MethodResult<IList<ForbiddenWordModel>> methodResult = new MethodResult<IList<ForbiddenWordModel>>();
            ArgumentNullException.ThrowIfNull(request);
            var forbiddenWordQuery = await _forbiddenWordRepository.Queryable
                               .Select(x => new ForbiddenWordModel
                               {
                                   Word = x.Word,
                               }).ToListAsync(cancellationToken: cancellationToken);
            methodResult.Result = forbiddenWordQuery;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }


}
