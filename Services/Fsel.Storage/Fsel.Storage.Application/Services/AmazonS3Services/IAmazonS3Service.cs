// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.ActionResults;
using Fsel.Storage.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace Fsel.Storage.Application.Services.AmazonS3Services
{
    public interface IAmazonS3Service
    {
        Task<MethodResult<string?>> UploadFileAsync(IFormFile file, EnumFolderType folderType, bool isResize = false);

        Task<MethodResult<IList<string>>> UploadFilesAsync(IList<IFormFile> files, EnumFolderType folderType, bool isResize = false);

        Task<MethodResult<string>> UploadResolutions(string? url);

        Task<MethodResult<string>> UploadResolutions(IFormFile? file);
    }
}
