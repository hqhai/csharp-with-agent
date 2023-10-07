// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Api.Controllers
{
    using Fsel.Common.ActionResults;
    using System.Net;
    using Fsel.Common.Constants;
    using MediatR;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Fsel.Cms.PlanetDefender.Application.Queries.LevelSettingQuery;
    using Fsel.Cms.PlanetDefender.Domain.Models.EntityModels;
    using Fsel.Cms.PlanetDefender.Application.Commands;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/level-setting")]
    [ApiController]
    [AllowAnonymous]
    public class LevelSettingController
    {
        private readonly IMediator _mediator;

        public LevelSettingController(IMediator mediator)
        {
            _mediator = mediator;
        }
        /// <summary>
        /// Get All Course type and Course level
        /// </summary>
        [HttpGet("all-course-level")]
        [ProducesResponseType(typeof(MethodResult<object>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetEnumCourseLevelsAsync()
        {
            var queryResult = await _mediator.Send(new GetAllEnumCourseLevelQuery()).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Create a Forbidden Word
        /// </summary>
        [HttpPost("choose-gender")]
        [ProducesResponseType(typeof(MethodResult<StudentGameInfoModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Create([FromBody] ChooseStudentGenderCommand command)
        {
            MethodResult<StudentGameInfoModel> queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Update a Forbidden Word
        /// </summary>
        [HttpPut("choose-level")]
        [ProducesResponseType(typeof(MethodResult<StudentGameInfoModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Update([FromBody] ChooseUserLevelCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);
            MethodResult<StudentGameInfoModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
