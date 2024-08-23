// Copyright (c) Atlantic. All rights reserved.

using System.Net;
using Fsel.Common.ActionResults;
using Fsel.Common.Constants;
using Fsel.Core.Base.Interfaces;
using Fsel.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Fsel.Shared.Attributes;

namespace Fsel.Course.Lms.Api.Controllers.V1i1
{
    [ApiVersions(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/test")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IQueueProvider _queueProvider;

        public TestController(IMediator mediator, IQueueProvider queueProvider)
        {
            _mediator = mediator;
            _queueProvider = queueProvider;
        }

        /// <summary>
        /// Search Course
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<string>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public IActionResult Search()
        {
            var a = TimeZoneInfo.GetSystemTimeZones();
            MethodResult<string> queryResult = new MethodResult<string> { Result = nameof(Search) };
            return queryResult.GetActionResult();
        }
    }
}
