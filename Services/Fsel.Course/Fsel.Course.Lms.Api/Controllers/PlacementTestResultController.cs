// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Queries.PlacementTestResultQuery;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/placement-test-result")]
    [ApiController]
    public class PlacementTestResultController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PlacementTestResultController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get Level By Student
        /// </summary>
        [HttpGet("level")]
        [Common.Attributes.Permission(role: nameof(EnumRole.Student))]
        [ProducesResponseType(typeof(MethodResult<PlacementTestDtoModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetLevelByStudentAsync()
        {
            var queryResult = await _mediator.Send(new GetPlacementTestByLevelQuery()).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get Sections By SectionGroup
        /// </summary>
        [HttpGet("sections")]
        [Common.Attributes.Permission(role: nameof(EnumRole.Student))]
        [ProducesResponseType(typeof(MethodResult<SectionGroupDtoModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetSectionsBySectionGroup([FromQuery] GetSectionBySectionGroupIdQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
