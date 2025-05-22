// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Storage.Api.Controllers
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Shared.Attributes;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Storage.Application.Services.AmazonS3Services;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersions(ApiSettings.APIVersion1)]
    [Route(Settings.APIDefaultRoute + "/file")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly IAmazonS3Service _amazonS3Service;

        public FileController(IAmazonS3Service amazonS3Service)
        {
            _amazonS3Service = amazonS3Service;
        }

        /// <summary>
        /// Upload file
        /// </summary>
        [DisableFormValueModelBinding]
        [DisableRequestSizeLimit]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = long.MaxValue)]
        [ProducesResponseType(typeof(MethodResult<string>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [HttpPost("{folderType}")]
        public async Task<IActionResult> Upload([FromRoute] EnumFolderType folderType, [FromQuery] EnumBucketType? bucketType, IFormFile file, [FromQuery] bool isResize = false, [FromQuery] bool isValidEmpty = false, [FromQuery] bool isAddSuffix = true)
        {
            var commandResult = await _amazonS3Service.UploadFileAsync(bucketType, file, folderType, isResize, isValidEmpty, isAddSuffix);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Upload file
        /// </summary>
        [DisableFormValueModelBinding]
        [DisableRequestSizeLimit]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = long.MaxValue)]
        [ProducesResponseType(typeof(MethodResult<string>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [HttpPost("multiple/{folderType}")]
        public async Task<IActionResult> Uploads([FromRoute] EnumFolderType folderType, [FromQuery] EnumBucketType? bucketType, IList<IFormFile> files, [FromQuery] bool isResize = false, [FromQuery] bool isValidEmpty = false, [FromQuery] bool isAddSuffix = true)
        {
            var commandResult = await _amazonS3Service.UploadFilesAsync(bucketType, files, folderType, isResize, isValidEmpty, isAddSuffix);
            return commandResult.GetActionResult();
        }
    }
}
