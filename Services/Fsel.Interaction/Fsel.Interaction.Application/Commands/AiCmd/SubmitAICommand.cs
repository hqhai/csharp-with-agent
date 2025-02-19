// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Commands.AiCmd
{
    using Fsel.Interaction.Application.Services.AIService;
    using Fsel.Interaction.Application.Services.AIService.Models;
    using MediatR;

    public class SubmitAICommand : IRequest<string?>
    {
        public string? SettingModel { get; set; }
        public string? SystemRoleAlConfig { get; set; }
        public string? UserAIConfig { get; set; }
    }

    public class SubmitAICommandHandler : IRequestHandler<SubmitAICommand, string?>
    {
        private readonly IOpenAIService _openAIService;
        private const string RoleSystem = "system";
        private const string RoleUser = "user";

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
                        Role =  RoleSystem,
                        Content =  request.SystemRoleAlConfig,
                    },
                    new
                    {
                        Role = RoleUser,
                        Content = request.UserAIConfig,
                    }
                }
            });

            var result = response.Content?.Choices?.Select(x => x.Message?.Content).FirstOrDefault();
            return result;
        }
    }
}
