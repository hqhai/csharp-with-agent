// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Api.Controllers.Student
{
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.System.Application.Commands.ErrorReportCmd;
    using Fsel.System.Domain.Models.EntityModels;
    using global::System.Net;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/error-report")]
    [ApiController]
   [Common.Attributes.Permission(roles: new string[] { nameof(EnumRole.Student), nameof(EnumRole.StudentCampus) })]
    public class ErrorReportController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ErrorReportController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Create a error report
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<ErrorReportModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreateErrorReportComand command)
        {
            MethodResult<ErrorReportModel> queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
