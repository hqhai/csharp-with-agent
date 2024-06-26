// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.Chatbots
{
    using Fsel.System.Application.Services.AIServices;
    using Fsel.System.Application.Services.AIServices.Models;
    using Fsel.System.Domain.Models.CommandModels.ChatBot;
    using MediatR;

    public class SubmitAICommand : IRequest<string?>
    {
        public double MaxToken { get; set; }

        public IList<ChatBotMessageModel>? ChatBotMessages { get; set; }
    }

    public class SubmitAICommandHandler : IRequestHandler<SubmitAICommand, string?>
    {
        private readonly IOpenAIService _openAIService;

        public SubmitAICommandHandler(IOpenAIService openAIService)
        {
            _openAIService = openAIService;
        }

        public async Task<string?> Handle(SubmitAICommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var response = await _openAIService.SubmitAICompletionsAsync(new RequestAIModel
            {
                Model = "gpt-4o",
                Messages = request.ChatBotMessages,
                Temperature = 0,
                MaxTokens = request.MaxToken,
                PresencePenalty = 0,
                TopP = 0
            });

            var result = response.Content?.Choices?.Select(x => x.Message?.Content).FirstOrDefault();
            return result;
        }
    }
}
