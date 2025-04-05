// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Api.Controllers
{
    using System.Net;
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Identity.Application.Queries.UserCourseSettingQuery;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Shared.Constants;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/user-course-setting")]
    [ApiController]
    public class UserCourseSettingController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UserCourseSettingController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get User Course settings
        /// </summary>
        [HttpGet("{userId}")]
        [ProducesResponseType(typeof(MethodResult<IList<UserCourseSettingModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetUserCourseSettings([FromRoute] Guid? userId)
        {
            MethodResult<IList<UserCourseSettingModel>> commandResult = await _mediator.Send(new GetUserCourseSettingsQuery { UserId = userId }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
