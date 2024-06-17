// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Realtime.Application.Hubs
{
    using Fsel.Core.Base;
    using Fsel.Core.Extensions;
    using Fsel.Core.Services.IpApiServices;
    using Fsel.Realtime.Application.Queues.Publishers;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels;
    using Microsoft.AspNetCore.SignalR;
    using Microsoft.Extensions.Logging;

    public class SetTimeModuleHub : BaseHub
    {
        private readonly SetTimeModulePublisher _setTimeModulePublisher;
        private readonly ILogger<SetTimeModuleHub> _logger;
        private readonly GetTimeModulePublisher _getTimeModulePublisher;
        private readonly IHubContext<SetTimeModuleHub> _setTimeModuleHubContext;
        private readonly AuthContext _authContext;

        public SetTimeModuleHub(SetTimeModulePublisher setTimeModulePublisher, ILogger<SetTimeModuleHub> logger, GetTimeModulePublisher getTimeModulePublisher, IHubContext<SetTimeModuleHub> setTimeModuleHubContext, AuthContext authContext, IIpApiService ipApiService) : base(authContext, ipApiService)
        {
            _setTimeModulePublisher = setTimeModulePublisher;
            _logger = logger;
            _getTimeModulePublisher = getTimeModulePublisher;
            _setTimeModuleHubContext = setTimeModuleHubContext;
            _authContext = authContext;
        }

        public override async Task OnConnectedHubAsync()
        {
            await Groups.AddGroupAsync(Context.ConnectionId, _authContext.CurrentUserId.ToString());
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
            _logger.LogInformation($"Disconnect Socket SetTimeModule  Type : {type}, ObjectId : {objectId}, DateTime: {DateTime.UtcNow}");
        }

        public async Task StartTime()
        {
            ConnectionTracker.Instance.RecordConnectionStart(Context.ConnectionId);
            await _setTimeModuleHubContext.GetGroup(_authContext.CurrentUserId.ToString()).SendAsync(RealtimeSettings.SetTimeModuleHub.Methods.SetTimeModule, new { Event = "StartTime" });

            _logger.LogInformation($"StartTime Socket SetTimeModule DateTime: {DateTime.UtcNow}");
            await Task.CompletedTask;
        }

        public async Task StopTime()
        {
            string type = (Context.GetHttpContext()?.Request.Query["Type"].ToString()!);
            string objectId = Context.GetHttpContext()?.Request.Query["ObjectId"].ToString()!;
            await DisConnectAsync(type, objectId);

            _logger.LogInformation($"StopTime Socket SetTimeModule  Type : {type}, ObjectId : {objectId}, DateTime: {DateTime.UtcNow}");
            await _setTimeModuleHubContext.GetGroup(_authContext.CurrentUserId.ToString()).SendAsync(RealtimeSettings.SetTimeModuleHub.Methods.SetTimeModule, new { Event = "StopTime" });
        }

        public async Task GetTime()
        {
            string type = Context.GetHttpContext()?.Request.Query["Type"].ToString() ?? string.Empty;
            string objectId = Context.GetHttpContext()?.Request.Query["ObjectId"].ToString() ?? string.Empty;
            await _getTimeModulePublisher.Publish(new SetTimeModuleModel
            {
                AccessTime = ConnectionTracker.Instance.GetTimeValue(Context.ConnectionId) ?? default,
                ObjectId = new Guid(objectId),
                Type = type,
            }, CancellationToken.None);

            _logger.LogInformation($"GetTime Socket SetTimeModule  Type : {type}, ObjectId : {objectId}, DateTime: {DateTime.UtcNow}");
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
