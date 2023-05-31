// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers.Cso
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Queries.ClassForumScoreQuery;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/cso/class-forum-score")]
    [ApiController]
    public class ClassForumScoreController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ClassForumScoreController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get List Class Forum Score
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<IList<ClassForumScoreModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetLIstCourseTeacher([FromQuery] GetListClassForumScoresQuery query)
        {
            MethodResult<IList<ClassForumScoreModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
