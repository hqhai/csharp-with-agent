// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Queries.SettingStudentQuery;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/setting")]
    [ApiController]
    [Authorize(Roles = nameof(EnumRole.Student))]
    public class SettingStudentController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SettingStudentController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Setting Student
        /// </summary>
        [HttpGet("check-student")]
        [ProducesResponseType(typeof(MethodResult<SettingStudentModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CheckStudent()
        {
            MethodResult<SettingStudentModel> queryResult = await _mediator.Send(new SettingStudentCheckQuery()).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
