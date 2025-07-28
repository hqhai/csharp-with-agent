namespace Fsel.Realtime.Application.Queues.Consumers
{
    using Fsel.Core.Base;
    using Fsel.Core.Extensions;
    using Fsel.Realtime.Application.Hubs;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.SignalR;
    using Microsoft.Extensions.Logging;

    public class SendNotifyBuyBlindBoxConsumer : BaseConsumer<SendNotifyBuyBlindBoxModel>
    {
        private readonly IHubContext<BuyBlindBoxHub> _buyBlindBoxHub;
        private readonly AuthContext _authContext;
        private readonly ILogger<object> _logger;

        public SendNotifyBuyBlindBoxConsumer(IHubContext<BuyBlindBoxHub> buyBlindBoxHub, AuthContext authContext, ILogger<object> logger, IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _buyBlindBoxHub = buyBlindBoxHub;
            _authContext = authContext;
            _logger = logger;
        }

        public override async Task ConsumeQueue(SendNotifyBuyBlindBoxModel? message)
        {
            if (message != null)
            {
                await _buyBlindBoxHub.GetGroup(_authContext.CurrentUserId.ToString()).SendAsync(RealtimeSettings.SendNotifyBuyBlindBoxHub.Methods.BuyBlindBox, message);
            }
        }
    }
}
