// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Queries.SupportQuestionQuery
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using Fsel.Interaction.Domain.Models.QueryModels.SupportQuestions;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchSupportQuestionQuery : SearchSupportQuestionQueryModel, IRequest<MethodResult<PagingItemsModel<SupportQuestionModel>>>
    {
    }

    public class SearchSupportQuestionQueryHandler : IRequestHandler<SearchSupportQuestionQuery, MethodResult<PagingItemsModel<SupportQuestionModel>>>
    {
        private readonly ISupportQuestionRepository _supportQuestionRepository;

        public SearchSupportQuestionQueryHandler(ISupportQuestionRepository supportQuestionRepository)
        {
            _supportQuestionRepository = supportQuestionRepository;
        }

        public async Task<MethodResult<PagingItemsModel<SupportQuestionModel>>> Handle(SearchSupportQuestionQuery request, CancellationToken cancellationToken)
        {
            MethodResult<PagingItemsModel<SupportQuestionModel>> methodResult = new MethodResult<PagingItemsModel<SupportQuestionModel>>();
            ArgumentNullException.ThrowIfNull(request);
            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }

            var supportQuestionQuery = _supportQuestionRepository.Queryable
                                .Select(x => new SupportQuestionModel
                                {
                                    Id = x.Id,
                                    Name = x.Name,
                                    IsFrequent = x.IsFrequent,
                                    CreatedDate = x.CreatedDate,
                                    Content = x.Content,
                                    IsActive = x.IsActive,
                                    SupportCategoryId = x.SupportCategoryId,
                                });
            if (!string.IsNullOrEmpty(request.Keyword))
            {
                supportQuestionQuery = supportQuestionQuery.Where(m => m.Id.ToString() == request.Keyword || (m.Name ?? string.Empty).ToLower().Trim().Contains(request.Keyword.ToLower().Trim()));
            }

            if (request.SupportCategoryId != null)
            {
                supportQuestionQuery = supportQuestionQuery.Where(m => m.SupportCategoryId == request.SupportCategoryId);
            }
            if (request.IsFrequent != null)
            {
                supportQuestionQuery = supportQuestionQuery.Where(m => m.IsFrequent == request.IsFrequent);
            }
            int totalItem = await supportQuestionQuery.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await supportQuestionQuery
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            methodResult.Result = new PagingItemsModel<SupportQuestionModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
