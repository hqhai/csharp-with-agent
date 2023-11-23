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

            var userAiConfig = request.ClassForum?.UserAlConfig?.Replace("{0}", request.WordContent, StringComparison.CurrentCulture);
            var response = await _openAIService.SubmitAICompletionsAsync(new RequestAIModel
            {
                Model = request.ClassForum?.SettingModel,
                Messages = new List<object>
                    {
                        new
                        {
                            Role =  "system",
                            Content =  request.ClassForum?.SystemRoleAlConfig,
                        },
                        new
                        {
                            Role = "user",
                            Content = userAiConfig,
                        }
                    },
                Temperature = request.ClassForum!.SettingTemperature,
                FrequencyPenalty = request.ClassForum.SettingFrequecy,
                MaxTokens = request.ClassForum.SettingWordMaxLength,
                PresencePenalty = request.ClassForum.SettingPresence,
                TopP = request.ClassForum.SettingTopP
            });
            return response.Content?.Choices?.Select(x => x.Message?.Content).FirstOrDefault();
        }
    }
}
