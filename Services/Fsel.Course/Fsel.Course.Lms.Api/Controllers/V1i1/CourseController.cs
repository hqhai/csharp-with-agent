// Copyright (c) Atlantic. All rights reserved.

using System.Net;
using Asp.Versioning;
using Fsel.Common.ActionResults;
using Fsel.Common.Attributes;
using Fsel.Common.Constants;
using Fsel.Core.Base;
using Fsel.Course.Domain.Models.EntityModels;
using Fsel.Course.Lms.Application.Queries.CourseQuery.V1i1;
using Fsel.Shared.Constants;
using Fsel.Shared.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Fsel.Course.Lms.Api.Controllers.V1i1
{
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/course")]
    [ApiController]
    [Permission(role: nameof(EnumRole.Student))]
    public class CourseController : BaseController
    {
        private readonly IMediator _mediator;

        public CourseController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get Manage Courses
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<IList<CourseManagerModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetManageCourses([FromQuery] GetManageCoursesQuery query)
        {
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
