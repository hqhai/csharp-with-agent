// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Storage.Application.Command.SpeechToTextCmd.V1i2
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Shared.Models.ShareModels;
    using Fsel.Storage.Application.Queues.Publisher;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class ConvertSpeechToTextCommand : IRequest<MethodResult<bool>>
    {
        public IFormFile? FormFile { get; set; }

        public string? CurrentDate { get; set; }
    }

    public class ConvertSpeechToTextCommandHandler : IRequestHandler<ConvertSpeechToTextCommand, MethodResult<bool>>
    {
        private readonly AuthContext _authContext;
        private readonly SpeechToTextAiPublisher _speechToTextAiPublisher;

        public ConvertSpeechToTextCommandHandler(AuthContext authContext, SpeechToTextAiPublisher speechToTextAiPublisher)
        {
            _authContext = authContext;
            _speechToTextAiPublisher = speechToTextAiPublisher;
        }

        public async Task<MethodResult<bool>> Handle(ConvertSpeechToTextCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(request.FormFile);
            var methodResult = new MethodResult<bool>();

            using var memoryStream = new MemoryStream();
            await request.FormFile.CopyToAsync(memoryStream, cancellationToken);

            var speechToTextAi = new SpeechToTextAiConsumerModel
            {
                UserId = _authContext.CurrentUserId,
                FileName = request.FormFile.FileName,
                ContentType = request.FormFile.ContentType,
                FileData = memoryStream.ToArray(),
                CurrentDate = request.CurrentDate
            };

            await _speechToTextAiPublisher.Publish(speechToTextAi, cancellationToken);

            methodResult.Result = true;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
