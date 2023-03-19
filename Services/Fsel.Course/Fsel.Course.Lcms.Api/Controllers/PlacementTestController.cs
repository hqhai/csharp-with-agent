using System.Net;
using Fsel.Common.ActionResults;
using Fsel.Common.Constants;
using Fsel.Common.Enums;
using Fsel.Common.Helpers;
using Fsel.Core.Base.BaseModels;
using Fsel.Course.Application.Commands.PlacementTestCmd;
using Fsel.Course.Application.Queries.PlacementTestQuery;
using Fsel.Course.Application.Querys.PlacementTestQuery;
using Fsel.Course.Domain.Models.EntityModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fsel.Course.Lcms.Api.Controllers
{
    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/placement-test")]
    [ApiController]
    [Authorize(Roles = nameof(EnumRole.MasterAdmin))]
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
            catch
            {
                VoidMethodResult errorResult = new VoidMethodResult();
                errorResult.AddError();
                return errorResult.GetActionResult();
            }
        }

        /// <summary>
        /// Get Placement Test
        /// </summary>
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
            catch
            {
                VoidMethodResult errorResult = new VoidMethodResult();
                errorResult.AddError();
                return errorResult.GetActionResult();
            }
        }

        /// <summary>
        /// Create a Placement Test
        /// </summary>
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
            catch
            {
                VoidMethodResult errorResult = new VoidMethodResult();
                errorResult.AddError();
                return errorResult.GetActionResult();
            }
        }

        /// <summary>
        /// Update a Placement Test
        /// </summary>
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
            catch
            {
                VoidMethodResult errorResult = new VoidMethodResult();
                errorResult.AddError();
                return errorResult.GetActionResult();
            }
        }

        /// <summary>
        /// Delete a Placement Test
        /// </summary>
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
            catch
            {
                VoidMethodResult errorResult = new VoidMethodResult();
                errorResult.AddError();
                return errorResult.GetActionResult();
            }
        }
    }
}
