using System.Net;
using Asp.Versioning;
using Fsel.Common.ActionResults;
using Fsel.Common.Attributes;
using Fsel.Common.Constants;
using Fsel.Core.Base.BaseModels;
using Fsel.Identity.Application.Commands.PermissionCmd;
using Fsel.Identity.Application.Commands.PermissionGroupCmd;
using Fsel.Identity.Application.Commands.RoleClaimCmd;
using Fsel.Identity.Application.Queries.AuthQuery;
using Fsel.Identity.Application.Queries.PermissionGroupQuery;
using Fsel.Identity.Application.Queries.PermissionQuery;
using Fsel.Identity.Application.Queries.RoleClaimQuery;
using Fsel.Identity.Domain.Entities;
using Fsel.Identity.Domain.Models.EntityModels;
using Fsel.Identity.Domain.Models.EntityModels.Permissions;
using Fsel.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Fsel.Identity.Api.Controllers.Admin
{
    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/admin/permission")]
    [ApiController]
    public class PermissionController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PermissionController(IMediator mediator)
        {
            _mediator = mediator;
        }

        #region Role Claim

        /// <summary>
        /// get role claims by role id
        /// </summary>
        [HttpGet("get-role-claims-by-role-id")]
        [ProducesResponseType(typeof(MethodResult<IList<RoleClaimModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(AuthorizationManagement.View)]
        public async Task<IActionResult> GetRoleClaimByRoleId([FromQuery] GetRoleClaimsByRoleIdQuery query)
        {
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// save role claims
        /// </summary>
        [HttpPost("save-role-claims")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(AuthorizationManagement.Update)]
        public async Task<IActionResult> SaveRoleClaims([FromBody] SaveRoleClaimsCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// get roles
        /// </summary>
        [HttpGet("get-roles")]
        [ProducesResponseType(typeof(MethodResult<IList<RoleModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(AuthorizationManagement.View)]
        public async Task<IActionResult> GetRoles([FromQuery] GetRolesQuery query)
        {
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        #endregion Role Claim

        #region Permission Group

        /// <summary>
        /// search permission group
        /// </summary>
        [HttpGet("search-permission-group")]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<PermissionGroupModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(RoleGroupManagement.View)]
        public async Task<IActionResult> ChangeStatusPermissionGroup([FromQuery] SearchPermissionGroupQuery query)
        {
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// change status permission group
        /// </summary>
        [HttpPost("change-status-permission-group")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(RoleGroupManagement.Update)]
        public async Task<IActionResult> ChangeStatusPermissionGroup([FromBody] ChangeStatusPermissionGroupCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// delete status permission group
        /// </summary>
        [HttpPost("delete-permission-group")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(RoleGroupManagement.Delete)]
        public async Task<IActionResult> DeletePermissionGroup([FromBody] DeletePermissionGroupCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// save permission group
        /// </summary>
        [HttpPost("save-permission-group")]
        [ProducesResponseType(typeof(MethodResult<PermissionGroupModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(new[] { RoleGroupManagement.Add, RoleGroupManagement.Update })]
        public async Task<IActionResult> SavePermissionGroup([FromBody] SavePermissionGroupCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        #endregion Permission Group

        #region Permission

        /// <summary>
        /// search permission
        /// </summary>
        [HttpGet("search-permission")]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<PermissionGroupModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(PermissionManagement.View)]
        public async Task<IActionResult> ChangeStatusPermission([FromQuery] SearchPermissionQuery query)
        {
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// change status permission
        /// </summary>
        [HttpPost("change-status-permission")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(PermissionManagement.Update)]
        public async Task<IActionResult> ChangeStatusPermission([FromBody] ChangeStatusPermissionCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// delete status permission
        /// </summary>
        [HttpPost("delete-permission")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(PermissionManagement.Delete)]
        public async Task<IActionResult> DeletePermission([FromBody] DeletePermissionCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// save permission
        /// </summary>
        [HttpPost("save-permission")]
        [ProducesResponseType(typeof(MethodResult<PermissionModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(new[] { PermissionManagement.Add, PermissionManagement.Update })]
        public async Task<IActionResult> SavePermission([FromBody] SavePermissionCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        #endregion Permission
    }
}
