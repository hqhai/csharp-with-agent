// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.FinalTestQuery
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.QueryModels.FinalTests;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchFinalTestQuery : SearchFinalTestQueryModel, IRequest<MethodResult<PagingItemsModel<FinalTestSearchModel>>>
    {
    }

    public class SearchFinalTestQueryHandler : IRequestHandler<SearchFinalTestQuery, MethodResult<PagingItemsModel<FinalTestSearchModel>>>
    {
        private readonly IFinalTestRepository _finalTestRepository;

        public SearchFinalTestQueryHandler(IFinalTestRepository finalTestRepository)
        {
            _finalTestRepository = finalTestRepository;
        }

        public async Task<MethodResult<PagingItemsModel<FinalTestSearchModel>>> Handle(SearchFinalTestQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<PagingItemsModel<FinalTestSearchModel>> methodResult = new MethodResult<PagingItemsModel<FinalTestSearchModel>>();
            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }
            var finalTestQuery = _finalTestRepository.Queryable.Where(p => !p.IsArchive)
                                                .Select(x => new FinalTestSearchModel
                                                {
                                                    Id = x.Id,
                                                    Name = x.Name,
                                                    FinalTestLevel = x.FinalTestLevel,
                                                    CreatedDate = x.CreatedDate,
                                                    CreatedFullName = x.CreatedFullName,
                                                    ExecutionTime = x.ExecutionTime,
                                                });
            if (!string.IsNullOrEmpty(request.Keyword))
            {
                finalTestQuery = finalTestQuery.Where(m => m.Id.ToString() == request.Keyword || (m.Name ?? string.Empty).ToLower().Trim().Contains(request.Keyword.ToLower().Trim()));
            }
            if (request.FinalTestLevel != null)
            {
                finalTestQuery = finalTestQuery.Where(m => m.FinalTestLevel == request.FinalTestLevel);
            }
            int totalItem = await finalTestQuery.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await finalTestQuery
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            methodResult.Result = new PagingItemsModel<FinalTestSearchModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
