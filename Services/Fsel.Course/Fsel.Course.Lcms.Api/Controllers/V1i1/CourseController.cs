// Copyright (c) Atlantic. All rights reserved.

using System.Net;
using Asp.Versioning;
using Fsel.Common.ActionResults;
using Fsel.Common.Attributes;
using Fsel.Common.Constants;
using Fsel.Course.Application.Commands.CourseCmd;
using Fsel.Course.Domain.Models.EntityModels;
using Fsel.Shared.Constants;
using Fsel.Shared.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Fsel.Course.Lcms.Api.Controllers.V1i1
{
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/course")]
    [ApiController]
    [Permission(role: nameof(EnumRole.MasterAdmin))]
    public class CourseController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CourseController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Create a Course
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<CourseModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreateCourseV1i1Command command)
        {
            MethodResult<CourseModel> queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Update a Course
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(MethodResult<CourseModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateCourseV1i1Command command)
        {
            ArgumentNullException.ThrowIfNull(command);
            command.Id = id;
            MethodResult<CourseModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
