// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Queries.UrBoxQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
    using Fsel.Ordering.Application.Services.UrBoxService;
    using Fsel.Ordering.Application.Services.UrBoxService.Models.Request;
    using Fsel.Ordering.Application.Services.UrBoxService.Models.Response;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Infrastructure.ValueSettings;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetListExchangeHistoryQuery : IRequest<MethodResult<ExchangeHistoryModel>>
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class GetListExchangeHistoryQueryHandler : IRequestHandler<GetListExchangeHistoryQuery, MethodResult<ExchangeHistoryModel>>
    {
        private readonly IUrBoxService _urBoxService;
        private readonly AppSetting _appSetting;
        private readonly AuthContext _authContext;
        private readonly IOrderTransactionRepository _orderTransactionRepository;
        private readonly IMapper _mapper;
        private readonly AuthContext _languageContext;

        public GetListExchangeHistoryQueryHandler(IUrBoxService urBoxService, AppSetting appSetting, AuthContext authContext, IOrderTransactionRepository orderTransactionRepository, IMapper mapper)
        {
            _urBoxService = urBoxService;
            _appSetting = appSetting;
            _authContext = authContext;
            _orderTransactionRepository = orderTransactionRepository;
            _mapper = mapper;
            _languageContext = authContext;
        }

        public async Task<MethodResult<ExchangeHistoryModel>> Handle(GetListExchangeHistoryQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<ExchangeHistoryModel>();

            var orderTransactions = await _orderTransactionRepository.Queryable.Where(p => p.CreatedUserId == _authContext.CurrentUserId && p.Status == EnumOrderTransactionStatus.Success && p.Type == EnumOrderTransactionType.UrBox).OrderByDescending(p => p.CreatedDate).ToListAsync(cancellationToken);

            var responses = orderTransactions.Select(p => p?.ResponseBody).Deserialize<List<RedemptionResponseModel>>();

            var gifts = responses?.Select(d => d.Data).Select(c => c.Cart).SelectMany(clg => clg.CodeLinkGift).ToList();

            List<Task<List<DetailExchangeHistory>?>> tasks = new List<Task<List<DetailExchangeHistory>?>>();

            List<Task<GetGiftDetailModel?>> taskGifts = new List<Task<GetGiftDetailModel?>>();

            var giftIds = gifts.Select(p => p.PriceId).Distinct().ToList();

            foreach (var item in orderTransactions)
            {
                tasks.Add(GetDetailExchangeHistoryAsync(item.Id));
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
                };
            });
            tasks.ForEach(t =>
            {
                if (t.Result != null)
                {
                    detailExchangeHistories.AddRange(t.Result);
                };
            });

            var history = new ExchangeHistoryModel();

            foreach (var gift in gifts)
            {
                var giftDetail = giftDetails.FirstOrDefault(p => p.Data?.Id == gift.PriceId);
                var item = detailExchangeHistories.FirstOrDefault(p => p.Id == gift.CartDetailId);
                var a = new GiftHistoryModel
                {
                    Id = gift.CartDetailId,
                    GiftId = gift.PriceId,
                    GiftName = giftDetail?.Data?.Title,
                    Price = "" + gift.Price,
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
                    Serial = gift.Serial
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
            methodResult.Result = history;
            return methodResult;
        }

        private static bool GetStatusGift(long deliveryCode)
        {
            if (deliveryCode == 2 || deliveryCode == 4 || deliveryCode == 8 || deliveryCode == 11)
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
