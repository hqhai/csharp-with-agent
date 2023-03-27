// Copyright (c) Atlantic. All rights reserved.

using System.Net;
using Fsel.Common.ActionResults;
using Fsel.Common.Constants;
using Fsel.Common.Enums;
using Fsel.Course.Domain.Models.EntityModels;
using Fsel.Course.Lms.Application.Commands.CourseCmd;
using Fsel.Course.Lms.Application.Queries.CourseQuery;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fsel.Course.Lms.Api.Controllers
{
    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/test")]
    [ApiController]
    [Authorize(Roles = nameof(EnumRole.Student))]
    public class TestController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TestController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Search Course
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<string>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public IActionResult Search()
        {
            MethodResult<string> queryResult = new MethodResult<string> { Result = nameof(Search) };
            return queryResult.GetActionResult();
        }
    }
}
