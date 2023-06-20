// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Api.Controllers
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Core.Base.BaseModels;
    using Fsel.System.Application.Commands.CourseTimeConfigCmd;
    using Fsel.System.Application.Querys;
    using Fsel.System.Application.Querys.CourseTimeConfigQuery;
    using Fsel.System.Domain.Models;
    using global::System.Net;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/course-time-config")]
    [ApiController]
    public class CourseTimeConfigController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CourseTimeConfigController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// get list course time config
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<CourseTimeConfigModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Search([FromQuery] GetListCourseTimeConfigQuery query)
        {
            MethodResult<PagingItemsModel<CourseTimeConfigModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Update course time config
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(MethodResult<CourseTimeConfigModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] SetMonthToClassCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);
            command.Id = id;
            MethodResult<CourseTimeConfigModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Update course time config
        /// </summary>
        [HttpPut]
        [ProducesResponseType(typeof(MethodResult<IList<CourseTimeConfigModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Update([FromBody] SetEnrollmentWeekToClassCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);
            MethodResult<IList<CourseTimeConfigModel>> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get List Courses time by course ids
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<IList<CourseTimeConfigModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetCoursesByIds([FromBody] IList<Guid> courseIds)
        {
            MethodResult<IList<CourseTimeConfigModel>> queryResult = await _mediator.Send(new GetCourseTimeConfigByListCourseIdQuery { CourseIds = courseIds }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
