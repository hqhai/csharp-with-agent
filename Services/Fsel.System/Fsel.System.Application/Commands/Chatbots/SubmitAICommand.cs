// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.Chatbots
{
    using Fsel.Shared.Constants;
    using Fsel.System.Application.Services.AIServices;
    using Fsel.System.Application.Services.AIServices.Models;
    using Fsel.System.Domain.Models.CommandModels.ChatBot;
    using MediatR;

    public class SubmitAICommand : IRequest<string?>
    {
        public double MaxToken { get; set; }
        public double Temperature { get; set; }
        public double PresencePenalty { get; set; }
        public double TopP { get; set; }
        public string? Model { get; set; }
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
                Model = request.Model ?? ValueSettings.ChatBotSetup.Model,
                Messages = request.ChatBotMessages,
                Temperature = request.Temperature,
                MaxTokens = request.MaxToken,
                PresencePenalty = request.PresencePenalty,
                TopP = request.TopP
            });

            var result = response.Content?.Choices?.Select(x => x.Message?.Content).FirstOrDefault();
            return result;
        }
    }
}
