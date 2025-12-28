// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers.V1i1
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Commands.PlacementTestCmd.V1i1;
    using Fsel.Shared.Attributes;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersions(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/placement-test")]
    [ApiController]
    [Common.Attributes.Permission(roles: new string[] { nameof(EnumRole.Student), nameof(EnumRole.StudentCampus) })]
    public class PlacementTestController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PlacementTestController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Create PlacementTest Answers
        /// </summary>
        [HttpPost("create-answers")]
        [Common.Attributes.Permission(roles: new string[] { nameof(EnumRole.Student), nameof(EnumRole.StudentCampus) })]
        [ProducesResponseType(typeof(MethodResult<PlacementTestResultModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreateAnswer([FromBody] CreatePlacementTestAnswerBySectionGroupCommand command)
        {
            MethodResult<PlacementTestResultModel> queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
