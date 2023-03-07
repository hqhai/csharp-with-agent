using Fsel.Common.ActionResults;
using Fsel.Common.Constants;
using Fsel.Common.Helpers;
using Fsel.Core.Base.BaseModels;
using Fsel.Course.Application.Commands.PlacementTestCmd;
using Fsel.Course.Application.Commands.UnitCmd;
using Fsel.Course.Application.Queries.UnitQuery;
using Fsel.Course.Application.Querys.PlacementTestQuery;
using Fsel.Course.Common.Models.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Fsel.Course.Api.Controllers
{
    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/unit")]
    [ApiController]
    [Authorize]
    public class UnitController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UnitController(IMediator mediator)
        {
            _mediator = mediator;
        }
    

        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<UnitModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateUnitCommand command)
        {
            try
            {
                MethodResult<UnitModel> queryResult = await _mediator.Send(command).ConfigureAwait(false);
                return queryResult.GetActionResult();
            }
            catch (Exception ex)
            {
                VoidMethodResult errorResult = new VoidMethodResult();
                errorResult.AddErrorMessage(MethodHelper.GetExceptionMessage(ex));
                return errorResult.GetActionResult();
            }
        }

        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<UnitModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Search([FromQuery] SearchUnitQuery query)
        {
            try
            {
                MethodResult<PagingItemsModel<UnitModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
                return queryResult.GetActionResult();
            }
            catch (Exception ex)
            {
                VoidMethodResult errorResult = new VoidMethodResult();
                errorResult.AddErrorMessage(MethodHelper.GetExceptionMessage(ex));
                return errorResult.GetActionResult();
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(MethodResult<UnitModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> delete([FromRoute] Guid id)
        {

            /*return Ok(await _mediator.Send(new DeleteUnitCommand { Id=id}));*/

            try
            {
                MethodResult<bool> commandResult = await _mediator.Send(new DeleteUnitCommand { Id = id }).ConfigureAwait(false);
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
