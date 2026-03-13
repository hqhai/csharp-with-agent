// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.KeyboardTextCmd
{
    using System.Collections.Concurrent;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Application.Services.StorageServices;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Refit;

    public class CrawlerDataKeyboardTextFileCommand : IRequest<MethodResult<bool>>
    {
        public string? LocationFile { get; set; }
        public string? KeyboardLayout { get; set; }
        public Guid LanguageId { get; set; }
    }

    public class CrawlerDataKeyboardTextFileCommandHandler
        : IRequestHandler<CrawlerDataKeyboardTextFileCommand, MethodResult<bool>>
    {
        private readonly IStorageService _storageService;
        private readonly IKeyboardLayoutRepository _keyboardLayoutRepository;

        public CrawlerDataKeyboardTextFileCommandHandler(
            IStorageService storageService,
            IKeyboardLayoutRepository keyboardLayoutRepository)
        {
            _storageService = storageService;
            _keyboardLayoutRepository = keyboardLayoutRepository;
        }

        public async Task<MethodResult<bool>> Handle(
            CrawlerDataKeyboardTextFileCommand request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(request.LocationFile);
            ArgumentNullException.ThrowIfNull(request.KeyboardLayout);

            var methodResult = new MethodResult<bool>();

            var isExisted = await _keyboardLayoutRepository.Queryable
                .AsNoTracking()
                .AnyAsync(
                    x => !string.IsNullOrEmpty(x.Name)
                         && x.Name.Trim() == request.KeyboardLayout.Trim(),
                    cancellationToken);

            if (isExisted)
            {
                methodResult.AddErrorBadRequest(
                    nameof(EnumSystemErrorCode.DataAlreadyExist),
                    nameof(request.KeyboardLayout),
                    "Keyboard layout name is already existed.");
                return methodResult;
            }

            if (!Directory.Exists(request.LocationFile))
            {
                methodResult.AddErrorBadRequest(
                    nameof(EnumSystemErrorCode.DataNotExist),
                    nameof(request.LocationFile),
                    "Folder location does not exist.");
                return methodResult;
            }

            var keyboardTexts = new ConcurrentBag<KeyboardText>();

            var svgFiles = Directory.GetFiles(request.LocationFile, "*.json", SearchOption.TopDirectoryOnly);

            var semaphore = new SemaphoreSlim(10);

            var tasks = svgFiles.Select(async filePath =>
            {
                await semaphore.WaitAsync(cancellationToken);

                try
                {
                    if (!TryParseKeyboardFromFileName(filePath, out var codePoint, out var text, out var romanization))
                    {
                        return;
                    }

                    var fileName = Path.GetFileName(filePath);
                    var linkS3 = await UploadFileToS3(filePath, fileName);

                    if (string.IsNullOrWhiteSpace(linkS3))
                    {
                        return;
                    }

                    keyboardTexts.Add(new KeyboardText
                    {
                        Name = text,
                        Unicode = codePoint,
                        FilePath = linkS3,
                        VectorLibrary = EnumVectorLibrary.Hangul
                    });
                }
                finally
                {
                    semaphore.Release();
                }
            });

            await Task.WhenAll(tasks);

            await _keyboardLayoutRepository.ExecuteTransactionAsync(async () =>
            {
                var newKeyboardLayout = new KeyboardLayout
                {
                    Name = request.KeyboardLayout,
                    KeyboardTexts = keyboardTexts.ToList(),
                    LanguageId = request.LanguageId
                };

                _keyboardLayoutRepository.Add(newKeyboardLayout);

                await _keyboardLayoutRepository.UnitOfWork
                    .SaveChangesAsync(cancellationToken)
                    .ConfigureAwait(false);

                methodResult.Result = true;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            });

            return methodResult;
        }

        private static bool TryParseKeyboardFromFileName(
            string filePath,
            out int codePoint,
            out string text,
            out string romanization)
        {
            codePoint = default;
            text = string.Empty;
            romanization = string.Empty;

            var fileName = Path.GetFileName(filePath);
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return false;
            }

            // Bỏ extension .svg
            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
            if (string.IsNullOrWhiteSpace(fileNameWithoutExt))
            {
                return false;
            }

            const string strokesSuffix = ".strokes";
            if (fileNameWithoutExt.EndsWith(strokesSuffix, StringComparison.OrdinalIgnoreCase))
            {
                fileNameWithoutExt = fileNameWithoutExt[..^strokesSuffix.Length];
            }

            var parts = fileNameWithoutExt.Split('_', StringSplitOptions.RemoveEmptyEntries);

            // Format tối thiểu:
            // 3149_ㅉ_ssang_jieut
            if (parts.Length < 3)
            {
                return false;
            }

            var unicodePart = parts[0];
            var characterPart = parts[1];
            romanization = string.Join("_", parts.Skip(2));

            if (!int.TryParse(
                    unicodePart,
                    NumberStyles.HexNumber,
                    CultureInfo.InvariantCulture,
                    out codePoint))
            {
                return false;
            }

            try
            {
                var expectedChar = char.ConvertFromUtf32(codePoint);

                // Kiểm tra character trong filename có khớp unicode không
                if (!string.Equals(expectedChar, characterPart, StringComparison.Ordinal))
                {
                    return false;
                }

                text = characterPart;
                return true;
            }
            catch (ArgumentOutOfRangeException)
            {
                return false;
            }
        }

        private async Task<string?> UploadFileToS3(string filePath, string fileName)
        {
            using var stream = File.OpenRead(filePath);
            var streamPart = new StreamPart(stream, fileName);

            var filePartS3 = await _storageService.UpLoadFile(
                EnumFolderType.Files,
                EnumBucketType.Fsel,
                streamPart,
                isAddSuffix: true);

            return filePartS3.Content?.Result;
        }
    }
}
