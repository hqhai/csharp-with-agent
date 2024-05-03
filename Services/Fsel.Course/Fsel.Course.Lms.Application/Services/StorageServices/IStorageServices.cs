// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.StorageServices
{
    using System.ComponentModel;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Refit;

    public interface IStorageServices
    {
        [Post("/v1/file/{folderType}")]
        Task<IActionResult> UploadFile([FromRoute] EnumFolderType folderType, [FromQuery] EnumBucketType? bucketType, IFormFile file, [FromQuery] bool isResize = false, [FromQuery] bool isValidEmpty = false);
    }

    public enum EnumBucketType
    {
        [Description("fsel")]
        Fsel,
        [Description("fsel-public")]
        FselPublic
    }


    public enum EnumFolderType
    {
        Files,
        Videos,
        Images,
        Questions,
        Fsis,
        AG,
    }
}
