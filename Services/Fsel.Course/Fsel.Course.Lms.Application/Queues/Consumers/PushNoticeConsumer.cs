namespace Fsel.Course.Lms.Application.Queues.Consumers
{
    using Fsel.Core.Base;
    using Fsel.Course.Lms.Application.Commands.OtherCmd;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;

    public class PushNoticeConsumer : BaseConsumer<PushNoticeQueueModel>
    {
        private readonly IMediator _mediator;

        public PushNoticeConsumer(IMediator mediator, AuthContext authContext, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _mediator = mediator;
        }

        public override async Task ConsumeQueue(PushNoticeQueueModel? message)
        {
            if (message == null)
            {
                return;
            }

            await _mediator.Send(new PushNoticeCommand { TimeNotifyType = message.TimeNotifyType }).ConfigureAwait(false);
        }
    }
}
