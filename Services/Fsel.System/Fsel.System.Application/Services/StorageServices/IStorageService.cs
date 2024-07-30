// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Services.StorageServices
{
    using Fsel.Common.ActionResults;
    using Fsel.Shared.Enums;
    using Fsel.System.Application.Services.StorageServices.Models;
    using Microsoft.AspNetCore.Mvc;
    using Refit;

    public interface IStorageService
    {
        [Post("/v1/transcript/text-to-speech")]
        Task<IApiResponse<MethodResult<string>>> TextToSpeech([Body] CreateChatbotAudioModel command);

        [Multipart]
        [Post("/v1/file/{folderType}")]
        Task<IApiResponse<MethodResult<string?>>> UpLoadFile([FromRoute] EnumFolderType folderType, [Query] EnumBucketType? bucketType, StreamPart file, [Query] bool isResize = false, [FromQuery] bool isValidEmpty = false);
    }
}
