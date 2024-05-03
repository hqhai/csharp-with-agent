// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.StudentChatbotCmd
{
    using System.Threading;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Lms.Application.Services.AiService;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class CreateChatBotCommand : IRequest<MethodResult<string>>
    {
       
    }

    public class CreateChatbotAudioCommandHandler : IRequestHandler<CreateChatBotCommand, MethodResult<string>>
    {
        private readonly IOpenAIService _openAIService;


        public CreateChatbotAudioCommandHandler(IOpenAIService openAIService)
        {
            _openAIService = openAIService;
        }

        public async Task<MethodResult<string>> Handle(CreateChatBotCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<string>();

            var response = await _openAIService.GenerateAudioByAIAsync(new AudioChatbotModel
            {
                Model = request.Model,
                Input = request.Text,
                Voice = request.Voice
            });

            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

     
    }
}
