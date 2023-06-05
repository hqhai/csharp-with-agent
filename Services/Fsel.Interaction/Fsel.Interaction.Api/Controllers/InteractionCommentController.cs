// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Api.Controllers
{
    using Fsel.Common.ActionResults;
    using System.Net;
    using Fsel.Common.Constants;
    using Fsel.Interaction.Application.Commands.ActionCmd;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using Fsel.Interaction.Application.Commands.CommentCmd;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using Fsel.Interaction.Application.Queries.InteractionQuery;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/comment")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CommentController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Create action
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreateCommentCommand command)
        {
            MethodResult<bool> queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// get comment
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<IList<ListCommentModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get([FromQuery] GetCommentsByObjectId command)
        {
            MethodResult<IList<ListCommentModel>> queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
