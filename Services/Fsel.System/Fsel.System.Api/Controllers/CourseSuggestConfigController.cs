// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Api.Controllers
{
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Attributes;
    using Fsel.Common.Constants;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Constants;
    using Fsel.System.Application.Commands.CourseSuggestConfigCmd;
    using Fsel.System.Application.Queries.CourseSuggestConfigQuery;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using Fsel.System.Domain.Models.EntityModels.IntegrationModels;
    using global::System.Net;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/course-suggest-config")]
    [ApiController]
    public class CourseSuggestConfigController : BaseController
    {
        private readonly IMediator _mediator;
        private readonly ICourseSuggestConfigRepository _courseSuggestConfigRepository;

        public CourseSuggestConfigController(IMediator mediator, ICourseSuggestConfigRepository courseSuggestConfigRepository)
        {
            _mediator = mediator;
            _courseSuggestConfigRepository = courseSuggestConfigRepository;
        }

        /// <summary>
        /// Execute-list-query
        /// </summary>
        [HttpPost("execute-list-query")]
        [ProducesResponseType(typeof(MethodResult<IList<CourseSuggestConfigModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ExecuteList([FromBody] BaseQueryModel query)
        {
            ArgumentNullException.ThrowIfNull(query);
            query.SetIsQueryAll(true);
            var result = await _courseSuggestConfigRepository.GetListResultAsync<CourseSuggestConfigModel>(query);
            return result.GetActionResult();
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

        /// <summary>
        /// Course Suggest Config
        /// </summary>
        [HttpPost("level-suggestion-users")]
        [ProducesResponseType(typeof(MethodResult<IList<CourseSuggestUsersModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetCourseSuggestByUserIds([FromBody] GetCourseSuggestByUserIdsQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
