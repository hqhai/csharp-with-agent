// Copyright (c) Atlantic. All rights reserved.

using System.Drawing;
using System.Drawing.Drawing2D;
using System.Net.Mime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Fsel.Common.ActionResults;
using Fsel.Common.Helpers;
using Fsel.Storage.Domain.Enums;
using Fsel.Storage.Domain.Enums.ErrorCodes;
using Fsel.Storage.Infrastructure.ValueSettings;
using Humanizer.Bytes;
using Microsoft.AspNetCore.Http;

namespace Fsel.Storage.Application.Services.AmazonS3Services
{
    public class AmazonS3Service : IAmazonS3Service, IDisposable
    {
        private readonly AppSetting _appSetting;
        private readonly AmazonS3Client _amazonS3Client;
        private readonly TransferUtility _transferUtility;
        private readonly int _targetWidthResize = 84;
        private readonly int _targetHeightResize = 84;
        private readonly double _partSize = ByteSize.FromMegabytes(100).Bytes; // Size of each part (100 MB)

        private readonly Dictionary<EnumFolderType, double> _maximumCapacity = new Dictionary<EnumFolderType, double>
        {
            { EnumFolderType.Fsis, ByteSize.FromGigabytes(1).Bytes }, //maximum question size (1 GB)
            { EnumFolderType.Videos, ByteSize.FromGigabytes(5).Bytes }, //maximum video size (5 GB)
            { EnumFolderType.Files, ByteSize.FromMegabytes(6).Bytes }, //maximum file size (6 MB)
            { EnumFolderType.Questions, ByteSize.FromMegabytes(6).Bytes }, //maximum question size (6 MB)
            { EnumFolderType.Images, ByteSize.FromMegabytes(20).Bytes } //maximum image size (20 MB)
        };

        public AmazonS3Service(AppSetting appSetting)
        {
            _appSetting = appSetting;

            ArgumentNullException.ThrowIfNull(_appSetting);
            ArgumentNullException.ThrowIfNull(_appSetting.StorageConfig);
            ArgumentNullException.ThrowIfNull(_appSetting.StorageConfig.Folders);
            _amazonS3Client = new AmazonS3Client(_appSetting.StorageConfig.AwsAccessKey, _appSetting.StorageConfig.AwsSecretAccessKey, new AmazonS3Config
            {
                ServiceURL = PathHelper.AddScheme(_appSetting.StorageConfig.AwsS3BaseUrl!),
            });
            _transferUtility = new TransferUtility(_amazonS3Client);
        }

        public async Task<MethodResult<string?>> UploadFileAsync(IFormFile? file, EnumFolderType folderType, bool isResize = false)
        {
            MethodResult<string?> result = IsValidFile(file, folderType);
            try
            {
                if (file == null || !result.IsOK)
                {
                    return result;
                }

                var folder = _appSetting.StorageConfig!.Folders!.GetPropValue<string>(folderType.ToString());
                var key = PathHelper.Combine(folder, file.FileName.ReplaceSpecialChars().AddSuffix());

                var initiateRequest = new InitiateMultipartUploadRequest
                {
                    BucketName = _appSetting.StorageConfig!.BucketName,
                    Key = key,
                };
                var initiateResponse = await _amazonS3Client.InitiateMultipartUploadAsync(initiateRequest);

                // Calculate the size of each part
                var partSize = _partSize;

                // Create a list of parts to upload
                var parts = new List<UploadPartResponse>();
                Stream stream;
                if (isResize)
                {
                    stream = OpenReadStreamResize(file);
                }
                else
                {
                    stream = file.OpenReadStream();
                }

                using (var sourceStream = stream)
                {
                    var buffer = new byte[(long)partSize];
                    int bytesRead;
                    int partNumber = 1;

                    var partUploadTasks = new List<Task>();

                    while ((bytesRead = await sourceStream.ReadAsync(buffer.AsMemory(0, buffer.Length))) > 0)
                    {
                        var partBuffer = new byte[bytesRead];
                        Array.Copy(buffer, 0, partBuffer, 0, bytesRead);

                        var uploadPartRequest = new UploadPartRequest
                        {
                            BucketName = _appSetting.StorageConfig!.BucketName,
                            Key = key,
                            UploadId = initiateResponse.UploadId,
                            PartNumber = partNumber,
                            PartSize = bytesRead,
                            InputStream = new MemoryStream(partBuffer)
                        };

                        partUploadTasks.Add(Task.Run(async () =>
                        {
                            var uploadPartResponse = await _amazonS3Client.UploadPartAsync(uploadPartRequest);
                            parts.Add(uploadPartResponse);
                        }));

                        partNumber++;
                    }

                    await Task.WhenAll(partUploadTasks).ConfigureAwait(false);

                    // Complete the upload process
                    var completeMultipartUploadRequest = new CompleteMultipartUploadRequest
                    {
                        BucketName = _appSetting.StorageConfig!.BucketName,
                        Key = key,
                        UploadId = initiateResponse.UploadId,
                        PartETags = parts.Select(p => new PartETag { PartNumber = p.PartNumber, ETag = p.ETag }).ToList()
                    };

                    var complete = await _amazonS3Client.CompleteMultipartUploadAsync(completeMultipartUploadRequest);
                    if (complete.HttpStatusCode != System.Net.HttpStatusCode.OK)
                    {
                        result.AddErrorServer();
                        return result;
                    }

                    result.Result = GenerateAwsFileUrl(_appSetting.StorageConfig.BucketName, _appSetting.StorageConfig.AwsS3BaseUrl, key);
                    return result;
                }
            }
            catch (Exception)
            {
                result.AddErrorServer();
                return result;
            }
        }

        private Stream OpenReadStreamResize(IFormFile file)
        {
            var stream = file.OpenReadStream();

            if (file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                using (Image sourceImage = Image.FromStream(stream))
                {
                    // Calculate the new size based on the target width and height
                    var newWidth = _targetWidthResize;
                    var newHeight = _targetHeightResize;

                    // Create a new image with resized size
                    using (var resizedImage = new Bitmap(newWidth, newHeight))
                    using (Graphics graphics = Graphics.FromImage(resizedImage))
                    {
                        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        graphics.SmoothingMode = SmoothingMode.HighQuality;
                        graphics.CompositingQuality = CompositingQuality.HighQuality;
                        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                        graphics.DrawImage(sourceImage, 0, 0, newWidth, newHeight);

                        // Create a memory stream for the resized image
                        var resizedStream = new MemoryStream();
                        resizedImage.Save(resizedStream, sourceImage.RawFormat);
                        resizedStream.Seek(0, SeekOrigin.Begin); // Move the stream cursor to the beginning
                        return resizedStream;
                    }
                }
            }

            return stream;
        }

        private MethodResult<string?> IsValidFile(IFormFile? file, EnumFolderType folderType)
        {
            var result = new MethodResult<string?>();
            if (file == null || !_appSetting.StorageConfig!.IsValid())
            {
                result.AddErrorBadRequest(nameof(EnumFileErrorCode.FileCannotEmpty), nameof(file));
                return result;
            }

            var capacity = _maximumCapacity.FirstOrDefault(x => x.Key == folderType).Value;
            if (file.Length > capacity)
            {
                result.AddErrorBadRequest(nameof(EnumFileErrorCode.FileIsLargerThanAllowedSize), nameof(file), file.Length);
                return result;
            }

            return result;
        }

        private static string? GenerateAwsFileUrl(string? bucketName, string? baseUrl, string? fileName)
        {
            var url = $"{bucketName}.{baseUrl}/{fileName}";
            return PathHelper.AddScheme(url);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _amazonS3Client.Dispose();
                _transferUtility.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
