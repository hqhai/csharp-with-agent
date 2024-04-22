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


            await Groups.RemoveFromGroupAsync(Context.ConnectionId, _authContext.CurrentUserId.ToString());
            ConnectionTracker.Instance.RecordConnectionStart(Context.ConnectionId);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);

            Guid userId = _authContext.CurrentUserId;
            string type = (Context.GetHttpContext()?.Request.Query["Type"].ToString()!);
            Guid objectId = new Guid(Context.GetHttpContext()?.Request.Query["ObjectId"].ToString()!);
            Guid courseId = new Guid(Context.GetHttpContext()?.Request.Query["CourseId"].ToString()!);
            Guid lessonId = new Guid(Context.GetHttpContext()?.Request.Query["LessonId"].ToString()!);

            // Ghi nhận thời điểm ngắt kết nối và tính toán thời gian kết nối
            var duration = ConnectionTracker.Instance.RecordConnectionEnd(Context.ConnectionId);
            if (duration.HasValue)
            {
                TrackingTimeModel model = new TrackingTimeModel
                {
                    UserId = userId,
                    EnumFeature = type,
                    ObjectId = objectId,
                    UnitId = userId,
                    LessonId = lessonId,
                    CourseId = courseId,
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
}
