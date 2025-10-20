// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Queries.Products
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
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
        private readonly IMapper _mapper;
        private readonly IOrderTransactionRepository _orderTransactionRepository;

        public SearchProductQueryHandler(IProductRepository productRepository, AuthContext authContext, IMapper mapper, IOrderTransactionRepository orderTransactionRepository)
        {
            _productRepository = productRepository;
            _authContext = authContext;
            _mapper = mapper;
            _orderTransactionRepository = orderTransactionRepository;
        }

        public async Task<MethodResult<PagingItemsModel<ProductModel>>> Handle(SearchProductQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<ProductModel>>();

            var validStatuses = new[] { EnumOrderTransactionStatus.Requested, EnumOrderTransactionStatus.Received };

            var result = await (from a in _productRepository.Queryable
                                join b in _orderTransactionRepository.Queryable
                                on a.Id equals b.ProductId into transactions
                                select new
                                {
                                    Product = a,
                                    QuantityChanged = transactions.Count(x => validStatuses.Contains(x.Status)),
                                    RemainingQuantity = a.Quantity - transactions.Count(x => validStatuses.Contains(x.Status)),
                                    ProductStatus = a.Quantity > transactions.Count(x => validStatuses.Contains(x.Status)),
                                    Status = a.Quantity > transactions.Count(x => validStatuses.Contains(x.Status))
                                                ? EnumProductStatus.Active
                                                : EnumProductStatus.InActive
                                }).ToListAsync(cancellationToken);

            var query = result.Select(x =>
            {
                var product = _mapper.Map<ProductModel>(x.Product);
                product.QuantityChanged = x.QuantityChanged;
                product.RemainingQuantity = x.RemainingQuantity;
                product.ProductStatus = x.ProductStatus;
                product.Status = x.Status;
                return product;
            }).ToList();

            if (!string.IsNullOrEmpty(request.Code))
            {
                query = query.Where(p => !string.IsNullOrEmpty(p.Code) && p.Code.Contains(request.Code, StringComparison.InvariantCultureIgnoreCase)).ToList();
            }

            if (!string.IsNullOrEmpty(request.Name))
            {
                query = query.Where(p => !string.IsNullOrEmpty(p.Name) && p.Name.Contains(request.Name, StringComparison.InvariantCultureIgnoreCase)).ToList();
            }

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                query = query.Where(p => !string.IsNullOrEmpty(p.Name) && p.Name.Contains(request.Keyword, StringComparison.InvariantCultureIgnoreCase) || !string.IsNullOrEmpty(p.Code) && p.Code.Contains(request.Keyword, StringComparison.InvariantCultureIgnoreCase)).ToList();
            }

            if (request.Status.HasValue)
            {
                query = query.Where(p => p.Status == request.Status).ToList();
            }

            var currentDate = DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam);

            if (_authContext.Roles?.FirstOrDefault() == EnumRole.Student.ToString() || _authContext.Roles?.FirstOrDefault() == EnumRole.StudentCampus.ToString())
            {
                var queryShowPriority = query.Where(p => p.ShowPriority).ToList();
                var queryNotShowPriority = query.Where(p => !p.ShowPriority).ToList();

                Func<ProductModel, object> orderByCriteria = request.PopularOrLatest == true
                    ? x => x.QuantityChanged
                    : x => x.CreatedDate!;

                queryShowPriority = queryShowPriority.OrderByDescending(orderByCriteria).ToList();
                queryNotShowPriority = queryNotShowPriority.OrderByDescending(orderByCriteria).ToList();

                if (request.Min.HasValue && request.Max.HasValue)
                {
                    queryShowPriority = queryShowPriority.Where(x => x.Price >= request.Min && x.Price <= request.Max).ToList();
                    queryNotShowPriority = queryNotShowPriority.Where(x => x.Price >= request.Min && x.Price <= request.Max).ToList();
                }

                query = queryShowPriority.Concat(queryNotShowPriority).ToList();

                query = query.Where(p => p.EventIds != null && request.EventIds != null && request.EventIds.Any(x => p.EventIds.Contains(x))).ToList();

                query = query.Where(p => p.ExpireDate >= currentDate && p.RemainingQuantity > 0).ToList();
            }

            IQueryable<ProductModel> queryable = query.AsQueryable();

            int totalItem = queryable.Count();

            var lists = new List<ProductModel>();

            if (_authContext.Roles?.FirstOrDefault() == EnumRole.Student.ToString() || _authContext.Roles?.FirstOrDefault() == EnumRole.StudentCampus.ToString())
            {
                lists = queryable
                    .ApplyPaging(request)
                    .AsNoTracking()
                    .ToList();
            }
            else
            {
                lists = queryable
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToList();
            }

            methodResult.Result = new PagingItemsModel<ProductModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
