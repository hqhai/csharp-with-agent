using Fsel.Common.ActionResults;
using Fsel.Common.Constants;
using Fsel.Common.Helpers;
using Fsel.Core.Base.BaseModels;
using Fsel.Course.Application.Commands.PlacementTestCmd;
using Fsel.Course.Application.Querys.PlacementTestQuery;
using Fsel.Course.Common.Models.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

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
        public async Task<IActionResult> Search([FromQuery] SearchPlacementTestQuery command)
        {
            try
            {
                MethodResult<PagingItemsModel<PlacementTestModel>> commandResult = await _mediator.Send(command).ConfigureAwait(false);
                return commandResult.GetActionResult();
            }
            catch (Exception ex)
            {
                VoidMethodResult errorCommandResult = new VoidMethodResult();
                errorCommandResult.AddErrorMessage(MethodHelper.GetExceptionMessage(ex));
                return errorCommandResult.GetActionResult();
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
                VoidMethodResult errorCommandResult = new VoidMethodResult();
                errorCommandResult.AddErrorMessage(MethodHelper.GetExceptionMessage(ex));
                return errorCommandResult.GetActionResult();
            }
        }
    }
}