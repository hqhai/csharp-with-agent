// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers
{
    using Fsel.Common.ActionResults;
    using System.Net;
    using Fsel.Common.Constants;
    using Fsel.Common.Enums;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Commands.LessonNoteCmd;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using MediatR;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/lessonResult")]
    /*    [Authorize(Roles = nameof(EnumRole.Student))]*/
    [ApiController]
    public class LessonResultController : ControllerBase
    {
        private readonly IMediator _mediator;

        public LessonResultController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Update a Lesson Summary  Note
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Update([FromRoute] Guid id)
        {
            MethodResult<bool> commandResult = await _mediator.Send(new UpdateLessonSummaryNoteCommand { Id = id }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
