namespace Fsel.Ordering.Application.Queries.MarketplacePremiumQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Ordering.Application.Queries.UrBoxQuery;
    using Fsel.Ordering.Application.Services.UrBoxService;
    using Fsel.Ordering.Domain.Entities;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.EntityModels;
    using Fsel.Ordering.Infrastructure.ValueSettings;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetProductPremiumQuery : IRequest<MethodResult<ProductModel>>
    {
        public Guid Id { get; set; }
    }

    public class GetProductPremiumQueryHandler : IRequestHandler<GetProductPremiumQuery, MethodResult<ProductModel>>
    {
        private readonly IUrBoxService _urBoxService;
        private readonly AppSetting _appSetting;
        private readonly AuthContext _languageContext;
        private readonly IMediator _mediator;
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;

        public GetProductPremiumQueryHandler(IUrBoxService urBoxService, AppSetting appSetting, AuthContext languageContext, IMediator mediator, IProductRepository productRepository, IMapper mapper)
        {
            _urBoxService = urBoxService;
            _appSetting = appSetting;
            _languageContext = languageContext;
            _mediator = mediator;
            _productRepository = productRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<ProductModel>> Handle(GetProductPremiumQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<ProductModel>();

            var product = await _productRepository.Queryable.Include(p => p.Translations).Include(p => p.OrderTransactions).FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
            if (product == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var quantityChanged = product.OrderTransactions.Where(p => p.Status == EnumOrderTransactionStatus.Requested || p.Status == EnumOrderTransactionStatus.Received || p.Status == EnumOrderTransactionStatus.Success).Count();

            var productModel = _mapper.Map<ProductModel>(product);

            productModel.QuantityChanged = quantityChanged;
            productModel.RemainingQuantity = productModel.Quantity - quantityChanged;
            productModel.Status = productModel.Quantity > quantityChanged ? EnumProductStatus.Active : EnumProductStatus.InActive;
            productModel.ProductStatus = productModel.Quantity > quantityChanged;

            if (productModel.MarketPlaceType == EnumMarketPlaceType.FSEL)
            {
                productModel.BrandName = _appSetting.MarketplacePremiumConfig?.BrandName;
                productModel.BrandImage = _appSetting.MarketplacePremiumConfig?.BrandImage;
            }
            else if (productModel.MarketPlaceType == EnumMarketPlaceType.UrBox)
            {
                var giftResult = await _mediator.Send(new GetGiftQuery() { Id = product.GlobalId }, cancellationToken);
                if (!giftResult.IsOK)
                {
                    methodResult.AddError(giftResult.ErrorMessages);
                    return methodResult;
                }

                var gift = giftResult.Result;

                productModel.BrandName = gift?.Brand;
                productModel.BrandImage = gift?.BrandImage;
                productModel.ProductGlobalConfig = new ProductGlobalConfig()
                {
                    BrandName = gift?.Brand,
                    BrandImage = gift?.BrandImage,
                    Content = gift?.Content,
                    Note = gift?.Note,
                    Addresses = gift?.Offices?.Where(x => !string.IsNullOrEmpty(x.Address)).Select(p => p.Address ?? string.Empty).ToList()
                };
            }

            methodResult.Result = productModel;

            return methodResult;
        }
    }
}
