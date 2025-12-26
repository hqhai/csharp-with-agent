// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Storage.Application.Command.SpeechToTextCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Storage.Application.Services.FFmpegServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Refit;

    public class CheckFileAndConvertCommand : IRequest<MethodResult<IFormFile>>
    {
        public IFormFile? FormFile { get; set; }
    }

    public class CheckFileAndConvertCommandHandler : IRequestHandler<CheckFileAndConvertCommand, MethodResult<IFormFile>>
    {
        private readonly IFFmpegServices _fFmpegServices;
        private readonly ILogger<CheckFileAndConvertCommand> _logger;
        private readonly IHttpClientFactory _httpClient;
        private List<string> _codecs = new List<string> { "WebM", "ADTS" };
        private List<string> _nameFiles = new List<string> { "AAC", "mp4", "WMA", "m4a" };
        //private List<(string, string)> _fileCodecs = new List<(string, string)> { ("WMA", "Windows media") };

        public CheckFileAndConvertCommandHandler(IFFmpegServices fFmpegServices,
                                                 ILogger<CheckFileAndConvertCommand> logger,
                                                 IHttpClientFactory httpClient)
        {
            _fFmpegServices = fFmpegServices;
            _logger = logger;
            _httpClient = httpClient;
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

            _logger.LogInformation($"LogInfoAudio:{checkFile}");

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

            // convert file ffmpeg
            var ffmpegConvert = await _fFmpegServices.Convert(filePart);
            if (!ffmpegConvert.IsSuccessStatusCode || string.IsNullOrEmpty(ffmpegConvert.Content?.Paths3))
            {
                methodResult.AddError(ffmpegConvert.Error);
                return methodResult;
            }

            _logger.LogInformation($"LogConvertAudio:{ffmpegConvert.Content}");

            // đọc dữ liệu từ link s3
            var httpClient = _httpClient.CreateClient();
            var response = await httpClient.GetAsync(ffmpegConvert.Content.Paths3);
            if (!response.IsSuccessStatusCode)
            {
                return methodResult;
            }
            var fileName = response.Content.Headers.ContentDisposition?.FileName?.Trim('"') ?? "audio.wav";
            var contentType = response.Content.Headers.ContentType?.ToString() ?? "audio/wav";
            var bytes = await response.Content.ReadAsByteArrayAsync();

            IFormFile formFileDefault = ConvertToIFormFile(bytes, fileName, contentType);

            methodResult.Result = formFileDefault;
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
