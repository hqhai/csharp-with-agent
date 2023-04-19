// Copyright (c) Atlantic. All rights reserved.

using Amazon.S3.Transfer;
using Amazon.S3;
using Microsoft.AspNetCore.Http;
using Fsel.Storage.Infrastructure.ValueSettings;
using Fsel.Shared.Helpers;
using Fsel.Storage.Domain.Enums;
using Fsel.Common.ActionResults;
using Fsel.Common.Helpers;

namespace Fsel.Storage.Application.Services.AmazonS3Services
{
    public class AmazonS3Service : IAmazonS3Service
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
                var key = PathHelper.Combine(folder, file.FileName.AddSuffix());

                using (var newMemoryStream = new MemoryStream())
                {
                    file.CopyTo(newMemoryStream);

                    var uploadRequest = new TransferUtilityUploadRequest
                    {
                        InputStream = newMemoryStream,
                        Key = key,
                        BucketName = _appSetting.StorageConfig!.BucketName,
                        ContentType = file.ContentType,
                    };

                    await _transferUtility.UploadAsync(uploadRequest);

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

        public static string? GenerateAwsFileUrl(string? bucketName, string? baseUrl, string? fileName)
        {
            var url = $"{bucketName}.{baseUrl}/{fileName}";
            return PathHelper.AddScheme(url);
        }
    }
}
