// Copyright (c) Atlantic. All rights reserved.

using System.Net;
using Asp.Versioning;
using Fsel.Common.ActionResults;
using Fsel.Common.Attributes;
using Fsel.Common.Constants;
using Fsel.Core.Base.BaseModels;
using Fsel.Identity.Application.Commands.UserGroupCmd;
using Fsel.Identity.Application.Queries.UserGroupQuery;
using Fsel.Identity.Domain.Models.EntityModels;
using Fsel.Shared.Constants;
using Fsel.Shared.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Fsel.Identity.Api.Controllers.Admin
{
    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/admin/user-group")]
    [ApiController]
    //[Permission]
    public class UserGroupController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UserGroupController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Lấy danh sách nhóm người dùng
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<UserGroupModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        //[Permission(role: nameof(EnumRole.Admin))]
        public async Task<IActionResult> GetUserGroups([FromQuery] GetUserGroupsQuery query)
        {
            var result = await _mediator.Send(query).ConfigureAwait(false);
            return result.GetActionResult();
        }

        /// <summary>
        /// Lấy thông tin nhóm người dùng theo Id
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(MethodResult<UserGroupModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        //[Permission(role: nameof(EnumRole.Admin))]
        public async Task<IActionResult> GetUserGroupById([FromRoute] Guid id)
        {
            var result = await _mediator.Send(new GetUserGroupByIdQuery { Id = id }).ConfigureAwait(false);
            return result.GetActionResult();
        }

        /// <summary>
        /// Tạo nhóm người dùng mới
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<UserGroupModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreateUserGroup([FromBody] CreateUserGroupCommand command)
        {
            var result = await _mediator.Send(command).ConfigureAwait(false);
            return result.GetActionResult();
        }

        /// <summary>
        /// Cập nhật thông tin nhóm người dùng
        /// </summary>
        [HttpPut]
        [ProducesResponseType(typeof(MethodResult<UserGroupModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        //[Permission(role: nameof(EnumRole.Admin))]
        public async Task<IActionResult> UpdateUserGroup([FromBody] UpdateUserGroupCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);
            var result = await _mediator.Send(command).ConfigureAwait(false);
            return result.GetActionResult();
        }

        /// <summary>
        /// Xóa nhóm người dùng
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        //[Permission(role: nameof(EnumRole.Admin))]
        public async Task<IActionResult> DeleteUserGroup([FromRoute] Guid id)
        {
            var result = await _mediator.Send(new DeleteUserGroupCommand { Id = id }).ConfigureAwait(false);
            return result.GetActionResult();
        }

        /// <summary>
        /// Thêm người dùng vào nhóm
        /// </summary>
        [HttpPost("add-user")]
        [ProducesResponseType(typeof(MethodResult<UserGroupMemberShipModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        //[Permission(role: nameof(EnumRole.Admin))]
        public async Task<IActionResult> AddUserToGroup([FromBody] AddUserToGroupCommand command)
        {
            var result = await _mediator.Send(command).ConfigureAwait(false);
            return result.GetActionResult();
        }

        /// <summary>
        /// Xóa người dùng khỏi nhóm
        /// </summary>
        [HttpPost("remove-user")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        //[Permission(role: nameof(EnumRole.Admin))]
        public async Task<IActionResult> RemoveUserFromGroup([FromBody] RemoveUserFromGroupCommand command)
        {
            var result = await _mediator.Send(command).ConfigureAwait(false);
            return result.GetActionResult();
        }
    }
} 
