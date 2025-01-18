// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers.Admin
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Course.Lms.Application.Queries.OtherFeatureQuery;
    using Fsel.Shared.Attributes;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersions(ApiSettings.APIVersion1)]
    [Route(Settings.APIDefaultRoute + "/admin/other")]
    [ApiController]
    [Common.Attributes.Permission(role: nameof(EnumRole.Admin))]
    public class OtherController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OtherController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get Param BeginnerGuide
        /// </summary>
        [HttpPost("param-beginner-guide")]
        [ProducesResponseType(typeof(MethodResult<IList<ParamBeginnerGuideModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetParamBeginnerGuide([FromBody] IList<Guid> studentIds)
        {
            MethodResult<IList<ParamBeginnerGuideModel>> queryResult = await _mediator.Send(new GetParamBeginnerGuideQuery { ListStudentIds = studentIds }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
