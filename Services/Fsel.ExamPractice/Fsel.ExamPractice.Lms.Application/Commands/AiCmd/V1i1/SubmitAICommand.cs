// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Lms.Application.Commands.AiCmd.V1i1
{
    using System;
    using System.Text.Json.Nodes;
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

            var modelAI = new RequestSchemaAIModel
            {
                Model = request.SettingModel,
                Input = new List<object>
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
                Text = new
                {
                    Format = request.Format == null ? null : JsonNode.Parse(request.Format.ToString() ?? string.Empty),
                },
                TopP = request.SettingTopP
            };
            var response = await _openAIService.SubmitAIResponsesAsync(modelAI);
            var statusAI = "completed";
            var result = response.Content?.Output?.FirstOrDefault(x => x.Status == statusAI)?.Content?.FirstOrDefault()?.Text;

            string requestLog = request.Serialize();
            string responseLog = "";
            try
            {
                responseLog = responseLog + " StatusCode: " + response.StatusCode.ToString();
                if (response.Content != null)
                {
                    responseLog = responseLog + " Content: " + response.Content.Serialize();
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
