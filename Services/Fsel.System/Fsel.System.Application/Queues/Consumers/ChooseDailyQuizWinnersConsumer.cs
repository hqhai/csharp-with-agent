namespace Fsel.System.Application.Queues.Consumers
{
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.System.Application.Commands.DailyQuiz;
    using MediatR;

    public class ChooseDailyQuizWinnersConsumer : BaseConsumer<BaseQueueModel>
    {
        private readonly IMediator _mediator;

        public ChooseDailyQuizWinnersConsumer(IMediator mediator, AuthContext authContext) : base(authContext)
        {
            _mediator = mediator;
        }

        public override async Task ConsumeQueue(BaseQueueModel? message)
        {
            if (message == null)
            {
                return;
            }

            await _mediator.Send(new ChooseDailyQuizWinnersCommand()).ConfigureAwait(false);
        }
    }
}
