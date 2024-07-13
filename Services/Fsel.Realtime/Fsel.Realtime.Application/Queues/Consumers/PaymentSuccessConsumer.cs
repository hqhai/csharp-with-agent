// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Realtime.Application.Queues.Consumers
{
    using Fsel.Core.Base;
    using Fsel.Core.Extensions;
    using Fsel.Realtime.Application.Hubs;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels;
    using Microsoft.AspNetCore.SignalR;

    public class PaymentSuccessConsumer : BaseConsumer<OrderQueueModel>
    {
        private readonly IHubContext<PaymentSuccessHub> _paymentSuccessHub;

        public PaymentSuccessConsumer(AuthContext authContext, IHubContext<PaymentSuccessHub> paymentSuccessHub) : base(authContext)
        {
            _paymentSuccessHub = paymentSuccessHub;
        }

        public override async Task ConsumeQueue(OrderQueueModel? message)
        {
            if (message != null)
            {
                await _paymentSuccessHub.GetGroup(message.UserId.ToString()).SendAsync(RealtimeSettings.PaymentSuccessHub.Methods.PaymentSuccess, message);
            }
        }
    }
}
