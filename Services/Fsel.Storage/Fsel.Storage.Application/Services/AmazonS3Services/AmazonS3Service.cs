// Copyright (c) Atlantic. All rights reserved.

using Amazon.S3.Transfer;
using Amazon.S3;
using Microsoft.AspNetCore.Http;
using Fsel.Storage.Infrastructure.ValueSettings;
using Fsel.Storage.Domain.Enums;
using Fsel.Common.ActionResults;
using Fsel.Common.Helpers;
using Amazon.S3.Model;

namespace Fsel.Storage.Application.Services.AmazonS3Services
{
    public class AmazonS3Service : IAmazonS3Service, IDisposable
    {
        private readonly AppSetting _appSetting;
        private readonly AmazonS3Client _amazonS3Client;
        private readonly TransferUtility _transferUtility;

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

        public async Task<MethodResult<string?>> UploadFileAsync(IFormFile file, EnumFolderType folderType)
        {
            MethodResult<string?> result = new MethodResult<string?>();
            try
            {
                if (file == null || !_appSetting.StorageConfig!.IsValid())
                {
                    result.StatusCode = StatusCodes.Status400BadRequest;
                    return result;
                }

                var folder = _appSetting.StorageConfig.Folders!.GetPropValue<string>(folderType.ToString());
                var key = PathHelper.Combine(folder, file.FileName.ReplaceSpecialChars().AddSuffix());

                var initiateRequest = new InitiateMultipartUploadRequest
                {
                    BucketName = _appSetting.StorageConfig!.BucketName,
                    Key = key,
                };
                var initiateResponse = await _amazonS3Client.InitiateMultipartUploadAsync(initiateRequest);

                // Calculate the size of each part
                var partSize = 100 * 1024 * 1024; // Size of each part (100 MB)

                // Create a list of parts to upload
                var parts = new List<UploadPartResponse>();

                using (var sourceStream = file.OpenReadStream())
                {
                    var buffer = new byte[partSize];
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
            catch (Exception ex)
            {
                result.AddErrorServer();
                return result;
            }
        }

        public static string? GenerateAwsFileUrl(string? bucketName, string? baseUrl, string? fileName)
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
