using Fsel.Core.Base;
using Fsel.Realtime.Application.Queues.Publishers;
using Fsel.Shared.Models.ShareModels;
using Microsoft.AspNetCore.SignalR;
// Đảm bảo rằng bạn đã thêm namespace của ConnectionTracker

namespace Fsel.Realtime.Application.Hubs
{
    public class FeatureAccessTimeHub : BaseHub
    {
        private readonly FeatureAccessTimePublisher _accessTimePublisher;

        public FeatureAccessTimeHub(FeatureAccessTimePublisher accessTimePublisher)
        {
            _accessTimePublisher = accessTimePublisher;
        }

        public override async Task OnConnectedAsync()
        {

            string userId = Context.GetHttpContext()?.Request.Query["UserId"].ToString()!;


            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
                ConnectionTracker.Instance.RecordConnectionStart(Context.ConnectionId);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            Guid userId = new Guid(Context.GetHttpContext()?.Request.Query["UserId"].ToString()!);
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
            await base.OnDisconnectedAsync(exception);
        }
    }
}
