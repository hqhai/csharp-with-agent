// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Api.Controllers
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Attributes;
    using Fsel.Common.Constants;
    using Fsel.Core.Base.BaseModels;
    using Fsel.System.Application.Commands.CourseTimeConfigCmd;
    using Fsel.System.Application.Querys.CourseTimeConfigQuery;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using global::System.Net;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using Asp.Versioning;
    using Fsel.Shared.Constants;

    [ApiVersion(ApiSettings.APIVersion1)][ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/course-time-config")]
    [ApiController]
    public class CourseTimeConfigController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ICourseTimeConfigRepository _courseTimeConfigRepository;

        public CourseTimeConfigController(IMediator mediator, ICourseTimeConfigRepository courseTimeConfigRepository)
        {
            _mediator = mediator;
            _courseTimeConfigRepository = courseTimeConfigRepository;
        }


        /// <summary>
        /// Execute-list-query
        /// </summary>
        [HttpPost("execute-list-query")]
        [ProducesResponseType(typeof(MethodResult<IList<CourseTimeConfigModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission]
        public async Task<IActionResult> ExecuteList([FromBody] BaseQueryModel query)
        {
            var result = await _courseTimeConfigRepository.GetListResultAsync<CourseTimeConfigModel>(query);
            return result.GetActionResult();
        }

        /// <summary>
        /// get list course time config
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<CourseTimeConfigModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Search([FromQuery] GetListCourseTimeConfigQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Save course time config
        /// </summary>
        [HttpPost("save-course-time-config")]
        [ProducesResponseType(typeof(MethodResult<CourseTimeConfigModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SaveList([FromBody] SetMonthToClassCommand command)
        {
            MethodResult<CourseTimeConfigModel> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Update course time config
        /// </summary>
        [HttpPut]
        [ProducesResponseType(typeof(MethodResult<IList<CourseTimeConfigModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Update([FromBody] SetEnrollmentWeekToClassCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);
            MethodResult<IList<CourseTimeConfigModel>> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get List Courses time by course ids
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<IList<CourseTimeConfigModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetCoursesByIds([FromBody] IList<Guid> courseIds)
        {
            MethodResult<IList<CourseTimeConfigModel>> queryResult = await _mediator.Send(new GetCourseTimeConfigByListCourseIdQuery { CourseIds = courseIds }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
