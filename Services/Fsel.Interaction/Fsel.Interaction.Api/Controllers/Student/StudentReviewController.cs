// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Api.Controllers.Student
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Interaction.Application.Commands.StudentReviewCmd;
    using Fsel.Interaction.Application.Queries.StudentReviewQuery;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/student/review")]
    [ApiController]
    public class StudentReviewController : ControllerBase
    {
        private readonly IMediator _mediator;

        public StudentReviewController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Create Student Review
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<StudentReviewModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreateStudentReviewCommand command)
        {
            MethodResult<StudentReviewModel> queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Update Student Review
        /// </summary>
        [HttpPut]
        [ProducesResponseType(typeof(MethodResult<StudentReviewModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Update([FromBody] UpdateStudentReviewCommand command)
        {
            MethodResult<StudentReviewModel> queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get Student Review
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<IList<StudentReviewInfoModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get([FromQuery] GetReviewStudentsByStudentQuery query)
        {
            MethodResult<IList<StudentReviewInfoModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
