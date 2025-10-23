// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Api.Controllers.Admin
{
    using System.Net;
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Attributes;
    using Fsel.Common.Constants;
    using Fsel.Identity.Application.Commands.CampusCmd;
    using Fsel.Identity.Domain.Models;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/admin/campus")]
    [ApiController]
    [Permission(roles: new string[] { nameof(EnumRole.Admin), nameof(EnumRole.TechSP) })]
    public class CampusController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CampusController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get list student by student ids
        /// </summary>
        [HttpPost("add-students-to-curriculum")]
        [ProducesResponseType(typeof(MethodResult<AddStudentIntoSchoolClassCommandModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(CurriculumManagement.AddStudents)]
        public async Task<IActionResult> ImportStudentsIntoPlatform([FromForm] AddStudentsToCurriculumActiveCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);

            if (!commandResult.IsOK || commandResult.Result == null || commandResult.Result.Stream == null)
            {
                return commandResult.GetActionResult();
            }
            return File(commandResult.Result.Stream, Settings.Excels.ContentType, "Add_Students_To_Curriculum_Error.xlsx");
        }
    }
}
