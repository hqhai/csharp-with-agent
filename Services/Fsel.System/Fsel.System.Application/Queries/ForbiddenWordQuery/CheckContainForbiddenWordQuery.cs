// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.ForbiddenWordQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Shared.Helpers;
    using Fsel.System.Domain.IRepositories;
    using global::System;
    using global::System.Collections.Generic;
    using global::System.Linq;
    using global::System.Threading.Tasks;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    public class CheckContainForbiddenWordQuery : IRequest<MethodResult<IList<string>>>
    {
        public string? Word { get; set; }
    }
    public class CheckContainForbiddenWordQueryHandler : IRequestHandler<CheckContainForbiddenWordQuery, MethodResult<IList<string>>>
    {
        private readonly IForbiddenWordRepository _forbiddenWordRepository;

        public CheckContainForbiddenWordQueryHandler(IForbiddenWordRepository forbiddenWordRepository)
        {
            _forbiddenWordRepository = forbiddenWordRepository;
        }

        public async Task<MethodResult<IList<string>>> Handle(CheckContainForbiddenWordQuery request, CancellationToken cancellationToken)
        {
            var methodResult = new MethodResult<IList<string>>();
            ArgumentNullException.ThrowIfNull(request);

            if (string.IsNullOrEmpty(request.Word))
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }

            var forbiddenWordQuery = await _forbiddenWordRepository.Queryable
            .Where(x => (" " + StringHelper.RemoveHTMLTags(request.Word) + " ").ToLower().Contains(" " + x.Word.ToLower() + " "))
            .Select(x => x.Word).Distinct().ToListAsync(cancellationToken: cancellationToken);
            methodResult.Result = forbiddenWordQuery;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }


}
