// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Storage.Application.Command.SpeechToTextCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Shared.Enums;
    using Fsel.Storage.Application.Services.AmazonS3Services;
    using Fsel.Storage.Application.Services.OpenAIServices;
    using Fsel.Storage.Domain.Enums.ErrorCodes;
    using Fsel.Storage.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Refit;

    public class ConvertSpeechToTextCommand : IRequest<MethodResult<TranscriptFileModel>>
    {
        public IFormFile? FormFile { get; set; }
    }

    public class ConvertSpeechToTextCommandHandler : IRequestHandler<ConvertSpeechToTextCommand, MethodResult<TranscriptFileModel>>
    {
        private readonly IOpenAIService _openAIService;
        private readonly IAmazonS3Service _amazonS3Service;
        private const string AIModel = "whisper-1";

        public ConvertSpeechToTextCommandHandler(IOpenAIService openAIService, IAmazonS3Service amazonS3Service)
        {
            _openAIService = openAIService;
            _amazonS3Service = amazonS3Service;
        }

        public async Task<MethodResult<TranscriptFileModel>> Handle(ConvertSpeechToTextCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<TranscriptFileModel> methodResult = new MethodResult<TranscriptFileModel>();

            if (request.FormFile == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumFileErrorCode.FormFileNotNull), nameof(request.FormFile), nameof(request.FormFile));
                return methodResult;
            }

            var stream = request.FormFile.OpenReadStream();
            var streamPart = new StreamPart(stream, request.FormFile.FileName, request.FormFile.ContentType);

            var content = await _openAIService.SpeechToTextByAIAsync(streamPart, AIModel);
            if (!content.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(content.Error?.Message);
                return methodResult;
            }

            var fileInfomation = await _amazonS3Service.UploadFileAsync(EnumBucketType.FselPublic, request.FormFile, EnumFolderType.Videos, false, false);

            methodResult.Result = new TranscriptFileModel { FilePath = fileInfomation.Result, Content = content.Content };
            return methodResult;
        }
    }
}
