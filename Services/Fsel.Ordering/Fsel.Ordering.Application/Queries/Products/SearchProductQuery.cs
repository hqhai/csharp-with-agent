// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Queries.Products
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
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
        private readonly AuthContext _authContext;

        public SearchProductQueryHandler(IProductRepository productRepository, AuthContext authContext)
        {
            _productRepository = productRepository;
            _authContext = authContext;
        }

        public async Task<MethodResult<PagingItemsModel<ProductModel>>> Handle(SearchProductQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<ProductModel>>();

            var query = await _productRepository.Queryable.Include(p => p.OrderTransactions.Where(x => x.Status == EnumOrderTransactionStatus.Requested || x.Status == EnumOrderTransactionStatus.Received)).Select(p => new ProductModel()
            {
                Id = p.Id,
                ExpireDate = p.ExpireDate,
                Name = p.Name,
                Code = p.Code,
                Quantity = p.Quantity,
                Status = p.Status,
                Images = p.Images,
                Price = p.Price,
                ProductStatus = p.Status == EnumProductStatus.Active,
                MarketPlaceType = p.MarketPlaceType,
                ProductType = p.ProductType,
                ShowPriority = p.ShowPriority,
                RemainingQuantity = p.Quantity - p.OrderTransactions.Count(),
                QuantityChanged = p.OrderTransactions.Count(),
                EventIds = p.EventIds,
                CreatedDate = p.CreatedDate,
            }).ToListAsync(cancellationToken);

            if (!string.IsNullOrEmpty(request.Code))
            {
                query = query.Where(p => !string.IsNullOrEmpty(p.Code) && p.Code.Contains(request.Code, StringComparison.InvariantCultureIgnoreCase)).ToList();
            }

            if (!string.IsNullOrEmpty(request.Name))
            {
                query = query.Where(p => !string.IsNullOrEmpty(p.Name) && p.Name.Contains(request.Name, StringComparison.InvariantCultureIgnoreCase)).ToList();
            }

            if (request.Status.HasValue)
            {
                query = query.Where(p => p.Status == request.Status).ToList();
            }

            if (request.EventIds != null && request.EventIds.Count > 0)
            {
                query = query.Where(p => p.EventIds != null && request.EventIds.Any(x => p.EventIds.Contains(x))).ToList();
            }

            if (request.PopularOrLatest.HasValue && request.PopularOrLatest == true)
            {
                query = query.OrderByDescending(x => x.QuantityChanged).ToList();
            }
            else if (request.PopularOrLatest.HasValue && request.PopularOrLatest == false)
            {
                query = query.OrderByDescending(x => x.CreatedDate).ToList();
            }

            if (_authContext.Roles?.FirstOrDefault() == EnumRole.Student.ToString())
            {
                query = query.Where(p => p.EventIds != null && request.EventIds != null && request.EventIds.Any(x => p.EventIds.Contains(x))).ToList();
                query = query.Where(p => p.ExpireDate.Date >= DateTime.UtcNow.Date).ToList();
                query = query.OrderByDescending(x => x.ShowPriority).ToList();
            }

            IQueryable<ProductModel> queryable = query.AsQueryable();

            int totalItem = queryable.Count();

            var lists = queryable
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToList();

            methodResult.Result = new PagingItemsModel<ProductModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
