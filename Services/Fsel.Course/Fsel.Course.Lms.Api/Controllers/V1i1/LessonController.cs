// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers.V1i1
{
    using System.Net;
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.EntityModels.V1i1;
    using Fsel.Course.Lms.Application.Queries.V1i1.LessonQuery;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/lesson")]
    [ApiController]
    [Common.Attributes.Permission(role: nameof(EnumRole.Student))]
    public class LessonController : ControllerBase
    {
        private readonly IMediator _mediator;

        public LessonController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get List Lesson
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<IList<LessonMockTestResultModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetLessons([FromQuery] GetLessonsQuery query)
        {
            MethodResult<IList<LessonMockTestResultModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get Lesson Detail
        /// </summary>
        [HttpGet("detail")]
        [ProducesResponseType(typeof(MethodResult<LessonModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get([FromQuery] GetLessonQuery query)
        {
            MethodResult<LessonModel> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get Lesson Report
        /// </summary>
        [HttpGet("lesson-report")]
        [ProducesResponseType(typeof(MethodResult<LessonReportModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetLessonReport([FromQuery] GetLessonReportQuery query)
        {
            MethodResult<LessonReportModel> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
