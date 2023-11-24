// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers
{
    using Fsel.Common.ActionResults;
    using System.Net;
    using Fsel.Common.Constants;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Queries.ClassForumQuery;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using Fsel.Course.Lms.Application.Queries.VideoTimeCodeResultQuery;
    using Fsel.Course.Domain.Models.QueryModels.VideoTimeCodeResults;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/video-time-code-result")]
    [ApiController]
    [Common.Attributes.Permission(role: nameof(EnumRole.Student))]
    public class VideoTimeCodeResultController : ControllerBase
    {
        private readonly IMediator _mediator;

        public VideoTimeCodeResultController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// get video time code result 
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<VideoTimeCodeResultByStudentModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get([FromQuery] GetVideoTimeCodeResultQuery command)
        {
            MethodResult<VideoTimeCodeResultByStudentModel> queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
