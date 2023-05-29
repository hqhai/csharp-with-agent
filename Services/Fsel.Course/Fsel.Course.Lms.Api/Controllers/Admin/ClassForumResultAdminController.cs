// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers.Admin
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Commands.CsoApproveCmd;
    using Fsel.Course.Lms.Application.Queries.ClassForumResultQuery;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/admin/class-forum-result")]
    [ApiController]
    [Authorize(Roles = nameof(EnumRole.MasterAdmin))]
    public class ClassForumResultAdminController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ClassForumResultAdminController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Search Class Forum Result
        /// </summary>
        [HttpGet("cso/search")]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<ClassForumResultSearchModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Search([FromQuery] SearchClassForumResultQuery query)
        {
            query.Status = Domain.Enums.EnumClassForumResultStatus.Pending;
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Cso approve
        /// </summary>
        [HttpPut]
        [ProducesResponseType(typeof(MethodResult<ClassForumResultModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Update([FromBody] ApproveClassForumPenddingCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);
            MethodResult<ClassForumResultModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
