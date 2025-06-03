// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Storage.Application.Command.SpeechToTextCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Shared.Enums;
    using Fsel.Storage.Application.Services.AmazonS3Services;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class ConvertFileToWAVCommand : IRequest<MethodResult<string>>
    {
        public IFormFile? FormFile { get; set; }
    }

    public class ConvertFileToWAVCommandHandler : IRequestHandler<ConvertFileToWAVCommand, MethodResult<string>>
    {
        private readonly IMediator _mediator;
        private readonly IAmazonS3Service _amazonS3Service;

        public ConvertFileToWAVCommandHandler(IMediator mediator,
                                              IAmazonS3Service amazonS3Service)
        {
            _mediator = mediator;
            _amazonS3Service = amazonS3Service;
        }

        public async Task<MethodResult<string>> Handle(ConvertFileToWAVCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(request.FormFile);
            MethodResult<string> methodResult = new MethodResult<string>();

            var convertFile = await _mediator.Send(new CheckFileAndConvertCommand { FormFile = request.FormFile }, cancellationToken);
            if (!convertFile.IsOK || convertFile.Result == null)
            {
                methodResult.AddErrorBadRequest(convertFile.ErrorMessages.ToList());
                return methodResult;
            }

            var file = await UpLoadFileAsync(convertFile.Result);
            methodResult.Result = file.Result;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<MethodResult<string?>> UpLoadFileAsync(IFormFile formFile)
        {
            return await _amazonS3Service.UploadFileAsync(EnumBucketType.FselPublic, formFile, EnumFolderType.Videos, false, false);
        }
    }
}
