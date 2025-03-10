// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Api.Controllers
{
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Attributes;
    using Fsel.Common.Constants;
    using Fsel.Shared.Constants;
    using Fsel.System.Application.Commands.CourseSuggestConfigCmd;
    using Fsel.System.Application.Queries.CourseSuggestConfigQuery;
    using Fsel.System.Domain.Models.EntityModels;
    using global::System.Net;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/course-suggest-config")]
    [ApiController]
    public class CourseSuggestConfigController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CourseSuggestConfigController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Create Course Suggest Config
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<CourseSuggestConfigModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission]
        public async Task<IActionResult> CreateCourseSuggestConfig([FromBody] CreateCourseSuggestConfigCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Update Course Suggest Config
        /// </summary>
        [HttpPut]
        [ProducesResponseType(typeof(MethodResult<CourseSuggestConfigModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission]
        public async Task<IActionResult> UpdateCourseSuggestConfig([FromBody] UpdateCourseSuggestConfigCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Course Suggest Config
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<CourseSuggestConfigModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission]
        public async Task<IActionResult> GetCourseSuggestConfig([FromQuery] GetCourseSuggestConfigQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Course Suggest Config ById
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(MethodResult<CourseSuggestConfigModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission]
        public async Task<IActionResult> GetCourseSuggestConfigById([FromRoute] Guid id)
        {
            var queryResult = await _mediator.Send(new GetCourseSuggestConfigByIdQuery { Id = id }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Course Suggest Config
        /// </summary>
        [HttpGet("level-suggestion")]
        [ProducesResponseType(typeof(MethodResult<IList<CourseSuggestConfigStudentModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission]
        public async Task<IActionResult> GetCourseSuggestConfigStudent()
        {
            var queryResult = await _mediator.Send(new GetCourseSuggestConfigStudentQuery()).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Check Course Suggest Config
        /// </summary>
        [HttpGet("check-suggestion")]
        [ProducesResponseType(typeof(MethodResult<IList<CourseSuggestConfigStudentModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission]
        public async Task<IActionResult> CheckCourseSuggetConfigByStudent([FromQuery] CheckCourseSuggetConfigByStudentQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
