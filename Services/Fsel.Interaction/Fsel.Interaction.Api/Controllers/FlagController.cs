// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Api.Controllers
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Attributes;
    using Fsel.Common.Constants;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Interaction.Application.Commands.FlagCmd;
    using Fsel.Interaction.Application.Queries.FlagQuery;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/flag")]
    [ApiController]
    public class FlagController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IFlagRepository _flagRepository;

        public FlagController(IMediator mediator, IFlagRepository flagRepository)
        {
            _mediator = mediator;
            _flagRepository = flagRepository;
        }

        /// <summary>
        /// Execute-list-query
        /// </summary>
        [HttpPost("execute-list-query")]
        [ProducesResponseType(typeof(MethodResult<IList<FlagModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission]
        public async Task<IActionResult> ExecuteList([FromBody] BaseQueryModel query)
        {
            var result = await _flagRepository.GetListResultAsync<FlagModel>(query);
            return result.GetActionResult();
        }

        /// <summary>
        /// Rate Class Forum Result
        /// </summary>
        [HttpPost("rate")]
        [ProducesResponseType(typeof(MethodResult<FlagModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Rate([FromBody] RateFlagCommand command)
        {
            MethodResult<FlagModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Search flag
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<FlagModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Search([FromQuery] SearchFlagQuery query)
        {
            MethodResult<PagingItemsModel<FlagModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Update status flag
        /// </summary>
        [HttpPut("update-status")]
        [ProducesResponseType(typeof(MethodResult<FlagModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateStatus([FromBody] UpdateStatusFlagCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);
            MethodResult<bool> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
