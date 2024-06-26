// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Realtime.Application.Queues.Consumers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base;
    using Fsel.Realtime.Application.Hubs;
    using Fsel.Shared.Models.ShareModels;

    public class DisconnectSocketCalculateTimeConsumer : BaseConsumer<SetTimeModuleModel>
    {
        private readonly SetTimeModuleHub _timeModuleHub;

        public DisconnectSocketCalculateTimeConsumer(SetTimeModuleHub timeModuleHub, AuthContext authContext) : base(authContext)
        {
            _timeModuleHub = timeModuleHub;
        }

        public override async Task ConsumeQueue(SetTimeModuleModel? message)
        {
            await _timeModuleHub.OnDisconnectedToTimeAsync(message);
        }
    }
}
