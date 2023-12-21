// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Api.Controllers.Student
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Interaction.Application.Commands.StudentReviewCmd;
    using Fsel.Interaction.Application.Queries.StudentReviewQuery;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Asp.Versioning;
    using Fsel.Shared.Constants;

    [ApiVersion(ApiSettings.APIVersion1)][ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/student/review")]
    [ApiController]
    [Common.Attributes.Permission(role: nameof(EnumRole.Student))]
    public class StudentReviewController : ControllerBase
    {
        private readonly IMediator _mediator;

        public StudentReviewController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Save Student Review
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<StudentReviewModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Save([FromBody] SaveStudentReviewCommand command)
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
        public async Task<IActionResult> Get()
        {
            MethodResult<IList<StudentReviewInfoModel>> queryResult = await _mediator.Send(new GetReviewStudentsByStudentQuery()).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
