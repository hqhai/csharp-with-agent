// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers
{
    using System.Net;
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Commands.OtherCmd;
    using Fsel.Course.Lms.Application.Queries.OtherFeatureQuery;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/other")]
    [ApiController]
    public class OtherController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OtherController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>s
        /// Get Feature Module
        /// </summary>
        [HttpGet("feature-module")]
        [ProducesResponseType(typeof(MethodResult<FeatureModuleModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetFeatureModule([FromQuery] GetFeatureModuleQuery query)
        {
            MethodResult<FeatureModuleModel> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>s
        /// send notify after pt
        /// </summary>
        [HttpGet("send-notify-after-pt")]
        [ProducesResponseType(typeof(MethodResult<FeatureModuleModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SendNotifyAfterPT([FromQuery] PushNoticeCommand command)
        {
            var queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
