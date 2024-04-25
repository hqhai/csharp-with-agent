using Fsel.Core.Base;
using Fsel.Core.Base.BaseModels;
using Fsel.System.Application.Commands.NoticeAccessFeatureCmd;
using MassTransit;
using MediatR;

namespace Fsel.System.Application.Queues.Consumers
{
    public class NoticeAccessFeatureConsumer : BaseConsumer<BaseQueueModel>
    {
        private readonly IMediator _mediator;

        public NoticeAccessFeatureConsumer(IMediator mediator, AuthContext authContext) : base(authContext)
        {
            _mediator = mediator;
        }

        public override async Task ConsumeQueue(ConsumeContext<BaseQueueDataModel<BaseQueueModel>> context)
        {
            if (context != null)
            {
                await _mediator.Send(new NoticeFeatureAccessCommand()).ConfigureAwait(false);
            }
        }
    }
}
