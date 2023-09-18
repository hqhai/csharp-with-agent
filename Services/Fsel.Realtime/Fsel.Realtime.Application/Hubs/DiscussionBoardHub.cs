// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Realtime.Application.Hubs
{
    using Fsel.Core.Base;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels;
    using Microsoft.AspNetCore.SignalR;

    public class DiscussionBoardHub : BaseHub
    {
        public void Send(DiscussionBoardQueueModel? model)
        {
            Clients.All.SendAsync(RealtimeSettings.DiscussionBoardHub.Methods.CommentLikeMessage, model);
        }
    }
}
