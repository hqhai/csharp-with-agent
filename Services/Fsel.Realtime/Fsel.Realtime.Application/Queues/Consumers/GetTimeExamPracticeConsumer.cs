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

    public class GetTimeExamPracticeConsumer : BaseConsumer<GetTimeModuleModel>
    {
        private readonly IHubContext<SetTimeExamPracticeHub> _setTimeModuleHub;

        public GetTimeExamPracticeConsumer(IHubContext<SetTimeExamPracticeHub> setTimeModuleHub, AuthContext authContext, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _setTimeModuleHub = setTimeModuleHub;
        }

        public override async Task ConsumeQueue(GetTimeModuleModel? message)
        {
            if (message != null)
            {
                await _setTimeModuleHub.GetGroup(message.UserId.ToString()).SendAsync(RealtimeSettings.SetTimeExamPracticeHub.Methods.SetTimeExamPracticeHub, new { Event = "GetTime", WorkingTime = message.WorkingTime, RemainingTime = message.RemainingTime });
            }
        }
    }
}
