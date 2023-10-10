// Copyright (c) Atlantic. All rights reserved.

using System.Net;
using Fsel.Common.ActionResults;
using Fsel.Common.Attributes;
using Fsel.Common.Constants;
using Fsel.Core.Base.BaseModels;
using Fsel.Course.Application.Commands.CourseCmd;
using Fsel.Course.Application.Queries;
using Fsel.Course.Application.Queries.CourseQuery;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.EntityModels;
using Fsel.Shared.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Fsel.Course.Lcms.Api.Controllers
{
    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/course")]
    [ApiController]
    //[Permission(role: nameof(EnumRole.MasterAdmin))]
    public class CourseController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CourseController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Search Course
        /// </summary>
        [HttpPost("get-all")]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<CourseSearchModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Search([FromBody] ExecuteQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Search Course
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<CourseSearchModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Search([FromQuery] SearchCourseQuery query)
        {
            MethodResult<PagingItemsModel<CourseSearchModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Create a Course
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<CourseModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreateCourseCommand command)
        {
            MethodResult<CourseModel> queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Update a Course
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(MethodResult<CourseModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateCourseCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);
            command.Id = id;
            MethodResult<CourseModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Delete a Course
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(MethodResult<CourseModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            MethodResult<bool> commandResult = await _mediator.Send(new DeleteCourseCommand { Id = id }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Update a Course in Active
        /// </summary>
        [HttpPut("active/{id}")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Active([FromRoute] Guid id)
        {
            MethodResult<bool> commandResult = await _mediator.Send(new ActiveCourseCommand { Id = id }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get Course
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(MethodResult<CourseModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get([FromRoute] Guid id)
        {
            MethodResult<CourseModel> commandResult = await _mediator.Send(new GetCourseQuery { Id = id }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get Courses by level
        /// </summary>
        [HttpGet("get-by-level")]
        [ProducesResponseType(typeof(MethodResult<List<CourseModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetCourseByLevel([FromQuery] GetCoursesByLevelQuery query)
        {
            MethodResult<List<CourseModel>> commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
