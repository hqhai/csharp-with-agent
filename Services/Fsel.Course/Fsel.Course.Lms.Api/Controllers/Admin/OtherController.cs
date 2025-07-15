// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers.Admin
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Commands.CutOffCmd;
    using Fsel.Course.Lms.Application.Commands.OtherFeatureCmd;
    using Fsel.Course.Lms.Application.Queries.OtherFeatureQuery;
    using Fsel.Course.Lms.Application.Queries.Reports;
    using Fsel.Shared.Attributes;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersions(ApiSettings.APIVersion1)]
    [Route(Settings.APIDefaultRoute + "/admin/other")]
    [ApiController]
    [Common.Attributes.Permission(roles: new string[] { nameof(EnumRole.Admin), nameof(EnumRole.AdminSchool), nameof(EnumRole.CSO) })]
    public class OtherController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OtherController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get Param BeginnerGuide
        /// </summary>
        [HttpPost("param-beginner-guide")]
        [ProducesResponseType(typeof(MethodResult<IList<ParamBeginnerGuideModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetParamBeginnerGuide([FromBody] IList<Guid> studentIds)
        {
            MethodResult<IList<ParamBeginnerGuideModel>> queryResult = await _mediator.Send(new GetParamBeginnerGuideQuery { ListStudentIds = studentIds }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get PlacementTest Event
        /// </summary>
        [HttpGet("get-placement-test-event")]
        [ProducesResponseType(typeof(MethodResult<IList<ReportPlacementTestEventModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get([FromQuery] GetReportPlacementTestEventQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get Placement Test school Event
        /// </summary>
        [HttpGet("get-placement-test-school-event")]
        [ProducesResponseType(typeof(MethodResult<IList<ReportPlacementTestEventModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get([FromQuery] GetReportPlacementTestEventSchoolQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// restore cut off
        /// </summary>
        [HttpPost("restore-cutoff")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> RestoreDataCutOff([FromBody] RestoreDataCutOffCommand command)
        {
            var methodResult = await _mediator.Send(command).ConfigureAwait(false);
            return methodResult.GetActionResult();
        }

        /// <summary>
        /// Delete Placement Test And Course
        /// </summary>
        [HttpDelete("{studentId}/pt-and-course")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> DeletePTAndCourse([FromRoute] Guid studentId)
        {
            var queryResult = await _mediator.Send(new DeletePlacementTestCommand { StudentId = studentId }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}