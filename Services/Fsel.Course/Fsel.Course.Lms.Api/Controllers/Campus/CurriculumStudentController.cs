// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers.Campus
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Attributes;
    using Fsel.Common.Constants;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Commands.CurriculumCmd;
    using Fsel.Course.Lms.Application.Queries.CurriculumQuery;
    using Fsel.Shared.Attributes;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersions(ApiSettings.APIVersion1)]
    [Route(Settings.APIDefaultRoute + "/curriculum-student")]
    [ApiController]
    public class CurriculumStudentController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CurriculumStudentController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// get curriculums by student id
        /// </summary>
        [HttpGet("get-curriculums-by-student-id")]
        [ProducesResponseType(typeof(MethodResult<IList<CurriculumModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(roles: new string[] { nameof(EnumRole.Student), nameof(EnumRole.StudentCampus) })]
        public async Task<IActionResult> GetCurriculumsByStudentId([FromQuery] GetCurriculumsByStudentIdQuery query)
        {
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// get current curriculum
        /// </summary>
        [HttpGet("get-current-curriculum")]
        [ProducesResponseType(typeof(MethodResult<CurriculumModel?>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(roles: new string[] { nameof(EnumRole.Student), nameof(EnumRole.StudentCampus) })]
        public async Task<IActionResult> GetCurrentCurriculumOfStudent([FromQuery] GetCurrentCurriculumOfStudentQuery query)
        {
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// change curriculum
        /// </summary>
        [HttpPost("change-curriculum")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(roles: new string[] { nameof(EnumRole.StudentCampus) })]
        public async Task<IActionResult> ChangeCurriculumOfStudent([FromBody] ChangeCurriculumOfStudentCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// change curriculum
        /// </summary>
        [HttpPost("reset-curriculum")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(roles: new string[] { nameof(EnumRole.StudentCampus) })]
        public async Task<IActionResult> ResetCurriculumOfStudent([FromBody] ResetCurriculumByStudentCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
