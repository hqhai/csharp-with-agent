// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Queries.Products
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
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
        private readonly IMapper _mapper;

        public SearchProductQueryHandler(IProductRepository productRepository, AuthContext authContext, IMapper mapper)
        {
            _productRepository = productRepository;
            _authContext = authContext;
            _mapper = mapper;
        }

        public async Task<MethodResult<PagingItemsModel<ProductModel>>> Handle(SearchProductQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<ProductModel>>();

            var products = await _productRepository.Queryable.Include(p => p.OrderTransactions.Where(x => x.Status == EnumOrderTransactionStatus.Requested || x.Status == EnumOrderTransactionStatus.Received)).ToListAsync(cancellationToken);

            var query = _mapper.Map<IList<ProductModel>>(products);

            query.ForEach(p =>
            {
                var product = products.FirstOrDefault(x => x.Id == p.Id);
                p.ProductStatus = p.Status == EnumProductStatus.Active;
                p.QuantityChanged = product?.OrderTransactions.Count ?? 0;
                p.RemainingQuantity = p.Quantity - p.QuantityChanged;
            });

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

            if (_authContext.Roles?.FirstOrDefault() == EnumRole.Student.ToString())
            {
                if (request.PopularOrLatest.HasValue && request.PopularOrLatest == true)
                {
                    query = query.OrderByDescending(x => x.QuantityChanged).ToList();
                }
                else if (request.PopularOrLatest.HasValue && request.PopularOrLatest == false)
                {
                    query = query.OrderByDescending(x => x.CreatedDate).ToList();
                }
                else
                {
                    query = query.OrderByDescending(x => x.ShowPriority).ToList();
                }
                if (request.Min.HasValue && request.Max.HasValue)
                {
                    query = query.Where(x => x.Price >= request.Min && x.Price <= request.Max).ToList();
                }
                query = query.Where(p => p.EventIds != null && request.EventIds != null && request.EventIds.Any(x => p.EventIds.Contains(x))).ToList();
                query = query.Where(p => p.ExpireDate.Date >= DateTime.UtcNow.Date).ToList();
            }

            IQueryable<ProductModel> queryable = query.AsQueryable();

            int totalItem = queryable.Count();

            var lists = new List<ProductModel>();

            if (_authContext.Roles?.FirstOrDefault() == EnumRole.Student.ToString())
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
