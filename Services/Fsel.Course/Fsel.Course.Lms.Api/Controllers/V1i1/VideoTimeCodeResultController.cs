// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers.V1i1
{
    using System.Net;
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Queries.VideoTimeCodeResultQuery;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1i1)]
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
        /// get video time code ranking
        /// </summary>
        [HttpGet("video-time-code-ranking-report")]
        [ProducesResponseType(typeof(MethodResult<TestResultRankingModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetVideoTimeCodeResult([FromQuery] GetVideoTimeCodeResultReportQuery command)
        {
            var queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// get video time code ranking
        /// </summary>
        [HttpGet("video-time-code-ranking")]
        [ProducesResponseType(typeof(MethodResult<IList<TestResultRankingModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get([FromQuery] GetVideoTimeCodeRankingQuery command)
        {
            var queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
