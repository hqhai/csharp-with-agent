// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Api.Controllers.Admin
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Core.Base;
    using Fsel.Ordering.Application.Commands.StudentCmd;
    using Fsel.Shared.Attributes;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersions(ApiSettings.APIVersion1)]
    [Route(Settings.APIDefaultRoute + "/admin/order")]
    [ApiController]
    [Common.Attributes.Permission(role: nameof(EnumRole.Admin))]
    public class StudentController : BaseController
    {
        private readonly IMediator _mediator;

        public StudentController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// delete
        /// </summary>
        [HttpDelete("delete-student/{id}")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> DeleteStudentFromClass([FromRoute] Guid id)
        {
            MethodResult<bool> commandResult = await _mediator.Send(new DeleteListDataUserCommand { UserId = id }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
