// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Api.Controllers
{
    using System.Net;
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Interaction.Application.Commands.ActionCmd;
    using Fsel.Interaction.Application.Queries.InterationActionQuery;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using Fsel.Shared.Constants;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/interaction-action")]
    [ApiController]
    public class InteractionActionController : BaseController
    {
        private readonly IMediator _mediator;
        private readonly IInteractionActionRepository _interactionActionRepository;

        public InteractionActionController(IMediator mediator, IInteractionActionRepository interactionActionRepository)
        {
            _mediator = mediator;
            _interactionActionRepository = interactionActionRepository;
        }

        /// <summary>
        /// Execute-list-query
        /// </summary>
        [HttpGet("execute-list-query")]
        [ProducesResponseType(typeof(MethodResult<IList<InteractionActionModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ExecuteList([FromQuery] BaseQueryModel query)
        {
            SetQuery(query);
            var result = await _interactionActionRepository.GetListResultAsync<InteractionActionModel>(query);
            return result.GetActionResult();
        }

        /// <summary>
        /// Create action
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreateActionCommand command)
        {
            MethodResult<bool> queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// get interaction action
        /// </summary>
        [HttpPost("actions")]
        [ProducesResponseType(typeof(MethodResult<IList<InteractionActionModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get([FromBody] GetActionObjecIdsQuery command)
        {
            MethodResult<IList<InteractionActionModel>> queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Create action
        /// </summary>
        [HttpPost("aggregate-number-of-likes-and-comments")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetNumberLikeAndComment([FromBody] AggregateNumberOfLikesAndCommentsQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
