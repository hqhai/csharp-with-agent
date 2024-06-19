// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Api.Controllers
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Attributes;
    using Fsel.Common.Constants;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Identity.Application.Commands.UserCmd;
    using Fsel.Identity.Application.Queries.PlatformQuery;
    using Fsel.Identity.Application.Queries.StudentQuery;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using Asp.Versioning;
    using Fsel.Shared.Constants;

    [ApiVersion(ApiSettings.APIVersion1)][ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/platform")]
    [ApiController]
    public class PlatformController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IPlatformRepository _platformRepository;
        public PlatformController(IMediator mediator, IPlatformRepository platformRepository)
        {
            _mediator = mediator;
            _platformRepository = platformRepository;
        }

        /// <summary>
        /// Execute-list-query
        /// </summary>
        [HttpPost("execute-list-query")]
        [ProducesResponseType(typeof(MethodResult<IList<PlatformModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission]
        public async Task<IActionResult> ExecuteList([FromBody] BaseQueryModel query)
        {
            var result = await _platformRepository.GetListResultAsync<PlatformModel>(query);
            return result.GetActionResult();
        }

        /// <summary>
        /// Get all platform
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<IList<PlatformModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetAll()
        {
            MethodResult<IList<PlatformModel>> commandResult = await _mediator.Send(new GetAllPlatformQuery { }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get platforms by type
        /// </summary>
        [HttpGet("get-platforms-by-type")]
        [ProducesResponseType(typeof(MethodResult<IList<PlatformModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetPlatformByType([FromQuery] GetPlatformsByTypeQuery query)
        {
            MethodResult<IList<PlatformModel>> commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get all platform
        /// </summary>
        [HttpGet("get-students-in-platform")]
        [ProducesResponseType(typeof(MethodResult<IList<StudentInPlatformModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetStudentsInPlatform([FromQuery] GetStudentsInPlatformQuery query)
        {
            MethodResult<IList<StudentInPlatformModel>> commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// change status account
        /// </summary>
        [HttpPost("change-status-account")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetAll([FromBody] ChangeAccountStatusCommand command)
        {
            MethodResult<bool> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
