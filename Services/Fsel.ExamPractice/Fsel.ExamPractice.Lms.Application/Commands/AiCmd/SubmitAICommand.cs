// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Lms.Application.Commands.AiCmd
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.Helpers;
    using Fsel.ExamPractice.Domain.Models.CommandModels.Ais;
    using Fsel.ExamPractice.Lms.Application.Services.AiService;
    using Fsel.ExamPractice.Lms.Application.Services.AIService.Models;
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

            string requestLog = ConvertHelper.Serialize(request);
            string responseLog = "";
            try
            {
                responseLog = responseLog + " StatusCode: " + response.StatusCode.ToString();
                if (response.Content != null)
                {
                    responseLog = responseLog + " Content: " + ConvertHelper.Serialize(response.Content);
                }
            }
            catch (Exception ex)
            {
                responseLog = result + " - Error convert json: " + ex.Message;
            }
            _logger.LogCritical($"ChatGPT response : {responseLog}, RequestLog : {requestLog}");
            return result;
        }
    }
}
