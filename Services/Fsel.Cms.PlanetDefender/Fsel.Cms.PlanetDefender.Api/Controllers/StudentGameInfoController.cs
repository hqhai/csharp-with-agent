// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Api.Controllers
{
    using System.Net;
    using Fsel.Cms.PlanetDefender.Application.Commands;
    using Fsel.Cms.PlanetDefender.Application.Commands.StudentGameInfoCmd;
    using Fsel.Cms.PlanetDefender.Application.Queries.StudentGameInfoQuery;
    using Fsel.Cms.PlanetDefender.Application.Services.UserServices.Models;
    using Fsel.Cms.PlanetDefender.Domain.Models.EntityModels;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Core.Base.BaseModels;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/cms-planet-defender")]
    [ApiController]
    public class StudentGameInfoController : ControllerBase
    {
        private readonly IMediator _mediator;

        public StudentGameInfoController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Search students in platform
        /// </summary>
        [HttpGet("search-students-in-platform")]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<StudentInPlatformModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetLevelOfStudentsByStudentids([FromQuery] SearchStudentsInPlatformQuery query)
        {
            MethodResult<PagingItemsModel<StudentInPlatformModel>> commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Create students nick name
        /// </summary>
        [HttpPost("create-nick-name")]
        [ProducesResponseType(typeof(MethodResult<StudentGameInfoModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreateNickNameStudentGameInfoCommand command)
        {
            MethodResult<StudentGameInfoModel> queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Link Account
        /// </summary>
        [HttpPost("link-account")]
        [ProducesResponseType(typeof(MethodResult<StudentGameInfoModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> LinkAccount([FromBody] LinkStudentGameInfoCommand command)
        {
            MethodResult<StudentGameInfoModel> queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
