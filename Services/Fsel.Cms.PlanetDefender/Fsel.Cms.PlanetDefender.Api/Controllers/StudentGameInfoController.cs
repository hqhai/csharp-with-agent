// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Api.Controllers
{
    using System.Net;
    using Fsel.Cms.PlanetDefender.Application.Commands.StudentGameInfoCmd;
    using Fsel.Cms.PlanetDefender.Application.Commands.ZMatterCmd;
    using Fsel.Cms.PlanetDefender.Application.Queries.StudentGameInfoQuery;
    using Fsel.Cms.PlanetDefender.Application.Services.UserServices.Models;
    using Fsel.Cms.PlanetDefender.Domain.Models.EntityModels;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/cms-planet-defender")]
    [ApiController]
    public class StudentGameInfoController : BaseController
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
            SetQuery(query);
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

        /// <summary>
        /// get list avatar image
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<StudentGameInfoModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get()
        {
            var commandResult = await _mediator.Send(new GetStudentGameInfoQuery()).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Update Tag Name id
        /// </summary>
        [HttpPut("update-tag-name")]
        [ProducesResponseType(typeof(MethodResult<StudentGameInfoModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateTagName([FromBody] UpdateStudentTagNameCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);
            MethodResult<StudentGameInfoModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Update Tag Name id
        /// </summary>
        [HttpPut("update-avatar-image")]
        [ProducesResponseType(typeof(MethodResult<StudentGameInfoModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateAvatarImage([FromBody] UpdateStudentAvatarImageCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);
            MethodResult<StudentGameInfoModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// get info account
        /// </summary>
        [HttpGet("get-detail-account/{studentId}")]
        [ProducesResponseType(typeof(MethodResult<AccountModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetInfoAccount([FromRoute] Guid studentId)
        {
            var commandResult = await _mediator.Send(new GetInfoAccountQuery { StudentId = studentId }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
