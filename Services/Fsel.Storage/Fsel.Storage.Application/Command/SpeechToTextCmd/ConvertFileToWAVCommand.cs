// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Storage.Application.Command.SpeechToTextCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Shared.Enums;
    using Fsel.Storage.Application.Services.AmazonS3Services;
    using Fsel.Storage.Application.Services.FFmpegServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Refit;

    public class ConvertFileToWAVCommand : IRequest<MethodResult<string>>
    {
        public IFormFile? FormFile { get; set; }
    }

    public class ConvertFileToWAVCommandHandler : IRequestHandler<ConvertFileToWAVCommand, MethodResult<string>>
    {
        private readonly IFFmpegServices _fFmpegServices;
        private readonly IAmazonS3Service _amazonS3Service;

        public ConvertFileToWAVCommandHandler(IFFmpegServices fFmpegServices,
                                              IAmazonS3Service amazonS3Service)
        {
            _fFmpegServices = fFmpegServices;
            _amazonS3Service = amazonS3Service;
        }

        public async Task<MethodResult<string>> Handle(ConvertFileToWAVCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(request.FormFile);
            MethodResult<string> methodResult = new MethodResult<string>();

            await using var stream = request.FormFile.OpenReadStream();
            var filePart = new StreamPart(stream, request.FormFile.FileName, request.FormFile.ContentType);

            var ffmpegConvert = await _fFmpegServices.Convert(filePart);
            if (!ffmpegConvert.IsSuccessStatusCode)
            {
                methodResult.AddError(ffmpegConvert.Error);
                return methodResult;
            }

            var jobConvertResult = await _fFmpegServices.Status(ffmpegConvert.Content?.JobId ?? Guid.Empty);
            if (!jobConvertResult.IsSuccessStatusCode)
            {
                methodResult.AddError(jobConvertResult.Error);
                return methodResult;
            }
            var jobConvert = jobConvertResult.Content;
            if (jobConvert == null || jobConvert.FileData == null || jobConvert.FileName == null || jobConvert.ContentType == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(jobConvert));
                return methodResult;
            }

            byte[] byteArray = Convert.FromBase64String(jobConvert.FileData);
            IFormFile formFile = ConvertToIFormFile(byteArray, jobConvert.FileName, jobConvert.ContentType);
            if (formFile == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(formFile));
                return methodResult;
            }

            var file = await UpLoadFileAsync(formFile);
            methodResult.Result = file.Result;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private static IFormFile ConvertToIFormFile(byte[] fileData, string fileName, string contentType)
        {
            var stream = new MemoryStream(fileData);
            return new FormFile(stream, 0, fileData.Length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = contentType
            };
        }

        private async Task<MethodResult<string?>> UpLoadFileAsync(IFormFile formFile)
        {
            return await _amazonS3Service.UploadFileAsync(EnumBucketType.FselPublic, formFile, EnumFolderType.Videos, false, false);
        }
    }
}
