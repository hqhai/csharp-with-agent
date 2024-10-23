// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers.Cso
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Course.Lms.Application.Queries.LessonQuery;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Attributes;

    [ApiVersions(ApiSettings.APIVersion1)]
    [Route(Settings.APIDefaultRoute + "/cso/lesson")]
    [ApiController]
    public class LessonController : ControllerBase
    {
        private readonly IMediator _mediator;

        public LessonController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get List lesson by Course
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<object>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetListLessonByCourseId([FromQuery] GetListUnitAndLessonByCourseQuery query)
        {
            MethodResult<object> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
