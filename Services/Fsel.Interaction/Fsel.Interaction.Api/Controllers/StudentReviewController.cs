// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Api.Controllers
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Interaction.Application.Queries.StudentReviewQuery;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/review")]
    [ApiController]
    public class StudentReviewController : ControllerBase
    {
        private readonly IMediator _mediator;

        public StudentReviewController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get List Student Review
        /// </summary>
        [HttpGet("student-reviews")]
        [ProducesResponseType(typeof(MethodResult<IList<StudentReviewModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get([FromQuery] EnumReviewType reviewType)
        {
            var queryResult = await _mediator.Send(new GetListStudentReviewQuery { ReviewType = reviewType }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
