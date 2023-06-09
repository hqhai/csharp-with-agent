// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Storage.Api.Controllers
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using System.Net;
    using Microsoft.AspNetCore.Mvc;
    using Fsel.Storage.Application.Services.AmazonS3Services;
    using Fsel.Storage.Domain.Enums;
    using Fsel.Shared.Attributes;

    [ApiVersion(Settings.APIVersion)]
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
        [HttpPost("{type}")]
        public async Task<IActionResult> Upload([FromRoute] EnumFolderType type, IFormFile file)
        {
            var commandResult = await _amazonS3Service.UploadFileAsync(file, type);
            return commandResult.GetActionResult();
        }
    }
}
