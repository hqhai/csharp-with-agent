// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Realtime.Application.Hubs
{
    using Fsel.Core.Base;
    using Fsel.Core.Extensions;
    using Fsel.Core.Services.IpApiServices;
    using Fsel.Realtime.Application.Queues.Publishers;
    using Fsel.Shared.Models.ShareModels;
    using Microsoft.AspNetCore.SignalR;

    public class SetTimeModuleHub : BaseHub
    {
        private readonly SetTimeModulePublisher _setTimeModulePublisher;
        private readonly AuthContext _authContext;

        public SetTimeModuleHub(SetTimeModulePublisher setTimeModulePublisher, AuthContext authContext, IIpApiService ipApiService) : base(authContext, ipApiService)
        {
            _setTimeModulePublisher = setTimeModulePublisher;
            _authContext = authContext;
        }

        public override async Task OnConnectedHubAsync()
        {
            await Groups.AddGroupAsync(Context.ConnectionId, _authContext.CurrentUserId.ToString());

            var startTime = Context.GetHttpContext()?.Request.Query["IsStartTime"].ToString();
            bool isStartTime = bool.Parse(string.IsNullOrEmpty(startTime) ? "true" : startTime);
            if (isStartTime)
            {
                ConnectionTracker.Instance.RecordConnectionStart(Context.ConnectionId);
            }
        }

        public override async Task OnDisconnectedHubAsync(Exception? exception)
        {
            Guid userId = _authContext.CurrentUserId;
            string type = (Context.GetHttpContext()?.Request.Query["Type"].ToString()!);
            string objectId = Context.GetHttpContext()?.Request.Query["ObjectId"].ToString()!;
            if (string.IsNullOrEmpty(objectId))
            {
                return;
            }
            await DisConnectAsync(type, objectId);

            if (!string.IsNullOrEmpty(userId.ToString()))
            {
                await Groups.RemoveGroupAsync(Context.ConnectionId, userId.ToString());
            }
        }

        public async Task StartTime()
        {
            ConnectionTracker.Instance.RecordConnectionStart(Context.ConnectionId);
            await Task.CompletedTask;
        }

        public async Task StopTime()
        {
            string type = (Context.GetHttpContext()?.Request.Query["Type"].ToString()!);
            string objectId = Context.GetHttpContext()?.Request.Query["ObjectId"].ToString()!;
            await DisConnectAsync(type, objectId);
        }

        public async Task DisConnectAsync(string type, string objectId)
        {
            var duration = ConnectionTracker.Instance.RecordConnectionEnd(Context.ConnectionId);
            if (duration.HasValue && duration.Value > 0)
            {
                await _setTimeModulePublisher.Publish(new SetTimeModuleModel
                {
                    Type = type,
                    ObjectId = new Guid(objectId),
                    AccessTime = duration ?? default
                }, CancellationToken.None);
            }
        }
    }
}
