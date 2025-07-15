// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.StorageServices
{
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Lms.Application.Services.StorageServices.Models;
    using Fsel.Shared.Enums;
    using Microsoft.AspNetCore.Mvc;
    using Refit;

    public interface IStorageService
    {
        [Multipart]
        [Post("/v1/file/{folderType}")]
        Task<IApiResponse<MethodResult<string?>>> UpLoadFile([FromRoute] EnumFolderType folderType, [Query] EnumBucketType? bucketType, StreamPart file, [Query] bool isResize = false, [Query] bool isValidEmpty = false, [Query] bool isAddSuffix = true);

        [Multipart]
        [Post("/v1/transcript/convert-wav")]
        Task<IApiResponse<MethodResult<string?>>> ConvertWav([AliasAs("file")] StreamPart file);

        [Post("/v1/transcript/convert-speech-to-text")]
        Task<IApiResponse<MethodResult<string>>> ConvertSpeechToText(ConvertSpeechToTextModel command);


        [Post("/v1/transcript/speech-to-text-language")]
        Task<IApiResponse<MethodResult<string?>>> ConvertSpeechToTextSetLanguage(ConvertSpeechToTextSetLanguageModel model);
    }
}
