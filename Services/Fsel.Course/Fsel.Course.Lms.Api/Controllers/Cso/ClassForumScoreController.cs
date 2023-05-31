// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers.Cso
{
    using Fsel.Common.ActionResults;
    using System.Net;
    using Fsel.Common.Constants;
    using Fsel.Course.Lms.Application.Queries.CourseQuery;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Queries.ClassForumScoreQuery;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/cso/class-forum-score")]
    [ApiController]
    [Authorize(Roles = nameof(EnumRole.MasterAdmin))]
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
