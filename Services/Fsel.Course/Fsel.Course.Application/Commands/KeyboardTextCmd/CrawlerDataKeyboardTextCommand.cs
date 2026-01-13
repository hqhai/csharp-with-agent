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

    public class CrawlerDataKeyboardTextCommand : IRequest<MethodResult<bool>>
    {
        public string? LocationFile { get; set; }

        public string? KeyboardLayout { get; set; }

        public Guid LanguageId { get; set; }
    }

    public class CrawlerDataKeyboardTextCommandHandler
        : IRequestHandler<CrawlerDataKeyboardTextCommand, MethodResult<bool>>
    {
        private readonly IStorageService _storageService;
        private readonly IKeyboardLayoutRepository _keyboardLayoutRepository;

        public CrawlerDataKeyboardTextCommandHandler(
            IStorageService storageService,
            IKeyboardLayoutRepository keyboardLayoutRepository)
        {
            _storageService = storageService;
            _keyboardLayoutRepository = keyboardLayoutRepository;
        }

        public async Task<MethodResult<bool>> Handle(
            CrawlerDataKeyboardTextCommand request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(request.LocationFile);
            ArgumentNullException.ThrowIfNull(request.KeyboardLayout);

            var methodResult = new MethodResult<bool>();

            // Check trùng tên layout
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

            // Chỉ lấy file svg
            var svgFiles = Directory.GetFiles(request.LocationFile, "*.svg");

            var semaphore = new SemaphoreSlim(10);

            var tasks = svgFiles.Select(async filePath =>
            {
                await semaphore.WaitAsync(cancellationToken);

                try
                {
                    // Dùng hàm helper để lấy codePoint + text từ tên file
                    if (!TryParseKeyboardFromFileName(filePath, out var codePoint, out var text, out var type))
                    {
                        // Lưu lại file không parse được để debug
                        return;
                    }

                    var fileName = Path.GetFileName(filePath);
                    var linkS3 = await UploadFileToS3(filePath, fileName);

                    if (string.IsNullOrEmpty(linkS3))
                    {
                        return;
                    }

                    keyboardTexts.Add(new KeyboardText
                    {
                        Name = text,
                        Unicode = codePoint,
                        FilePath = linkS3,
                        VectorLibrary = EnumVectorLibrary.Kvg
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
        out string? style)
        {
            codePoint = default;
            text = string.Empty;
            style = null;

            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(filePath);
            if (string.IsNullOrWhiteSpace(fileNameWithoutExt))
            {
                return false;
            }

            // "0004c"  -> ["0004c"]
            // "05224-Kaisho" -> ["05224", "Kaisho"]
            var parts = fileNameWithoutExt.Split('-', 2, StringSplitOptions.RemoveEmptyEntries);
            var codeToken = parts[0];
            style = parts.Length > 1 ? parts[1] : null;

            // Parse mã Unicode dạng HEX
            if (!int.TryParse(
                    codeToken,
                    NumberStyles.HexNumber,
                    CultureInfo.InvariantCulture,
                    out codePoint))
            {
                return false;
            }

            try
            {
                text = char.ConvertFromUtf32(codePoint);
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
                EnumFolderType.Images,
                EnumBucketType.Fsel,
                streamPart,
                isAddSuffix: true);

            return filePartS3.Content?.Result;
        }
    }
}
