// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers.V1i1
{
    using System.Net;
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Commands.VideoTimeCodeResultCmd;
    using Fsel.Course.Lms.Application.Queries.VideoTimeCodeResultQuery;
    using Fsel.Shared.Attributes;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersions(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/video-time-code-result")]
    [ApiController]
   [Common.Attributes.Permission(roles: new string[] { nameof(EnumRole.Student), nameof(EnumRole.StudentCampus) })]
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
        [HttpGet("video-time-code-report")]
        [ProducesResponseType(typeof(MethodResult<TestResultReportModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetVideoTimeCodeResult([FromQuery] GetVideoTimeCodeReportQuery command)
        {
            var queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// get video time code ranking
        /// </summary>
        [HttpGet("video-time-code-ranking")]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<TestResultRankingModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get([FromQuery] GetVideoTimeCodeRankingQuery command)
        {
            var queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// get video time code ranking
        /// </summary>
        [HttpGet("video-time-code-test")]
        [ProducesResponseType(typeof(MethodResult<IList<VideoTimeCodeModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetVideoTimeCodeTest([FromQuery] GetVideoTimeCodeTestQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Action Video Time Code
        /// </summary>
        [HttpPost("action-time-code")]
        [ProducesResponseType(typeof(MethodResult<VideoResultModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [MapToApiVersion(ApiSettings.APIVersion1i1)]
        public async Task<IActionResult> ActionTimeCode([FromBody] ActionVideoTimeCodeCommand command)
        {
            var queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
