// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers.V1i2
{
    using System.Net;
    using Application.Queries.LessonQuery.V1i2;
    using Common.ActionResults;
    using Common.Constants;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using Shared.Attributes;
    using Shared.Constants;
    using LessonModel = Domain.Models.EntityModels.V1i2.LessonModel;

    [ApiVersions(ApiSettings.APIVersion1i2)]
    [Route(Settings.APIDefaultRoute + "/lesson")]
    [ApiController]
    //[Permission(role: nameof(EnumRole.Student))]
    public class LessonController: ControllerBase
    {
        private readonly IMediator _mediator;

        public LessonController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get Lesson
        /// </summary>
        [HttpGet("v2")]
        [ProducesResponseType(typeof(MethodResult<IList<LessonModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetLessons([FromQuery] SearchLessonQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
