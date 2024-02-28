// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.InAppPurchaseService
{
    using System.Net.Http.Json;
    using Fsel.Common.Helpers;
    using Fsel.Ordering.Application.Commands.OrderCmds;
    using Fsel.Ordering.Application.Services.InAppPurchaseService.Models;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Infrastructure.ValueSettings;
    using Fsel.Shared.Enums;
    using Google.Apis.AndroidPublisher.v3;
    using Google.Apis.AndroidPublisher.v3.Data;
    using Google.Apis.Auth.OAuth2;
    using Google.Apis.Services;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Newtonsoft.Json;

    public class InAppPurchaseService : IInAppPurchaseService
    {
        private readonly AppSetting _appSetting;
        private readonly IOrderRepository _orderRepository;
        private readonly IMediator _mediator;

        public InAppPurchaseService(AppSetting appSetting, IOrderRepository orderRepository, IMediator mediator)
        {
            _appSetting = appSetting;
            _orderRepository = orderRepository;
            _mediator = mediator;
        }

        public async Task<bool> ValidatePurchase(PurchaseDetails purchaseDetails, string? uid, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(uid) || purchaseDetails == null)
            {
                return false;
            }
            if (purchaseDetails.IsIOS)
            {
                return await ValidateIos(purchaseDetails, uid, cancellationToken);
            }
            else if (purchaseDetails.IsAndroid)
            {
                return await ValidateAndroid(purchaseDetails, uid, cancellationToken);
            }
            return false;
        }

        public async Task<bool> ValidateIos(PurchaseDetails purchaseDetails, string uid, CancellationToken cancellationToken)
        {
            if (purchaseDetails == null)
            { return false; }

            string prodUrl = "https://buy.itunes.apple.com/verifyReceipt";
            string sbUrl = "https://sandbox.itunes.apple.com/verifyReceipt";

            var requestBody = new Dictionary<string, dynamic>() {
                { "receipt-data", purchaseDetails.VerificationData},
                { "password", _appSetting?.PurchaseValidationSettings?.AppStore?.SharedSecret ?? string.Empty},
                {"exclude-old-transactions", false } };

            using (var client = new HttpClient())
            {
                HttpResponseMessage response = await client.PostAsJsonAsync(prodUrl, requestBody, cancellationToken);
                var receiptJson = await response.Content.ReadAsStringAsync(cancellationToken);
                var receipt = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(receiptJson);

                // if code = 21007 go back and fetch using SB URL
                if (receipt != null && receipt["status"] == 21007)
                {
                    response = await client.PostAsJsonAsync(sbUrl, requestBody, cancellationToken);
                    receiptJson = await response.Content.ReadAsStringAsync(cancellationToken);
                    receipt = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(receiptJson);
                }

                if (receipt != null && receipt["status"] == 0)
                {
                    if (receipt != null && receipt["receipt"]["in_app"][0]["in_app_ownership_type"] == "PURCHASED")
                    {
                        await SaveReceipt(purchaseDetails, uid, receiptJson, cancellationToken);
                        return true;
                    }
                }
                client.Dispose();
            }

            return false;
        }

        public async Task<bool> ValidateAndroid(PurchaseDetails purchaseDetails, string uid, CancellationToken cancellationToken)
        {
            if (purchaseDetails == null)
            { return false; }

            using var stream = new FileStream("service-account.json", FileMode.Open, FileAccess.Read);
            ServiceAccountCredential? credential = GoogleCredential.FromStream(stream)
                                                   .UnderlyingCredential as ServiceAccountCredential;

            if (credential == null)
            {
                return false;
            }

            using (var pubService = new AndroidPublisherService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential
            }))
            {
                try
                {
                    string packageName = _appSetting?.PurchaseValidationSettings?.GooglePlay?.BundleId ?? string.Empty;
                    string productId = purchaseDetails.ProductID;
                    string token = purchaseDetails.VerificationData;
                    ProductPurchase? receipt = await pubService.Purchases.Products.Get(
                                     packageName, productId, token).ExecuteAsync(cancellationToken);
                    if (receipt != null && receipt.ConsumptionState == 1)
                    {
                        await SaveReceipt(purchaseDetails, uid, receipt.Deserialize<string>(), cancellationToken);
                        return true;
                    }
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }

        private async Task<bool> SaveReceipt(PurchaseDetails purchaseDetails, string uid, string? receipt, CancellationToken cancellationToken)
        {
            var order = await _orderRepository.Queryable.FirstOrDefaultAsync(p => p.UserId.ToString() == uid && p.Id.ToString() == purchaseDetails.ProductID && p.Status == EnumOrderStatus.New, cancellationToken);
            if (order != null)
            {
                var changeStatusOrderResult = await _mediator.Send(new ChangeStatusOrderCommand() { OrderId = order.Id, OrderStatus = EnumOrderStatus.Payment, Receipt = receipt }, cancellationToken).ConfigureAwait(false);
                if (changeStatusOrderResult.IsOK)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
