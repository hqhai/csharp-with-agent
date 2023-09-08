// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers.Admin
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Queries.Students;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/admin/lesson")]
    [ApiController]
    public class LessonController : ControllerBase
    {
        private readonly IMediator _mediator;

        public LessonController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get LessonComment By Student
        /// </summary>
        [HttpGet("{studentId}")]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<CourseSearchModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get([FromRoute] Guid studentId)
        {
            MethodResult<IList<LessonCommentByStudentModel>> queryResult = await _mediator.Send(new GetLessonCommentByStudentQuery { StudentId = studentId }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
