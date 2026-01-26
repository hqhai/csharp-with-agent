// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Realtime.Infrastructure.Services
{
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Shared.Enums;
    using Microsoft.AspNetCore.Mvc;
    using Refit;

    /// <summary>
    /// Refit interface for Course Storage API
    /// </summary>
    public interface IStorageService
    {
        /// <summary>
        /// Upload a file to S3 storage
        /// </summary>
        [Multipart]
        [Post("/v1/file/{folderType}")]
        Task<IApiResponse<MethodResult<string?>>> UploadFile(
            [FromRoute] EnumFolderType folderType,
            [Query] EnumBucketType? bucketType,
            [AliasAs("file")] StreamPart file,
            [Query] bool isResize = false,
            [Query] bool isValidEmpty = false,
            [Query] bool isAddSuffix = true);
    }
}
