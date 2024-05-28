// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queues.Consumers
{
    using Fsel.Shared.Models.ShareModels;
    using Fsel.System.Application.Commands.QuestBoardCmd;
    using Fsel.System.Domain.Models.CommandModels.QuestBoards;
    using global::System.Threading.Tasks;
    using MassTransit;
    using MediatR;

    public class DoQuestBoardConsumer : IConsumer<QuestBoardQueueModel>
    {
        private readonly IMediator _mediator;

        public DoQuestBoardConsumer(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task Consume(ConsumeContext<QuestBoardQueueModel> context)
        {
            var message = context?.Message;
            if (message == null)
            {
                return;
            }
            await _mediator.Send(new DoQuestBoardCommand
            {
                StudentID = message.StudentID,
                Type = message.Type,
                Category = message.Category,
                Value = message.Value,
            }).ConfigureAwait(false);
        }
    }
}
