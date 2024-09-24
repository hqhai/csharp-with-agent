// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.ActionResults;
using Fsel.Shared.Enums;
using Microsoft.AspNetCore.Http;

namespace Fsel.Storage.Application.Services.AmazonS3Services
{
    public interface IAmazonS3Service
    {
        Task<MethodResult<string?>> UploadFileAsync(EnumBucketType? bucketType, IFormFile file, EnumFolderType folderType, bool isResize = false, bool isValidEmpty = false);

        Task<MethodResult<IList<string>>> UploadFilesAsync(EnumBucketType? bucketType, IList<IFormFile> files, EnumFolderType folderType, bool isResize = false, bool isValidEmpty = false);

        Task<MethodResult<string>> UploadResolutions(EnumBucketType? bucketType, string? url);

        Task<MethodResult<string>> UploadResolutions(EnumBucketType? bucketType, IFormFile? file);
    }
}
