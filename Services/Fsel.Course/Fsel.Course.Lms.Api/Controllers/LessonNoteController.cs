// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Commands.LessonNoteCmd;
    using Fsel.Course.Lms.Application.Queries.LessonNoteQuery;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/lesson-note")]
    [ApiController]
    public class LessonNoteController : ControllerBase
    {
        private readonly IMediator _mediator;

        public LessonNoteController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get List Lesson Note
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(MethodResult<LessonNoteModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get([FromRoute] Guid id)
        {
            MethodResult<IList<LessonNoteModel>> queryResult = await _mediator.Send(new GetLessonNoteQuery { Id = id }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Create a Lesson Note
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<LessonNoteModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreateLessonNoteCommand command)
        {
            MethodResult<LessonNoteModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Update a Lesson Note
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(MethodResult<LessonNoteModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateLessonNoteCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);
            command.Id = id;
            MethodResult<LessonNoteModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Delete a Lesson Note
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(MethodResult<LessonNoteModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            MethodResult<bool> commandResult = await _mediator.Send(new DeleteLessonNoteCommand { Id = id }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Update a Lesson Summary  Note
        /// </summary>
        [HttpPut("{lesson-result-id}")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] string summaryNote)
        {
            MethodResult<bool> commandResult = await _mediator.Send(new UpdateLessonSummaryNoteCommand { Id = id, SummaryNote = summaryNote }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
