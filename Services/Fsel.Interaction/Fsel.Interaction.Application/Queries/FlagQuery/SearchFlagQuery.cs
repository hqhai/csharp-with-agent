// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Queries.FlagQuery
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Interaction.Domain.Enums;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using Fsel.Interaction.Domain.Models.QueryModels.Flags;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchFlagQuery : SearchFlagQueryModel, IRequest<MethodResult<PagingItemsModel<FlagModel>>>
    {
    }

    public class SearchFlagQueryHandler : IRequestHandler<SearchFlagQuery, MethodResult<PagingItemsModel<FlagModel>>>
    {
        private readonly IFlagRepository _flagRepository;

        public SearchFlagQueryHandler(IFlagRepository flagRepository)
        {
            _flagRepository = flagRepository;
        }

        public async Task<MethodResult<PagingItemsModel<FlagModel>>> Handle(SearchFlagQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<FlagModel>>();

            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }

            var flagQuery = _flagRepository.Queryable
                        .Where(x => x.Status == EnumFlagStatus.New)
                        .Select(x => new FlagModel
                        {
                            Id = x.Id,
                            CreatedDate = x.CreatedDate,
                            CreatedFullName = x.CreatedFullName,
                            FeedBack = x.FeedBack,
                            FlagIssue = x.FlagIssue,
                            ObjectId = x.ObjectId,
                            Status = x.Status,
                            Type = x.Type,
                        });

            int totalItem = await flagQuery.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

            var lists = await flagQuery
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            methodResult.Result = new PagingItemsModel<FlagModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
