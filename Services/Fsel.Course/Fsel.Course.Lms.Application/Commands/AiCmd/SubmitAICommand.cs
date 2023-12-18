// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.AiCmd
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Course.Domain.Models.CommandModels.Ais;
    using Fsel.Course.Lms.Application.Services.AiService;
    using Fsel.Course.Lms.Application.Services.AIService.Models;
    using MediatR;

    public class SubmitAICommand : SubmitAICommandModel, IRequest<string?>
    {
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
                Model = request.SettingModel,
                Messages = new List<object>
                {
                    new
                    {
                        Role =  "system",
                        Content =  request.SystemRoleAlConfig,
                    },
                    new
                    {
                        Role = "user",
                        Content = request.UserAIConfig,
                    }
                },
                Temperature = request.SettingTemperature,
                FrequencyPenalty = request.SettingFrequecy,
                MaxTokens = request.SettingWordMaxLength,
                PresencePenalty = request.SettingPresence,
                TopP = request.SettingTopP
            });

            var result = response.Content?.Choices?.Select(x => x.Message?.Content).FirstOrDefault();
            return result;
        }
    }
}
