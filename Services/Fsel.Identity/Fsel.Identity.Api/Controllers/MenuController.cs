using System.Net;
using Asp.Versioning;
using Fsel.Common.ActionResults;
using Fsel.Common.Attributes;
using Fsel.Common.Constants;
using Fsel.Identity.Application.Queries.MenuQuery;
using Fsel.Shared.Constants;
using Fsel.Shared.Models.ShareModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Fsel.Identity.Api.Controllers
{
    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/menu")]
    [ApiController]
    [Permission]
    public class MenuController : ControllerBase
    {
        private readonly IMediator _mediator;

        public MenuController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// get menus
        /// </summary>
        [HttpGet("get-menus")]
        [ProducesResponseType(typeof(MethodResult<IList<MenuModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetMenus()
        {
            var commandResult = await _mediator.Send(new GetMenusQuery()).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// get menus
        /// </summary>
        [HttpGet("get-menus-by-user")]
        [ProducesResponseType(typeof(MethodResult<IList<MenuConfig>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetMenusByUser()
        {
            var commandResult = await _mediator.Send(new GetMenuByUserQuery()).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
