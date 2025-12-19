// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers.Campus
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Attributes;
    using Fsel.Common.Constants;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Queries.CourseQuery.V1i1;
    using Fsel.Course.Lms.Application.Queries.HomeWorkConfigQuery;
    using Fsel.Course.Lms.Application.Queries.HomeWorkQuery;
    using Fsel.Shared.Attributes;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersions(ApiSettings.APIVersion1)]
    [Route(Settings.APIDefaultRoute + "/curriculum/course")]
    [ApiController]
    public class CourseController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CourseController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// search course
        /// </summary>
        [HttpGet("search-course")]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<CourseSearchModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(CurriculumManagement.View)]
        public async Task<IActionResult> SearchCourse([FromQuery] SearchCourseQuery query)
        {
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// search
        /// </summary>
        [HttpGet("search-homework")]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<HomeWorkSearchModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(CurriculumManagement.View)]
        public async Task<IActionResult> SearchHomeWork([FromQuery] SearchHomeWorkForCurriculumQuery query)
        {
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// search create users info homeWork
        /// </summary>
        [HttpGet("search-created-users-info-homework")]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<EntityModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(CurriculumManagement.View)]
        public async Task<IActionResult> SearchCreateUsersInfoHomeWork([FromQuery] SearchCreateUsersInfoHomeWorkQuery query)
        {
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
