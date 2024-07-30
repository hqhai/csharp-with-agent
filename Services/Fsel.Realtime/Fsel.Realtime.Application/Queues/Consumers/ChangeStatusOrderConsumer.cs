// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Realtime.Application.Queues.Consumers
{
    using Fsel.Core.Base;
    using Fsel.Core.Extensions;
    using Fsel.Realtime.Application.Hubs;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels;
    using Microsoft.AspNetCore.SignalR;

    public class ChangeStatusOrderConsumer : BaseConsumer<OrderQueueModel>
    {
        private readonly IHubContext<PaymentHub> _paymentSuccessHub;

        public ChangeStatusOrderConsumer(AuthContext authContext, IHubContext<PaymentHub> paymentSuccessHub) : base(authContext)
        {
            _paymentSuccessHub = paymentSuccessHub;
        }

        public override async Task ConsumeQueue(OrderQueueModel? message)
        {
            if (message != null)
            {
                await _paymentSuccessHub.GetGroup(message.UserId.ToString()).SendAsync(RealtimeSettings.PaymentHub.Methods.Payment, message);
            }
        }
    }
}
