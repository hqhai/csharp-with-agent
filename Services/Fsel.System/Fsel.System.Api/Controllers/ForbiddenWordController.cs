// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Api.Controllers
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Core.Base.BaseModels;
    using Fsel.System.Application.Commands.ForbiddenWordCmd;
    using Fsel.System.Application.Queries.ForbiddenWordQuery;
    using Fsel.System.Application.Querys.ForbiddenWordQuery;
    using Fsel.System.Domain.Models.EntityModels;
    using global::System.Net;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using Asp.Versioning;
    using Fsel.Shared.Constants;
    using Fsel.Common.Attributes;

    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/forbidden-word")]
    [ApiController]
    public class ForbiddenWordController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ForbiddenWordController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Search Forbidden Word
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<ForbiddenWordModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(ForbiddenWordsStorage.View)]
        public async Task<IActionResult> Search([FromQuery] SearchForbiddenWordQuery query)
        {
            MethodResult<PagingItemsModel<ForbiddenWordModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// get list
        /// </summary>
        [HttpGet("get-list-forbidden-word")]
        [ProducesResponseType(typeof(MethodResult<IList<string>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission]
        public async Task<IActionResult> CheckContainForbiddenWord([FromQuery] CheckContainForbiddenWordQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Create a Forbidden Word
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<ForbiddenWordModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(ForbiddenWordsStorage.Add)]
        public async Task<IActionResult> Create([FromBody] CreateForbiddenWordCommand command)
        {
            MethodResult<ForbiddenWordModel> queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Update a Forbidden Word
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(MethodResult<ForbiddenWordModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(ForbiddenWordsStorage.Update)]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateForbiddenWordCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);
            command.Id = id;
            MethodResult<ForbiddenWordModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Delete a Forbidden Word
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(MethodResult<ForbiddenWordModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(ForbiddenWordsStorage.Update)]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            MethodResult<bool> commandResult = await _mediator.Send(new DeleteForbiddenWordCommand { Id = id }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Delete a Forbidden Word
        /// </summary>
        [HttpDelete("list-forbidden-word")]
        [ProducesResponseType(typeof(MethodResult<ForbiddenWordModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(ForbiddenWordsStorage.Update)]
        public async Task<IActionResult> DeleteListForbiddenWord([FromBody] DeleteListForbiddenWordCommand delete)
        {
            MethodResult<bool> commandResult = await _mediator.Send(delete).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get Forbidden Word
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(MethodResult<ForbiddenWordModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(ForbiddenWordsStorage.View)]
        public async Task<IActionResult> Get([FromRoute] Guid id)
        {
            MethodResult<ForbiddenWordModel> commandResult = await _mediator.Send(new GetForbiddenWordQuery { Id = id }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
