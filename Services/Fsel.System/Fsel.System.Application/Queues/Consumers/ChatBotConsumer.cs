using Fsel.Core.Base;
using Fsel.Shared.Models.ShareModels;
using Fsel.System.Application.Commands.Chatbots;
using MediatR;

namespace Fsel.System.Application.Queues.Consumers
{
    public class ChatBotConsumer : BaseConsumer<ChatBotSendingMessageModel>
    {
        private readonly IMediator _mediator;

        public ChatBotConsumer(IMediator mediator, AuthContext authContext, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _mediator = mediator;
        }

        public override async Task ConsumeQueue(ChatBotSendingMessageModel? message)
        {
            if (message != null)
            {
                await _mediator.Send(new SaveChatBotMessageCommand
                {
                    Content = message.Content,
                    ChatBotId = message.ChatbotId
                }).ConfigureAwait(false);
            }
        }
    }
}
