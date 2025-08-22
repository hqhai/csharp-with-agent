// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.Chatbots
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.System.Application.Services.FFmpegServices;
    using global::System;
    using global::System.Net.Http;
    using global::System.Net.Http.Headers;
    using global::System.Threading.Tasks;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Microsoft.Net.Http.Headers;
    using Refit;

    public class ConvertFileWavCommand : IRequest<MethodResult<string>>
    {
        public string? File { get; set; }
    }

    public class ConvertFileWavCommandHandler : IRequestHandler<ConvertFileWavCommand, MethodResult<string>>
    {
        private readonly ILogger<ConvertFileWavCommandHandler> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IFFmpegServices _fFmpegServices;

        public ConvertFileWavCommandHandler(
            ILogger<ConvertFileWavCommandHandler> logger,
            IHttpClientFactory httpClientFactory,
            IFFmpegServices fFmpegServices)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _fFmpegServices = fFmpegServices;
        }

        public async Task<MethodResult<string>> Handle(ConvertFileWavCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<string>();
            var result = new MethodResult<string>();
            request.File ??= string.Empty;
            if (string.IsNullOrWhiteSpace(request.File))
            {
                result.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required), nameof(request.File));
                return methodResult;
            }

            try
            {
                var formFile = await DownloadAsIFormFileAsync(request.File, cancellationToken);
                var streamPart = ToStreamPart(formFile, forceWav: false);
                var wavUrl = await ConvertAndUploadAsync(streamPart, cancellationToken);

                result.Result = wavUrl;
                return methodResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Convert WAV pipeline failed for {Url}", request.File);
                result.AddErrorBadRequest(ex.Message);
                return methodResult;
            }
        }

        /// <summary>
        /// Calls ffmpeg service to convert; streams from returned S3 URL straight into storage upload.
        /// </summary>
        private async Task<string> ConvertAndUploadAsync(StreamPart sourceFile, CancellationToken ct)
        {
            // 1) Convert via ffmpeg service
            var ffmpegConvert = await _fFmpegServices.Convert(sourceFile);
            if (!ffmpegConvert.IsSuccessStatusCode || string.IsNullOrEmpty(ffmpegConvert.Content?.Paths3))
            {
                throw new InvalidOperationException("FFmpeg convert failed.");
            }
            return ffmpegConvert.Content!.Paths3;
        }

        private static string ResolveFileName(HttpContentHeaders headers, string urlFallback)
        {
            var cd = headers.ContentDisposition;
            var name = cd?.FileNameStar ?? cd?.FileName;
            name = name?.Trim('\"');

            if (!string.IsNullOrWhiteSpace(name))
            {
                return name;
            }
            try
            {
                var uri = new Uri(urlFallback, UriKind.Absolute);
                var fromPath = Path.GetFileName(uri.LocalPath);
                if (!string.IsNullOrWhiteSpace(fromPath))
                {
                    return fromPath;
                }
            }
            catch
            {
                // ignore
            }

            return "downloaded.bin";
        }

        public static StreamPart ToStreamPart(IFormFile file, bool forceWav = false)
        {
            if (file is null || file.Length == 0)
            {
                throw new ArgumentException("File is required.", nameof(file));
            }
            var stream = file.OpenReadStream(); // disposed by Refit after request
            var fileName = string.IsNullOrWhiteSpace(file.FileName) ? "upload.bin" : file.FileName;
            if (forceWav)
            {
                fileName = Path.ChangeExtension(fileName, ".wav");
            }
            var contentType = string.IsNullOrWhiteSpace(file.ContentType)
                ? (forceWav ? "audio/wav" : "application/octet-stream")
                : file.ContentType;

            return new StreamPart(stream, fileName, contentType);
        }

        public async Task<IFormFile> DownloadAsIFormFileAsync(string url, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentException("Url is required.", nameof(url));
            }
            var client = _httpClientFactory.CreateClient();
            using var resp = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct);
            resp.EnsureSuccessStatusCode();

            var fileName = ResolveFileName(resp.Content.Headers, url);
            var contentType = resp.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";

            // Disk-backed to avoid large in-memory buffers
            var tempPath = Path.Combine(Path.GetTempPath(), $"dl_{Guid.NewGuid():N}{Path.GetExtension(fileName)}");
            await using (var fs = new FileStream(
                tempPath,
                FileMode.Create,
                FileAccess.ReadWrite,
                FileShare.Read,
                64 * 1024,
                FileOptions.Asynchronous))
            {
                await using var net = await resp.Content.ReadAsStreamAsync(ct);
                await net.CopyToAsync(fs, ct);
            }

            var ms = new MemoryStream(await File.ReadAllBytesAsync(tempPath, ct)); // if you MUST return IFormFile
            ms.Position = 0;

            var formFile = new FormFile(ms, 0, ms.Length, "file", fileName)
            {
                Headers = new HeaderDictionary()
            };
            formFile.Headers[HeaderNames.ContentType] = contentType;
            return formFile;
        }
    }
}
