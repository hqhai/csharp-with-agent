// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Storage.Application.Command.SpeechToTextCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Storage.Application.Services.FFmpegServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Refit;

    public class CheckFileAndConvertCommand : IRequest<MethodResult<IFormFile>>
    {
        public IFormFile? FormFile { get; set; }
    }

    public class CheckFileAndConvertCommandHandler : IRequestHandler<CheckFileAndConvertCommand, MethodResult<IFormFile>>
    {
        private readonly IFFmpegServices _fFmpegServices;
        private List<string> _codecs = new List<string> { "WebM", "ADTS" };
        private List<string> _nameFiles = new List<string> { "AAC", "mp4", "WMA" };
        //private List<(string, string)> _fileCodecs = new List<(string, string)> { ("WMA", "Windows media") };

        public CheckFileAndConvertCommandHandler(IFFmpegServices fFmpegServices)
        {
            _fFmpegServices = fFmpegServices;
        }

        public async Task<MethodResult<IFormFile>> Handle(CheckFileAndConvertCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(request.FormFile);
            MethodResult<IFormFile> methodResult = new MethodResult<IFormFile>();

            await using var stream = request.FormFile.OpenReadStream();
            var filePart = new StreamPart(stream, request.FormFile.FileName, request.FormFile.ContentType);

            //check file
            var checkFileResult = await _fFmpegServices.GetInfoAudio(filePart);
            var checkFile = checkFileResult.Content;
            if (!checkFileResult.IsSuccessStatusCode || checkFile == null || checkFile.AudioInfo == null)
            {
                methodResult.AddError(checkFileResult.Error);
                return methodResult;
            }

            if (_codecs.Any(x => x.ToLower() == checkFile.AudioInfo.Codec.ToLower()) ||
                _nameFiles.Any(x => x.ToLower() == checkFile.Filename?.Substring(checkFile.Filename.LastIndexOf('.') + 1).ToLower()))
            {
                var convertWav = await ConvertWav(request.FormFile);
                if (!convertWav.IsOK)
                {
                    methodResult.AddErrorBadRequest(convertWav.ErrorMessages.ToList());
                    return methodResult;
                }

                request.FormFile = convertWav.Result;
            }

            methodResult.Result = request.FormFile;
            return methodResult;
        }

        private async Task<MethodResult<IFormFile>> ConvertWav(IFormFile originalFile)
        {
            MethodResult<IFormFile> methodResult = new MethodResult<IFormFile>();

            await using var stream = originalFile.OpenReadStream();
            var filePart = new StreamPart(stream, originalFile.FileName, originalFile.ContentType);

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

            methodResult.Result = formFile;
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
    }
}
