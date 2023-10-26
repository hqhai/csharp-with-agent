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

            var forbiddenWords = await _forbiddenWordRepository.Queryable
             .Where(x => !string.IsNullOrEmpty(x.Word) && StringHelper.RemoveHTMLTags(request.Word).Trim().ToLower().Contains(x.Word.Trim().ToLower()))
             .Select(x => x.Word ?? string.Empty).Distinct().ToListAsync(cancellationToken: cancellationToken);
            methodResult.Result = forbiddenWords;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }


}
