// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lcms.Api.Controllers
{
    using System.Net;
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Course.Domain.Models.CommandModels.RubyScope;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Application.Commands.RubyAnnotationCmd;
    using Fsel.Course.Application.Commands.RubyScopeCmd;
    using Fsel.Course.Application.Queries.RubyQuery;
    using Fsel.Shared.Constants;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using Fsel.Shared.Enums;
    using Fsel.Course.Domain.Models.QueryModels.Ruby;

    [ApiVersion(ApiSettings.APIVersion1)]
    [Route(Settings.APIDefaultRoute + "/ruby")]
    [ApiController]
    //[Common.Attributes.Permission(role: nameof(EnumRole.MasterAdmin))]
    public class RubyController : ControllerBase
    {
        private readonly IMediator _mediator;

        public RubyController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Create Annotation for scope
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<RubyAnnotationModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreateRubyAnnotationCommand command)
        {
            MethodResult<CreateRubyByScopeCommandModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Render ruby
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpGet("render")]
        [ProducesResponseType(typeof(MethodResult<object>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Render([FromQuery] RenderRubyQuery command)
        {
            MethodResult<object> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get ruby by objectId
        /// </summary>
        /// <param name="objectId"></param>
        /// <returns></returns>
        [HttpGet("{objectId}")]
        [ProducesResponseType(typeof(MethodResult<object>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Render([FromRoute] Guid objectId)
        {
            MethodResult<RubyResponseModel> commandResult = await _mediator.Send(new RubyResponseQuery { ObjectId = objectId}).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Delete a Ruby Scope
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(MethodResult<object>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            MethodResult<bool> commandResult = await _mediator.Send(new DeleteRubyScopeCommand { Id = id}).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Delete a RubyAnnotation
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("annotation/{id}")]
        [ProducesResponseType(typeof(MethodResult<object>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> DeleteRubyAnnotation([FromRoute] Guid id)
        {
            MethodResult<bool> commandResult = await _mediator.Send(new DeleteRubyAnnotationCommand { Id = id }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Update a RubyAnnotaion
        /// </summary>
        [HttpPut("annotation/{id}")]
        [ProducesResponseType(typeof(MethodResult<LessonNoteModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateRubyAnnotationCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);

            command.Id = id;
            MethodResult<RubyAnnotationModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
