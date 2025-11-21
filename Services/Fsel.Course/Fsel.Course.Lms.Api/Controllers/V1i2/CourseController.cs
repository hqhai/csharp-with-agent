// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers.V1i2
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Attributes;
    using Fsel.Common.Constants;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.EntityModels.ModuleModels;
    using Fsel.Course.Lms.Application.Commands.CourseResultCmd;
    using Fsel.Course.Lms.Application.Queries.CourseQuery.V1i2;
    using Fsel.Shared.Attributes;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersions(ApiSettings.APIVersion1i2)]
    [Route(Settings.APIDefaultRoute + "/course")]
    [Permission(role: nameof(EnumRole.Student))]
    [ApiController]
    public class CourseController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CourseController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get units by course Id
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<CourseModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get()
        {
            MethodResult<CourseModel> commandResult = await _mediator.Send(new GetCourseQuery()).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get Modules
        /// </summary>
        [HttpGet("modules")]
        [ProducesResponseType(typeof(MethodResult<IList<ModuleCourseModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetModules()
        {
            var queryResult = await _mediator.Send(new GetModulesQuery()).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Start Course Result
        /// </summary>
        [HttpPost("start/{courseResultId}")]
        [ProducesResponseType(typeof(MethodResult<CourseResultModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> StartCourseResult([FromRoute] Guid courseResultId)
        {
            MethodResult<CourseResultModel> commandResult = await _mediator.Send(new StartCourseResultCommand { CourseResultId = courseResultId }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
