// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Queries.SupportCategoryQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using Fsel.Interaction.Domain.Models.QueryModels.SupportCategorys;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchSupportCategoryQuery : SearchSupportCategoryQueryModel, IRequest<MethodResult<PagingItemsModel<SupportCategoryModel>>>
    {
    }

    public class SearchSupportCategoryQueryHandler : IRequestHandler<SearchSupportCategoryQuery, MethodResult<PagingItemsModel<SupportCategoryModel>>>
    {
        private readonly ISupportCategoryRepository _supportCategoryRepository;

        public SearchSupportCategoryQueryHandler(ISupportCategoryRepository supportCategoryRepository)
        {
            _supportCategoryRepository = supportCategoryRepository;
        }

        public async Task<MethodResult<PagingItemsModel<SupportCategoryModel>>> Handle(SearchSupportCategoryQuery request, CancellationToken cancellationToken)
        {
            MethodResult<PagingItemsModel<SupportCategoryModel>> methodResult = new MethodResult<PagingItemsModel<SupportCategoryModel>>();
            ArgumentNullException.ThrowIfNull(request);
            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }
            var supportCategoryQuery = _supportCategoryRepository.Queryable
                                .Select(x => new SupportCategoryModel
                                {
                                    Id = x.Id,
                                    Name = x.Name,
                                    Title = x.Title,
                                    CreatedDate = x.CreatedDate,
                                    IconPath = x.IconPath,
                                    IsActive = x.IsActive,
                                });
            if (!string.IsNullOrEmpty(request.Keyword))
            {
                supportCategoryQuery = supportCategoryQuery.Where(m => m.Id.ToString() == request.Keyword || (m.Name ?? string.Empty).Contains(request.Keyword));
            }
            int totalItem = await supportCategoryQuery.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await supportCategoryQuery
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            methodResult.Result = new PagingItemsModel<SupportCategoryModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
