// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Api.Controllers.Admins
{
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.System.Application.Commands.StudentCmd;
    using global::System.Net;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/admin/student")]
    [ApiController]
    [Common.Attributes.Permission(role: nameof(EnumRole.Admin))]
    public class StudentController : ControllerBase
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
