// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Course.Lms.Application.Commands.LessonNoteCmd;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/lesson-result")]
    [ApiController]
    public class LessonResultController : ControllerBase
    {
        private readonly IMediator _mediator;

        public LessonResultController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /* /// <summary>
         /// Update a Lesson Summary  Note
         /// </summary>
         [HttpPut("{id}")]
         [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
         [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
         public async Task<IActionResult> Update([FromRoute] Guid id)
         {
             MethodResult<bool> commandResult = await _mediator.Send(new UpdateLessonSummaryNoteCommand { Id = id }).ConfigureAwait(false);
             return commandResult.GetActionResult();
         }*/
    }
}
