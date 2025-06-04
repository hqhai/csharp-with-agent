namespace Fsel.Course.Lms.Application.Queues.Consumers
{
    using Fsel.Core.Base;
    using Fsel.Course.Lms.Application.Commands.OtherCmd;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;

    public class SendNotifyAfterChooseLevelConsumer : BaseConsumer<SendNotifyAfterChooseLevelQueueModel>
    {
        private readonly IMediator _mediator;

        public SendNotifyAfterChooseLevelConsumer(IMediator mediator, AuthContext authContext, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _mediator = mediator;
        }

        public override async Task ConsumeQueue(SendNotifyAfterChooseLevelQueueModel? message)
        {
            if (message == null)
            {
                return;
            }

            await _mediator.Send(new SendNotifyForStudentsCommand { NotifyAfterChooseLevelType = message.NotifyAfterChooseLevelType }).ConfigureAwait(false);
        }
    }
}
