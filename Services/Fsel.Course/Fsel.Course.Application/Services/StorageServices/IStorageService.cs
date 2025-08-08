// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Services.StorageServices
{
    using Fsel.Common.ActionResults;
    using Fsel.Shared.Enums;
    using Microsoft.AspNetCore.Mvc;
    using Refit;

    public interface IStorageService
    {
        [Multipart]
        [Post("/v1/file/{folderType}")]
        Task<IApiResponse<MethodResult<string?>>> UpLoadFile([FromRoute] EnumFolderType folderType, [Query] EnumBucketType? bucketType, StreamPart file, [Query] bool isResize = false, [Query] bool isValidEmpty = false, [Query] bool isAddSuffix = true);

    }
}
