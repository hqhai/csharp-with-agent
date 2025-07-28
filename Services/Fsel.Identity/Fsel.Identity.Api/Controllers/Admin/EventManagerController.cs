// Copyright (c) Atlantic. All rights reserved.

using System.Net;
using Asp.Versioning;
using Fsel.Common.ActionResults;
using Fsel.Common.Attributes;
using Fsel.Common.Constants;
using Fsel.Identity.Application.Commands.EventCmd;
using Fsel.Identity.Application.Queries.EventQuery;
using Fsel.Identity.Domain.Models.EntityModels;
using Fsel.Shared.Constants;
using Fsel.Shared.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Fsel.Identity.Api.Controllers.Admin
{
    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/admin/event-manager")]
    [ApiController]
    //[Permission(roles: new[] { nameof(EnumRole.Admin), nameof(EnumRole.DepartmentAdmin) })]
    public class EventManagerController : ControllerBase
    {
        private readonly IMediator _mediator;

        public EventManagerController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Thêm người dùng vào sự kiện
        /// </summary>
        [HttpPost("add-user-to-event")]
        [ProducesResponseType(typeof(MethodResult<EventManagerModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        //[Permission(roles: new[] { nameof(EnumRole.Admin), nameof(EnumRole.EducationDepartment), nameof(EnumRole.DepartmentAdmin) })]
        public async Task<IActionResult> AddUserToEvent([FromBody] AddUserToEventCommand command)
        {
            MethodResult<EventManagerModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Xóa người dùng khỏi sự kiện
        /// </summary>
        [HttpDelete("remove-user-from-event")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        //[Permission(roles: new[] { nameof(EnumRole.Admin), nameof(EnumRole.EducationDepartment), nameof(EnumRole.DepartmentAdmin) })]
        public async Task<IActionResult> RemoveUserFromEvent([FromBody] RemoveUserFromEventCommand command)
        {
            MethodResult<bool> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Lấy danh sách các sự kiện được quản lý bởi người dùng
        /// </summary>
        [HttpGet("get-events-by-user/{userId}")]
        [ProducesResponseType(typeof(MethodResult<List<CompetitionEventsModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetEventsByUser([FromRoute] Guid userId)
        {
            MethodResult<List<CompetitionEventsModel>> commandResult = await _mediator.Send(new GetEventsByUserQuery { UserId = userId }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
} 
