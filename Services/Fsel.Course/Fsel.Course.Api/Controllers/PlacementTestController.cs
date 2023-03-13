using System.Net;
using Fsel.Common.ActionResults;
using Fsel.Common.Constants;
using Fsel.Common.Helpers;
using Fsel.Core.Base.BaseModels;
using Fsel.Course.Application.Commands.PlacementTestCmd;
using Fsel.Course.Application.Queries.PlacementTestQuery;
using Fsel.Course.Application.Querys.PlacementTestQuery;
using Fsel.Course.Domain.Models.EntityModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fsel.Course.Api.Controllers
{
    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/placement-test")]
    [ApiController]
    [Authorize]
    public class PlacementTestController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PlacementTestController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Search Placement Test
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<PlacementTestModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Search([FromQuery] SearchPlacementTestQuery query)
        {
            try
            {
                MethodResult<PagingItemsModel<PlacementTestModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
                return queryResult.GetActionResult();
            }
            catch (Exception ex)
            {
                VoidMethodResult errorResult = new VoidMethodResult();
                errorResult.AddErrorMessage(MethodHelper.GetExceptionMessage(ex));
                return errorResult.GetActionResult();
            }
        }

        /// <summary>
        /// Get Placement Test
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(MethodResult<PlacementTestModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Get([FromRoute] Guid id)
        {
            try
            {
                MethodResult<PlacementTestModel> commandResult = await _mediator.Send(new GetPlacementTestQuery { Id = id }).ConfigureAwait(false);
                return commandResult.GetActionResult();
            }
            catch (Exception ex)
            {
                VoidMethodResult errorResult = new VoidMethodResult();
                errorResult.AddErrorMessage(MethodHelper.GetExceptionMessage(ex));
                return errorResult.GetActionResult();
            }
        }

        /// <summary>
        /// Create a Placement Test
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<PlacementTestModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreatePlacementTestCommand command)
        {
            try
            {
                MethodResult<PlacementTestModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
                return commandResult.GetActionResult();
            }
            catch (Exception ex)
            {
                VoidMethodResult errorResult = new VoidMethodResult();
                errorResult.AddErrorMessage(MethodHelper.GetExceptionMessage(ex));
                return errorResult.GetActionResult();
            }
        }

        /// <summary>
        /// Update a Placement Test
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(MethodResult<PlacementTestModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdatePlacementTestCommand command)
        {
            try
            {
                command.Id = id;
                MethodResult<PlacementTestModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
                return commandResult.GetActionResult();
            }
            catch (Exception ex)
            {
                VoidMethodResult errorResult = new VoidMethodResult();
                errorResult.AddErrorMessage(MethodHelper.GetExceptionMessage(ex));
                return errorResult.GetActionResult();
            }
        }

        /// <summary>
        /// Delete a Placement Test
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(MethodResult<PlacementTestModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            try
            {
                MethodResult<bool> commandResult = await _mediator.Send(new DeletePlacementTestCommand { Id = id }).ConfigureAwait(false);
                return commandResult.GetActionResult();
            }
            catch (Exception ex)
            {
                VoidMethodResult errorResult = new VoidMethodResult();
                errorResult.AddErrorMessage(MethodHelper.GetExceptionMessage(ex));
                return errorResult.GetActionResult();
            }
        }
    }
}
