// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Storage.Api.Controllers
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Shared.Attributes;
    using Fsel.Storage.Application.Services.AmazonS3Services;
    using Fsel.Storage.Domain.Enums;
    using Microsoft.AspNetCore.Mvc;
    using Asp.Versioning;
    using Fsel.Shared.Constants;

    [ApiVersion(ApiSettings.APIVersion1)][ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/video")]
    [ApiController]
    public class VideoController : ControllerBase
    {
        private readonly IAmazonS3Service _amazonS3Service;

        public VideoController(IAmazonS3Service amazonS3Service)
        {
            _amazonS3Service = amazonS3Service;
        }

        /// <summary>
        /// Get Transcription
        /// </summary>
        [HttpPost("url-resolutions")]
        [ProducesResponseType(typeof(MethodResult<string>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UploadResolutions(string url)
        {
            var result = await _amazonS3Service.UploadResolutions(url);
            return result.GetActionResult();
        }

        /// <summary>
        /// Get Transcription
        /// </summary>
        [HttpPost("file-resolutions")]
        [DisableFormValueModelBinding]
        [DisableRequestSizeLimit]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = long.MaxValue)]
        [ProducesResponseType(typeof(MethodResult<string>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UploadResolutions(IFormFile file)
        {
            var result = await _amazonS3Service.UploadResolutions(file);
            return result.GetActionResult();
        }
    }
}
