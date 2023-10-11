// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Commands.ClassForumResultCmd;
    using Fsel.Course.Lms.Application.Commands.ClassForumResultFlagCmd;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using System.Net;
    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/class-forum-result-flag")]
    [ApiController]
    [Authorize(Roles = nameof(EnumRole.Student))]
    public class ClassForumResultFlagController
    {
        private readonly IMediator _mediator;

        public ClassForumResultFlagController(IMediator mediator)
        {
            _mediator = mediator;
        }
        /// <summary>
        /// Rate Class Forum Result
        /// </summary>
        [HttpPost("rate")]
        [ProducesResponseType(typeof(MethodResult<ClassForumResultFlagModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Rate([FromBody] RateClassForumResultFlagCommand command)
        {
            MethodResult<ClassForumResultFlagModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
