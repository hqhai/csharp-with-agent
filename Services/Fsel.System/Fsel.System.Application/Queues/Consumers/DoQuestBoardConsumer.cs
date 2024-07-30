// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queues.Consumers
{
    using Fsel.Core.Base;
    using Fsel.Shared.Models.ShareModels;
    using Fsel.System.Application.Commands.QuestBoardCmd;
    using global::System.Threading.Tasks;
    using MassTransit;
    using MediatR;

    public class DoQuestBoardConsumer : BaseConsumer<QuestBoardQueueModel>
    {
        private readonly IMediator _mediator;

        public DoQuestBoardConsumer(IMediator mediator, AuthContext authContext, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _mediator = mediator;
        }

        public override async Task ConsumeQueue(QuestBoardQueueModel? message)
        {
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
