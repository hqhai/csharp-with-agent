using Fsel.Core.Base;
using Fsel.Core.Services.IpApiServices;
using Fsel.Realtime.Application.Queues.Publishers;
using Fsel.Shared.Models.ShareModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
// Đảm bảo rằng bạn đã thêm namespace của ConnectionTracker

namespace Fsel.Realtime.Application.Hubs
{
    [Authorize]
    public class FeatureAccessTimeHub : BaseHub
    {
        private readonly FeatureAccessTimePublisher _accessTimePublisher;
        private readonly AuthContext _authContext;

        public FeatureAccessTimeHub(FeatureAccessTimePublisher accessTimePublisher, AuthContext authContext, IIpApiService ipApiService) : base(authContext, ipApiService)
        {
            _accessTimePublisher = accessTimePublisher;
            _authContext = authContext;
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();


            await Groups.AddToGroupAsync(Context.ConnectionId, _authContext.CurrentUserId.ToString());
            ConnectionTracker.Instance.RecordConnectionStart(Context.ConnectionId);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);

            Guid userId = _authContext.CurrentUserId;
            string type = (Context.GetHttpContext()?.Request.Query["Type"].ToString()!);
            string lessonId = Context.GetHttpContext()?.Request.Query["LessonId"].ToString()!;
            string unitId = Context.GetHttpContext()?.Request.Query["UnitId"].ToString()!;
            string courseId = Context.GetHttpContext()?.Request.Query["CourseId"].ToString()!;
            string objectId = Context.GetHttpContext()?.Request.Query["ObjectId"].ToString()!;

            // Ghi nhận thời điểm ngắt kết nối và tính toán thời gian kết nối
            var duration = ConnectionTracker.Instance.RecordConnectionEnd(Context.ConnectionId);

            TrackingTimeModel model = new TrackingTimeModel
            {
                UserId = userId,
                EnumFeature = type,
                ObjectId = string.IsNullOrEmpty(objectId) ? null : new Guid(objectId),
                UnitId = string.IsNullOrEmpty(unitId) ? null : new Guid(unitId),
                LessonId = string.IsNullOrEmpty(lessonId) ? null : new Guid(lessonId),
                CourseId = string.IsNullOrEmpty(courseId) ? null : new Guid(courseId),
                AccessTime = duration
            };


            if (!string.IsNullOrEmpty(userId.ToString()))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId.ToString());
            }

            await _accessTimePublisher.Publish(model, CancellationToken.None);
        }
    }
}
