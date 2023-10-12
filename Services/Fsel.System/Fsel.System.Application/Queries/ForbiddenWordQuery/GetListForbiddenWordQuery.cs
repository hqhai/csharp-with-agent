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

    public class CheckContainForbiddenWordQuery : IRequest<MethodResult<IList<String>>>
    {
        public string? Word { get; set; }
    }
    public class GetListForbiddenWordQueryHandler : IRequestHandler<CheckContainForbiddenWordQuery, MethodResult<IList<String>>>
    {
        private readonly IForbiddenWordRepository _forbiddenWordRepository;

        public GetListForbiddenWordQueryHandler(IForbiddenWordRepository forbiddenWordRepository)
        {
            _forbiddenWordRepository = forbiddenWordRepository;
        }

        public async Task<MethodResult<IList<String>>> Handle(CheckContainForbiddenWordQuery request, CancellationToken cancellationToken)
        {
            MethodResult<IList<String>> methodResult = new MethodResult<IList<String>>();
            ArgumentNullException.ThrowIfNull(request);
            var forbiddenWordQuery = await _forbiddenWordRepository.Queryable
             .Where(x => request.Word.Contains(x.Word))
             .Select(x => x.Word.ToLower()).Distinct().ToListAsync(cancellationToken: cancellationToken);
            methodResult.Result = forbiddenWordQuery;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }


}
