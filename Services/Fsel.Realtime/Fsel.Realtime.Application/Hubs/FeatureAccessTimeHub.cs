using Fsel.Common.Helpers;
using Fsel.Core.Base;
using Fsel.Core.Extensions;
using Fsel.Core.Services.IpApiServices;
using Fsel.Realtime.Application.Queues.Publishers;
using Fsel.Realtime.Application.Trackers;
using Fsel.Shared.Models.ShareModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

// Đảm bảo rằng bạn đã thêm namespace của ConnectionTracker

namespace Fsel.Realtime.Application.Hubs
{
    [Authorize]
    public class FeatureAccessTimeHub : BaseHub
    {
        private readonly FeatureAccessTimePublisher _accessTimePublisher;
        private readonly AuthContext _authContext;
        private readonly ILogger<FeatureAccessTimeHub> _logger;

        private string? UserAgent
        {
            get
            {
                var httpContext = Context.GetHttpContext();
                // Ưu tiên lấy từ query string
                var userAgentFromQuery = httpContext?.Request.Query["User-Agent"].ToString();
                if (!string.IsNullOrEmpty(userAgentFromQuery))
                    return userAgentFromQuery;
                // Nếu không có thì lấy từ header
                return httpContext?.Request.Headers["User-Agent"].ToString();
            }
        }

        public FeatureAccessTimeHub(FeatureAccessTimePublisher accessTimePublisher, AuthContext authContext, IIpApiService ipApiService, IHttpContextAccessor httpContextAccessor, ILogger<FeatureAccessTimeHub> logger) : base(authContext, ipApiService, httpContextAccessor)
        {
            _accessTimePublisher = accessTimePublisher;
            _authContext = authContext;
            _logger = logger;
        }

        public override async Task OnConnectedHubAsync()
        {
            await Groups.AddGroupAsync(Context.ConnectionId, _authContext.CurrentUserId.ToString());
            ConnectionTracker.Instance.RecordConnectionStart(Context.ConnectionId);
            _logger.LogInformation($"Connected FeatureAccessTime Socket: {Context.ConnectionId}, DateTime: {DateTime.UtcNow}");
        }

        public async Task AccessFeature(TrackingTimeModel model)
        {
            var userAgent = UserAgent;
            model.UserAgent = userAgent;
            var trackingModel = ConnectionTracker.Instance.GetModel(Context.ConnectionId);

            _logger.LogInformation($"TrackingModelt: {Context.ConnectionId}, type: {model.EnumFeature}, lessonId : {model.LessonId},Objectd: {model.ObjectId}, courseId: {model.CourseId}");

            if (trackingModel != null)
            {
                var duration = ConnectionTracker.Instance.RecordConnectionEnd(Context.ConnectionId);
                trackingModel.AccessTime = duration;
                await _accessTimePublisher.Publish(trackingModel, CancellationToken.None);
                _logger.LogInformation($"Invoke FeatureAccessTime Socket: {Context.ConnectionId},type: {trackingModel.EnumFeature}, duration : {duration}, model :{ConvertHelper.Serialize(trackingModel)}");
            }

            ConnectionTracker.Instance.RecordConnectionStart(Context.ConnectionId, model);
        }

        public override async Task OnDisconnectedHubAsync(Exception? exception)
        {
            var httpContext = Context.GetHttpContext();
            var query = httpContext?.Request.Query;

            var type = query?["Type"].ToString();
            var duration = ConnectionTracker.Instance.RecordConnectionEnd(Context.ConnectionId);
            var trackingModel = ConnectionTracker.Instance.GetModel(Context.ConnectionId);

            _logger.LogInformation(
                "Disconnect FeatureAccessTime Socket: {ConnectionId}, duration: {Duration}, type: {Type}",
                Context.ConnectionId, duration, type);

            TrackingTimeModel model;

            if (trackingModel != null)
            {
                trackingModel.AccessTime = duration;
                model = trackingModel;
            }
            else
            {
                var userId = _authContext.CurrentUserId;

                model = new TrackingTimeModel
                {
                    UserId = userId,
                    EnumFeature = type,
                    CourseResultId = ParseGuid(query?["CourseResultId"]),
                    CourseId = ParseGuid(query?["CourseId"]),
                    UnitId = ParseGuid(query?["UnitId"]),
                    LessonId = ParseGuid(query?["LessonId"]),
                    ObjectId = ParseGuid(query?["ObjectId"]),
                    AccessTime = duration,
                    UserAgent = UserAgent
                };

                if (userId != Guid.Empty)
                {
                    await Groups.RemoveGroupAsync(Context.ConnectionId, userId.ToString());
                }
            }

            await _accessTimePublisher.Publish(model, CancellationToken.None);
        }

        private static Guid? ParseGuid(string? value)
        {
            return Guid.TryParse(value, out var guid) ? guid : null;
        }
    }
}
