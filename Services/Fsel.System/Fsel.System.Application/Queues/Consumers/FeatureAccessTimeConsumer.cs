using Fsel.Core.Base;
using Fsel.Shared.Models.ShareModels;
using Fsel.System.Application.Commands.FeatureAccessTimeCmd;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Fsel.System.Application.Queues.Consumers
{
    public class FeatureAccessTimeConsumer : BaseConsumer<TrackingTimeModel>
    {
        private readonly IMediator _mediator;

        public FeatureAccessTimeConsumer(IMediator mediator, AuthContext authContext, IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _mediator = mediator;
        }

        public override async Task ConsumeQueue(TrackingTimeModel? message)
        {
            if (message != null)
            {
                await _mediator.Send(new SaveFeatureAccessTimeCommand
                {
                    UserId = message.UserId,
                    AccessTime = message.AccessTime,
                    ObjectId = message.ObjectId,
                    CourseId = message.CourseId,
                    UnitId = message.UnitId,
                    LessonId = message.LessonId,
                    Type = message.EnumFeature,
                    UserAgent = message.UserAgent
                }).ConfigureAwait(false);
            }
        }
    }
}
