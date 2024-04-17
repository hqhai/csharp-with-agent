using Fsel.Core.Base.BaseModels;
using Fsel.System.Application.Commands.NoticeAccessFeatureCmd;
using Fsel.System.Application.Commands.QuestBoardStudentCmd;
using MassTransit;
using MediatR;

namespace Fsel.System.Application.Queues.Consumers
{
    public class NoticeAccessFeatureConsumer : IConsumer<BaseQueueModel>
    {
        private readonly IMediator _mediator;

        public NoticeAccessFeatureConsumer(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task Consume(ConsumeContext<BaseQueueModel> context)
        {
            if (context != null)
            {
                await _mediator.Send(new NoticeFeatureAccessCommand()).ConfigureAwait(false);
            }
        }
    }
}
