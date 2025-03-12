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
    using Fsel.Storage.Domain.Models.CommandModels;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersions(ApiSettings.APIVersion1)]
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
        public async Task<IActionResult> UploadResolutions([FromBody] UrlResolutionRequestModel request)
        {
            var result = await _amazonS3Service.UploadResolutions(request?.BucketType, request?.Url ?? string.Empty);
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
        public async Task<IActionResult> UploadResolutions([FromQuery] EnumBucketType? bucketType, IFormFile file)
        {
            var result = await _amazonS3Service.UploadResolutions(bucketType, file);
            return result.GetActionResult();
        }
    }
}
