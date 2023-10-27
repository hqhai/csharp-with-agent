// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers.Teacher
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Commands.MockTestResultCmd;
    using Fsel.Course.Lms.Application.Queries.MockTestResultQuery;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/teacher/mock-test-result")]
    [ApiController]
    [Common.Attributes.Permission(role: nameof(EnumRole.Teacher))]
    public class MockTestResultController : ControllerBase
    {
        private readonly IMediator _mediator;

        public MockTestResultController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Search MockTest
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<MockTestResultSearchModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Search([FromQuery] SearchMockTestByTeacherQuery query)
        {
            MethodResult<PagingItemsModel<MockTestResultSearchModel>> commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get Courses
        /// </summary>
        [HttpGet("courses")]
        [ProducesResponseType(typeof(MethodResult<IList<CourseModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetCourses()
        {
            var commandResult = await _mediator.Send(new GetCoursesByMockTestResultQuery()).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Grade MockTestResu;t
        /// </summary>
        [HttpPost("grade")]
        [ProducesResponseType(typeof(MethodResult<IList<CourseModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GradeMockTest([FromBody] GradeMockTestResultCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get MockTest Detail
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(MethodResult<IList<CourseModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get([FromRoute] Guid id)
        {
            var commandResult = await _mediator.Send(new GetMockTestResultByTeacherQuery { MockTestResultId = id }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
