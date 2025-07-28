// Copyright (c) Atlantic. All rights reserved.

using System.Net;
using Fsel.Common.ActionResults;
using Fsel.Common.Constants;
using Fsel.Core.Base.BaseModels;
using Fsel.Course.Application.Commands.LessonCmd;
using Fsel.Course.Application.Queries.LessonQuery;
using Fsel.Course.Domain.Models.EntityModels;
using Fsel.Shared.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using Fsel.Shared.Constants;

namespace Fsel.Course.Lcms.Api.Controllers
{
    [ApiVersion(ApiSettings.APIVersion1)]
    [Route(Settings.APIDefaultRoute + "/lesson")]
    [ApiController]
    [Common.Attributes.Permission(role: nameof(EnumRole.MasterAdmin))]
    public class LessonController : ControllerBase
    {
        private readonly IMediator _mediator;

        public LessonController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Search Lesson
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<LessonSearchModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Search([FromQuery] SearchLessonQuery query)
        {
            MethodResult<PagingItemsModel<LessonSearchModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get Lesson
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(MethodResult<LessonModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get([FromRoute] Guid id)
        {
            MethodResult<LessonModel> queryResult = await _mediator.Send(new GetLessonQuery { Id = id }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Create a Lesson
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<LessonModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreateLessonCommand command)
        {
            MethodResult<LessonModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Update a Lesson
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(MethodResult<LessonModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateLessonCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);
            command.Id = id;
            MethodResult<LessonModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Delete a Lesson
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(MethodResult<LessonModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            MethodResult<bool> commandResult = await _mediator.Send(new DeleteLessonCommand { Id = id }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
