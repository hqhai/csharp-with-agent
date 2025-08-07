using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Common.Helpers;
using Fsel.Core.Base;
using Fsel.Ordering.Application.Services.UrBoxService;
using Fsel.Ordering.Application.Services.UrBoxService.Models.Request;
using Fsel.Ordering.Application.Services.UrBoxService.Models.Response;
using Fsel.Ordering.Domain.IRepositories;
using Fsel.Ordering.Domain.Models.EntityModels;
using Fsel.Ordering.Infrastructure.ValueSettings;
using Fsel.Shared.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Ordering.Application.Queries.MarketplacePremiumQuery
{
    public class GetExchangeHistoriesQuery : IRequest<MethodResult<ExchangeHistoryModel>>
    {
    }

    public class GetExchangeHistoriesQueryHandler : IRequestHandler<GetExchangeHistoriesQuery, MethodResult<ExchangeHistoryModel>>
    {
        private readonly IUrBoxService _urBoxService;
        private readonly AppSetting _appSetting;
        private readonly AuthContext _authContext;
        private readonly IOrderTransactionRepository _orderTransactionRepository;
        private readonly IMapper _mapper;
        private readonly AuthContext _languageContext;
        private readonly IProductRepository _productRepository;

        public const int deliveryCode2 = 2;
        public const int deliveryCode4 = 4;
        public const int deliveryCode8 = 8;
        public const int deliveryCode11 = 11;

        public GetExchangeHistoriesQueryHandler(IUrBoxService urBoxService, AppSetting appSetting, AuthContext authContext, IOrderTransactionRepository orderTransactionRepository, IMapper mapper, AuthContext languageContext, IProductRepository productRepository)
        {
            _urBoxService = urBoxService;
            _appSetting = appSetting;
            _authContext = authContext;
            _orderTransactionRepository = orderTransactionRepository;
            _mapper = mapper;
            _languageContext = languageContext;
            _productRepository = productRepository;
        }

        public async Task<MethodResult<ExchangeHistoryModel>> Handle(GetExchangeHistoriesQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<ExchangeHistoryModel>();

            var urboxTransactions = await (from pt in _orderTransactionRepository.Queryable
                                           join p in _productRepository.Queryable on pt.ProductId equals p.Id
                                           where p.IsPremium && pt.CreatedUserId == _authContext.CurrentUserId && pt.Type == EnumOrderTransactionType.UrBoxPremium
                                           && pt.Status == EnumOrderTransactionStatus.Success
                                           select new
                                           {
                                               UrboxTransaction = pt,
                                               Product = p
                                           }).ToListAsync(cancellationToken);

            var responses = urboxTransactions.Select(p => p.UrboxTransaction).Select(p => p?.ResponseBody).Deserialize<List<RedemptionResponseModel>>();

            var gifts = responses?.Select(d => d.Data).Select(c => c.Cart).SelectMany(clg => clg.CodeLinkGift).ToList();

            List<Task<List<DetailExchangeHistory>?>> tasks = new List<Task<List<DetailExchangeHistory>?>>();

            List<Task<GetGiftDetailModel?>> taskGifts = new List<Task<GetGiftDetailModel?>>();

            var giftIds = gifts.Select(p => p.PriceId).Distinct().ToList();

            foreach (var item in urboxTransactions)
            {
                tasks.Add(GetDetailExchangeHistoryAsync(item.UrboxTransaction.Id));
            }

            foreach (var item in giftIds)
            {
                taskGifts.Add(GetDetailGift(item, _languageContext.CurrentCountryInfo?.CultureCode?.Substring(0, 2)));
            }

            await Task.WhenAll(tasks);
            await Task.WhenAll(taskGifts);

            var detailExchangeHistories = new List<DetailExchangeHistory>();
            var giftDetails = new List<GetGiftDetailModel>();
            taskGifts.ForEach(t =>
            {
                if (t.Result != null)
                {
                    giftDetails.Add(t.Result);
                }
                ;
            });
            tasks.ForEach(t =>
            {
                if (t.Result != null)
                {
                    detailExchangeHistories.AddRange(t.Result);
                }
                ;
            });

            var history = new ExchangeHistoryModel();

            foreach (var gift in gifts)
            {
                var giftDetail = giftDetails.FirstOrDefault(p => p.Data?.Id == gift.PriceId);
                var item = detailExchangeHistories.FirstOrDefault(p => p.Id == gift.CartDetailId);
                var urboxTransaction = urboxTransactions.FirstOrDefault(p => p.Product.GlobalId == gift.PriceId);
                if (urboxTransaction != null)
                {
                    var a = new GiftHistoryModel
                    {
                        Id = urboxTransaction.Product.Id.ToString(),
                        GiftId = gift.PriceId,
                        GiftName = giftDetail?.Data?.Title,
                        Price = "" + urboxTransaction.Product.Price,
                        Content = giftDetail?.Data?.Content,
                        Note = giftDetail?.Data?.Note,
                        Expired = gift.Expired,
                        CodeImage = gift.CodeImage,
                        Code = gift.Code,
                        Image = giftDetail?.Data?.Image,
                        BrandImage = giftDetail?.Data?.BrandImage,
                        BrandTitle = giftDetail?.Data?.BrandImage,
                        Delivery = item?.Delivery,
                        Offices = giftDetail?.Data?.Offices?.Select(p => p.Address).ToList(),
                        Pin = gift.Pin,
                        Serial = gift.Serial,
                        MarketPlaceType = EnumMarketPlaceType.UrBox
                    };
                    if (item != null && GetStatusGift(item.DeliveryCode))
                    {
                        history.Used.Add(a);
                    }
                    else
                    {
                        history.UnUsed.Add(a);
                    }
                }
            }

            var productTransactions = await (from pt in _orderTransactionRepository.Queryable
                                             join p in _productRepository.Queryable on pt.ProductId equals p.Id
                                             where p.IsPremium && pt.CreatedUserId == _authContext.CurrentUserId && pt.Type == EnumOrderTransactionType.ProductPremium
                                             && (pt.Status == EnumOrderTransactionStatus.Requested || pt.Status == EnumOrderTransactionStatus.Received)
                                             select new
                                             {
                                                 ProductTransaction = pt,
                                                 Product = p
                                             }).ToListAsync(cancellationToken);

            foreach (var productTransaction in productTransactions)
            {
                var product = _mapper.Map<ProductModel>(productTransaction.Product);

                if (productTransaction.ProductTransaction.Status == EnumOrderTransactionStatus.Requested)
                {
                    history.UnUsed.Add(new GiftHistoryModel()
                    {
                        Id = product.Id.ToString(),
                        GiftId = product.GlobalId,
                        GiftName = product.Name,
                        Price = "" + productTransaction.Product.Price,
                        BrandImage = _appSetting.MarketplacePremiumConfig?.BrandImage,
                        BrandTitle = _appSetting.MarketplacePremiumConfig?.BrandName,
                        Pin = productTransaction.ProductTransaction.Code,
                        Code = productTransaction.ProductTransaction.Code,
                        Image = productTransaction.Product.Images?.FirstOrDefault(),
                        MarketPlaceType = EnumMarketPlaceType.FSEL
                    });
                }
                else
                {
                    history.Used.Add(new GiftHistoryModel()
                    {
                        Id = productTransaction.Product.Id.ToString(),
                        GiftId = product.Id.ToString(),
                        GiftName = product.Name,
                        Price = "" + productTransaction.Product.Price,
                        BrandImage = _appSetting.MarketplacePremiumConfig?.BrandImage,
                        BrandTitle = _appSetting.MarketplacePremiumConfig?.BrandName,
                        Pin = productTransaction.ProductTransaction.Code,
                        Code = productTransaction.ProductTransaction.Code,
                        Image = productTransaction.Product.Images?.FirstOrDefault(),
                        MarketPlaceType = EnumMarketPlaceType.FSEL
                    });
                }
            }

            methodResult.Result = history;
            return methodResult;
        }

        private static bool GetStatusGift(long deliveryCode)
        {
            if (deliveryCode == deliveryCode2 || deliveryCode == deliveryCode4 || deliveryCode == deliveryCode8 || deliveryCode == deliveryCode11)
            {
                return true;
            }
            return false;
        }

        private async Task<List<DetailExchangeHistory>?> GetDetailExchangeHistoryAsync(Guid id)
        {
            var detailExchangeHistoryResult = await _urBoxService.GetDetailExchangeHistory(new GetDetailExchangeHistoryModel(_appSetting)
            {
                TransactionId = id.ToString(),
            });

            if (detailExchangeHistoryResult.IsSuccessStatusCode)
            {
                return detailExchangeHistoryResult.Content?.Data?.Detail?.ToList();
            }

            return null;
        }

        private async Task<GetGiftDetailModel?> GetDetailGift(string? id, string? language)
        {
            var theGiftResult = await _urBoxService.Get(new GetTheGiftQueryModel(_appSetting)
            {
                AppSecret = _appSetting.UrBoxConfig?.AppSecret,
                AppId = _appSetting.UrBoxConfig?.AppId,
                Id = id,
                Language = language,
            });

            var theGift = theGiftResult.Content;
            if (theGift?.Status == 200)
            {
                return theGift;
            }
            return null;
        }
    }
}
