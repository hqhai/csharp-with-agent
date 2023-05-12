// Copyright (c) Atlantic. All rights reserved.
using Fsel.Common.ActionResults;
using System.Net;
using Fsel.Common.Constants;
using Fsel.Course.Application.Commands.CourseCmd;
using Fsel.Course.Domain.Models.EntityModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Fsel.Course.Application.Commands.ClassForumCmd;
using Fsel.Course.Application.Commands.LessonCmd;

namespace Fsel.Course.Lcms.Api.Controllers
{
    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/class-forum")]
    [ApiController]
    public class ClassForumController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ClassForumController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Create a Class Forum
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<ClassForumModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreateClassForumCommand command)
        {
            MethodResult<ClassForumModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
