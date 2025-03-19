// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.AiCmd
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Models.CommandModels.Ais;
    using Fsel.Course.Lms.Application.Services.AiService;
    using Fsel.Course.Lms.Application.Services.AIService.Models;
    using MediatR;
    using Microsoft.Extensions.Logging;

    public class SubmitAICommand : SubmitAICommandModel, IRequest<string?>
    {
    }

    public class SubmitAICommandHandler : IRequestHandler<SubmitAICommand, string?>
    {
        private readonly IOpenAIService _openAIService;
        private readonly ILogger<SubmitAICommandHandler> _logger;

        public SubmitAICommandHandler(IOpenAIService openAIService, ILogger<SubmitAICommandHandler> logger)
        {
            _openAIService = openAIService;
            _logger = logger;
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
            _logger.LogCritical("ChatGPT response : ", ConvertHelper.Serialize(request), result);
            return result;
        }
    }
}
