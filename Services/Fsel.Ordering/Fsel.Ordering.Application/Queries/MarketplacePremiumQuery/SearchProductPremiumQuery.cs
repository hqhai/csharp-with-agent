namespace Fsel.Ordering.Application.Queries.MarketplacePremiumQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Ordering.Application.Services.UrBoxService;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.EntityModels;
    using Fsel.Ordering.Infrastructure.ValueSettings;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchProductPremiumQuery : BaseQueryModel, IRequest<MethodResult<PagingItemsModel<ProductModel>>>
    {
    }

    public class SearchProductPremiumQueryHandler : IRequestHandler<SearchProductPremiumQuery, MethodResult<PagingItemsModel<ProductModel>>>
    {
        private readonly IUrBoxService _urBoxService;
        private readonly AppSetting _appSetting;
        private readonly AuthContext _languageContext;
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;
        private readonly IOrderTransactionRepository _orderTransactionRepository;

        public SearchProductPremiumQueryHandler(IUrBoxService urBoxService, AppSetting appSetting, AuthContext languageContext, IProductRepository productRepository, IMapper mapper, IOrderTransactionRepository orderTransactionRepository)
        {
            _urBoxService = urBoxService;
            _appSetting = appSetting;
            _languageContext = languageContext;
            _productRepository = productRepository;
            _mapper = mapper;
            _orderTransactionRepository = orderTransactionRepository;
        }

        public async Task<MethodResult<PagingItemsModel<ProductModel>>> Handle(SearchProductPremiumQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<ProductModel>>();

            var productEntities = await _productRepository.Queryable.Include(p => p.OrderTransactions).Where(p => p.IsPremium).OrderBy(p => p.Price).ToListAsync(cancellationToken);

            var products = _mapper.Map<IList<ProductModel>>(productEntities);

            int totalItem = products.Count;

            var lists = products
                    .ApplySortAndPaging(request)
                    .ToList();

            var productIds = lists.Select(p => p.Id).ToList();

            var transactions = await _orderTransactionRepository.Queryable.WhereBulkContains(productIds, p => p.ProductId).ToListAsync(cancellationToken);

            lists.ForEach(p =>
            {
                p.BrandName = p.MarketPlaceType == EnumMarketPlaceType.FSEL ? _appSetting.MarketplacePremiumConfig?.BrandName : p.ProductGlobalConfig?.BrandName;
                p.BrandImage = p.MarketPlaceType == EnumMarketPlaceType.FSEL ? _appSetting.MarketplacePremiumConfig?.BrandImage : p.ProductGlobalConfig?.BrandImage;
                p.RemainingQuantity = p.Quantity - (p.MarketPlaceType == EnumMarketPlaceType.FSEL ? transactions.Where(x => x.ProductId == p.Id && (x.Status == EnumOrderTransactionStatus.Requested || x.Status == EnumOrderTransactionStatus.Received)).Count() : transactions.Where(x => x.ProductId == p.Id && x.Status == EnumOrderTransactionStatus.Success).Count());
            });

            methodResult.Result = new PagingItemsModel<ProductModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
