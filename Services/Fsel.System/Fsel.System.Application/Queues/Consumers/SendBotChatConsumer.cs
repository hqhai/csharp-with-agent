// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queues.Consumers
{
    using Fsel.Core.Base;
    using Fsel.Shared.Models.ShareModels;
    using Fsel.System.Application.Commands.Chatbots;
    using global::System.Threading.Tasks;
    using MediatR;

    public class SendBotChatConsumer : BaseConsumer<ChatBotSendingMessageModel>
    {
        private readonly IMediator _mediator;

        public SendBotChatConsumer(IMediator mediator, AuthContext authContext, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _mediator = mediator;
        }

        public override async Task ConsumeQueue(ChatBotSendingMessageModel? message)
        {
            if (message != null)
            {
                await _mediator.Send(new SaveBotChatMessageCommand
                {
                    ChatBotId = message.ChatbotId
                }).ConfigureAwait(false);
            }
        }
    }
}
