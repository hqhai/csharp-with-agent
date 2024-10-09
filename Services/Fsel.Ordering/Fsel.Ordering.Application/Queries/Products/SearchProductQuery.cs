// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Queries.Products
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.EntityModels;
    using Fsel.Ordering.Domain.Models.QueryModels.Products;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchProductQuery : SearchProductQueryModel, IRequest<MethodResult<PagingItemsModel<ProductModel>>>
    {
    }

    public class SearchProductQueryHandler : IRequestHandler<SearchProductQuery, MethodResult<PagingItemsModel<ProductModel>>>
    {
        private readonly IProductRepository _productRepository;

        public SearchProductQueryHandler(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<MethodResult<PagingItemsModel<ProductModel>>> Handle(SearchProductQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<ProductModel>>();

            var query = _productRepository.Queryable.Include(p => p.OrderTransactions.Where(x => x.Status == EnumOrderTransactionStatus.Requested || x.Status == EnumOrderTransactionStatus.Received)).Select(p => new ProductModel()
            {
                Id = p.Id,
                ExpireDate = p.ExpireDate,
                Name = p.Name,
                Code = p.Code,
                Quantity = p.Quantity,
                Status = p.Status,
                Product = p.Status == EnumProductStatus.Active,
                RemainingQuantity = p.Quantity - p.OrderTransactions.Count(),
            });

            if (!string.IsNullOrEmpty(request.Code))
            {
                query = query.Where(p => !string.IsNullOrEmpty(p.Code) && p.Code.Contains(request.Code));
            }

            if (!string.IsNullOrEmpty(request.Name))
            {
                query = query.Where(p => !string.IsNullOrEmpty(p.Name) && p.Name.Contains(request.Name));
            }

            if (request.Status.HasValue)
            {
                query = query.Where(p => p.Status == request.Status);
            }

            int totalItem = await query.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await query
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            methodResult.Result = new PagingItemsModel<ProductModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
