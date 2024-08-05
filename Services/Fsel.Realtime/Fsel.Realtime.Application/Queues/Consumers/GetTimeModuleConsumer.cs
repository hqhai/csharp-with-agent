// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Realtime.Application.Queues.Consumers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base;
    using Fsel.Core.Extensions;
    using Fsel.Realtime.Application.Hubs;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels;
    using Microsoft.AspNetCore.SignalR;

    public class GetTimeModuleConsumer : BaseConsumer<GetTimeModuleModel>
    {
        private readonly IHubContext<SetTimeModuleHub> _setTimeModuleHub;

        public GetTimeModuleConsumer(IHubContext<SetTimeModuleHub> setTimeModuleHub, AuthContext authContext, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _setTimeModuleHub = setTimeModuleHub;
        }

        public override async Task ConsumeQueue(GetTimeModuleModel? message)
        {
            if (message != null)
            {
                await _setTimeModuleHub.GetGroup(message.UserId.ToString()).SendAsync(RealtimeSettings.SetTimeModuleHub.Methods.SetTimeModule, new { Event = "GetTime", WorkingTime = message.WorkingTime, RemainingTime = message.RemainingTime });
            }
        }
    }
}
