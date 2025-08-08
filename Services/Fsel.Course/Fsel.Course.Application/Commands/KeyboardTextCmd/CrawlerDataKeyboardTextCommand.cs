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

    public class CrawlerDataKeyboardTextCommandHandler : IRequestHandler<CrawlerDataKeyboardTextCommand, MethodResult<bool>>
    {
        private readonly IStorageService _storageService;
        private readonly IKeyboardLayoutRepository _keyboardLayoutRepository;

        public CrawlerDataKeyboardTextCommandHandler(IStorageService storageService,
                                                     IKeyboardLayoutRepository keyboardLayoutRepository)
        {
            _storageService = storageService;
            _keyboardLayoutRepository = keyboardLayoutRepository;
        }

        public async Task<MethodResult<bool>> Handle(CrawlerDataKeyboardTextCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(request.LocationFile);
            ArgumentNullException.ThrowIfNull(request.KeyboardLayout);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            var checkNameKeyboardLayout = await _keyboardLayoutRepository.Queryable.AsNoTracking().AnyAsync(x => !string.IsNullOrEmpty(x.Name) && x.Name.Trim() == request.KeyboardLayout.Trim(), cancellationToken);
            if (checkNameKeyboardLayout)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(checkNameKeyboardLayout));
                return methodResult;
            }

            ConcurrentBag<KeyboardText> keyboardTexts = new ConcurrentBag<KeyboardText>();

            var svgFiles = Directory.GetFiles(request.LocationFile);

            var semaphore = new SemaphoreSlim(10);

            var tasks = svgFiles.Select(async filePath =>
            {
                await semaphore.WaitAsync();

                try
                {
                    var fileName = Path.GetFileName(filePath);
                    string name = fileName.Replace(".svg", "", StringComparison.OrdinalIgnoreCase);
                    var linkS3 = await UploadFileToS3(filePath, fileName);

                    if (linkS3 != null)
                    {
                        int codePoint = int.Parse(name, CultureInfo.InvariantCulture);
                        string text = char.ConvertFromUtf32(codePoint);

                        lock (keyboardTexts)
                        {
                            keyboardTexts.Add(new KeyboardText
                            {
                                Name = text,
                                Unicode = codePoint,
                                FilePath = linkS3
                            });
                        }
                    }
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
                await _keyboardLayoutRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.Result = true;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            });

            return methodResult;
        }

        private async Task<string?> UploadFileToS3(string filePath, string fileName)
        {
            using var stream = File.OpenRead(filePath);
            var streamPart = new StreamPart(stream, fileName);

            var filePartS3 = await _storageService.UpLoadFile(EnumFolderType.Images, EnumBucketType.FselPublic, streamPart, isAddSuffix: true);
            return filePartS3.Content?.Result;
        }
    }
}
