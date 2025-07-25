// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Realtime.Application.Hubs
{
    using Fsel.Core.Base;
    using Fsel.Core.Extensions;
    using Fsel.Core.Services.IpApiServices;
    using Fsel.Realtime.Application.Queues.Publishers;
    using Fsel.Realtime.Application.Trackers;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.SignalR;
    using Microsoft.Extensions.Logging;

    public class SetTimeExamPracticeHub : BaseHub
    {
        private readonly SetTimeExamPracticePublisher _setTimeModulePublisher;
        private readonly ILogger<SetTimeExamPracticeHub> _logger;
        private readonly GetTimeExamPracticePublisher _getTimeModulePublisher;
        private readonly IHubContext<SetTimeExamPracticeHub> _setTimeModuleHubContext;
        private readonly AuthContext _authContext;

        public SetTimeExamPracticeHub(SetTimeExamPracticePublisher setTimeModulePublisher, ILogger<SetTimeExamPracticeHub> logger, GetTimeExamPracticePublisher getTimeModulePublisher, IHubContext<SetTimeExamPracticeHub> setTimeModuleHubContext, AuthContext authContext, IIpApiService ipApiService, IHttpContextAccessor httpContextAccessor) : base(authContext, ipApiService, httpContextAccessor)
        {
            _setTimeModulePublisher = setTimeModulePublisher;
            _logger = logger;
            _getTimeModulePublisher = getTimeModulePublisher;
            _setTimeModuleHubContext = setTimeModuleHubContext;
            _authContext = authContext;
        }

        public override async Task OnConnectedHubAsync()
        {
            string type = (Context.GetHttpContext()?.Request.Query["Type"].ToString()!);
            string objectId = Context.GetHttpContext()?.Request.Query["ObjectId"].ToString()!;
            await Groups.AddGroupAsync(Context.ConnectionId, _authContext.CurrentUserId.ToString());
            _logger.LogInformation($"Connected Socket SetTimeExamPractice ConnectId : {Context.ConnectionId}, Type : {type}, ObjectId : {objectId}, DateTime: {DateTime.UtcNow}");
        }

        public override async Task OnDisconnectedHubAsync(Exception? exception = default)
        {
            Guid userId = _authContext.CurrentUserId;
            string type = (Context.GetHttpContext()?.Request.Query["Type"].ToString()!);
            string objectId = Context.GetHttpContext()?.Request.Query["ObjectId"].ToString()!;
            if (string.IsNullOrEmpty(objectId))
            {
                return;
            }

            ConnectionTracker.Instance.RecordConnectionEndUser(userId.ToString());
            await DisConnectAsync(type, objectId);
            if (!string.IsNullOrEmpty(userId.ToString()))
            {
                await _setTimeModuleHubContext.GetGroup(_authContext.CurrentUserId.ToString()).SendAsync(RealtimeSettings.SetTimeExamPracticeHub.Methods.SetTimeExamPracticeHub, new { Event = "Disconnect" });
                await Groups.RemoveGroupAsync(Context.ConnectionId, userId.ToString());
            }

            _logger.LogInformation($"Disconnect SetTimeExamPractice 1 : {Context.ConnectionId}, Type : {type}, ObjectId : {objectId}, DateTime: {DateTime.UtcNow}");
        }

        public async Task OnDisconnectedToTimeAsync(SetTimeModuleModel? setTimeModule)
        {
            var userId = _authContext.CurrentUserId.ToString();
            if (setTimeModule == null || setTimeModule.Type == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(setTimeModule.ObjectId.ToString()))
            {
                return;
            }
            var connectionId = ConnectionTracker.Instance.RecordConnectionEndUser(userId);
            if (connectionId != null)
            {
                _logger.LogInformation($"Disconnect SetTimeExamPractice 2: {connectionId}, Type : {setTimeModule.Type}, ObjectId : {setTimeModule.ObjectId}, DateTime: {DateTime.UtcNow}");

                await DisConnectAsync(setTimeModule.Type, setTimeModule.ObjectId.ToString(), connectionId, setTimeModule.SubmissionCount);

                await _setTimeModuleHubContext.GetGroup(userId).SendAsync(RealtimeSettings.SetTimeModuleHub.Methods.SetTimeModule, new { Event = "Disconnect" });
                await _setTimeModuleHubContext.Groups.RemoveGroupAsync(connectionId, userId);
            }

            _logger.LogInformation($"Disconnect SetTimeExamPractice 3: {connectionId}, Type : {setTimeModule.Type}, ObjectId : {setTimeModule.ObjectId}, DateTime: {DateTime.UtcNow}");
        }

        public async Task StartTime()
        {
            var userId = _authContext.CurrentUserId.ToString();

            ConnectionTracker.Instance.RecordConnectionStart(Context.ConnectionId);
            ConnectionTracker.Instance.RecordConnectionStartUser(Context.ConnectionId, userId);

            await _setTimeModuleHubContext.GetGroup(userId).SendAsync(RealtimeSettings.SetTimeExamPracticeHub.Methods.SetTimeExamPracticeHub, new { Event = "StartTime" });

            _logger.LogInformation($"Connect SetTimeExamPractice : {Context.ConnectionId}");
            await Task.CompletedTask;
        }

        public async Task StopTime()
        {
            string type = (Context.GetHttpContext()?.Request.Query["Type"].ToString()!);
            string objectId = Context.GetHttpContext()?.Request.Query["ObjectId"].ToString()!;

            var userId = _authContext.CurrentUserId.ToString();
            ConnectionTracker.Instance.RecordConnectionEndUser(userId);

            await DisConnectAsync(type, objectId);
            await _setTimeModuleHubContext.GetGroup(userId).SendAsync(RealtimeSettings.SetTimeExamPracticeHub.Methods.SetTimeExamPracticeHub, new { Event = "StopTime" });
            _logger.LogInformation($"Disconnect SetTimeExamPractice 4: {Context.ConnectionId}");
        }

        public async Task GetTime()
        {
            string type = Context.GetHttpContext()?.Request.Query["Type"].ToString() ?? string.Empty;
            string objectId = Context.GetHttpContext()?.Request.Query["ObjectId"].ToString() ?? string.Empty;
            await _getTimeModulePublisher.Publish(new SetTimeExamPracticeModel
            {
                AccessTime = ConnectionTracker.Instance.GetTimeValue(Context.ConnectionId) ?? default,
                ObjectId = new Guid(objectId),
                Type = type,
            }, CancellationToken.None);

            _logger.LogInformation($"GetTime Socket SetTimeExamPractice ConnectId : {Context.ConnectionId}, Type : {type}, ObjectId : {objectId}, DateTime: {DateTime.UtcNow}");
        }

        public async Task DisConnectAsync(string type, string objectId, string? connectionId = null, EnumSubmissionCount? submissionCount = default)
        {
            var duration = ConnectionTracker.Instance.RecordConnectionEnd(connectionId ?? Context.ConnectionId);
            if (duration.HasValue && duration.Value > 0)
            {
                await _setTimeModulePublisher.Publish(new SetTimeModuleModel
                {
                    Type = type,
                    ObjectId = new Guid(objectId),
                    AccessTime = duration ?? default,
                    SubmissionCount = submissionCount
                }, CancellationToken.None);
            }
        }
    }
}
