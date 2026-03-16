// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.Chatbots
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.System.Application.Queues.Publisher;
    using Fsel.System.Domain.Entities.Chatbots;
    using Fsel.System.Domain.Enums.ErrorCodes;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.CommandModels.ChatBot;
    using Fsel.System.Domain.Models.EntityModels;
    using global::System.ComponentModel.DataAnnotations;
    using global::System.Threading;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class SaveChatBotMessageCommand : IRequest<MethodResult<ChatBotModel>>
    {
        [MaxLength(4000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Content { get; set; }

        public Guid ChatBotId { get; set; }
    }

    public class SaveChatBotMessageCommandHandler : IRequestHandler<SaveChatBotMessageCommand, MethodResult<ChatBotModel>>
    {
        private readonly IMapper _mapper;
        private readonly IChatBotRepository _chatBotRepository;
        private readonly SendBotChatPublisher _sendBotChatPublisher;

        public SaveChatBotMessageCommandHandler(IMapper mapper, IChatBotRepository chatBotRepository, SendBotChatPublisher sendBotChatPublisher)
        {
            _mapper = mapper;
            _chatBotRepository = chatBotRepository;
            _sendBotChatPublisher = sendBotChatPublisher;
        }

        public async Task<MethodResult<ChatBotModel>> Handle(SaveChatBotMessageCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var result = new MethodResult<ChatBotModel>();

            var chatbot = await GetChatBotAsync(request.ChatBotId, result, cancellationToken);
            if (!result.IsOK)
            {
                return result;
            }

            if (chatbot.RemainToken == 0)
            {
                result.AddErrorBadRequest(nameof(EnumOutOfAIToken.TheNumberOfTokensHasReachedTheLimit));
                return result;
            }

            // ✅ Append user message
            var conversations = _mapper.Map<List<ChatbotResponseModel>>(chatbot.Conversations);
            var userMessage = CompletionElement("user", request.Content);
            conversations.Add(userMessage);
            chatbot.Conversations = _mapper.Map<List<ChatBotMessage>>(conversations);

            // ✅ Save DB
            _chatBotRepository.Update(chatbot);
            await _chatBotRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            await _sendBotChatPublisher.Publish(new Shared.Models.ShareModels.ChatBotSendingMessageModel
            {
                ChatbotId = chatbot.Id
            }, cancellationToken);
            return result;
        }

        private async Task<ChatBot> GetChatBotAsync(
            Guid chatBotId,
            MethodResult<ChatBotModel> result,
            CancellationToken ct)
        {
            var chatbot = await _chatBotRepository.Queryable
                .FirstOrDefaultAsync(x => x.Id == chatBotId, ct);

            if (chatbot == null)
            {
                result.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return new ChatBot();
            }

            return chatbot;
        }

        private static ChatbotResponseModel CompletionElement(
            string? role,
            string? content,
            string? filePath = null)
        {
            return new ChatbotResponseModel
            {
                Role = role,
                Content = content,
                FilePath = filePath
            };
        }
    }
}
