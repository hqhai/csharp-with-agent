// Copyright (c) Atlantic. All rights reserved.

using System.Net;
using Fsel.Common.ActionResults;
using Fsel.Common.Attributes;
using Fsel.Common.Constants;
using Fsel.Core.Base.BaseModels;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.EntityModels;
using Fsel.Course.Lms.Application.Commands.CourseResultCmd;
using Fsel.Course.Lms.Application.Queries.CourseQuery;
using Fsel.Shared.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Fsel.Course.Lms.Api.Controllers
{
    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/course")]
    [ApiController]
    //[Permission(role: nameof(EnumRole.Student))]
    public class CourseController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ICourseRepository _courseRepository;

        public CourseController(IMediator mediator, ICourseRepository courseRepository)
        {
            _mediator = mediator;
            _courseRepository = courseRepository;
        }

        /// <summary>
        /// Execute-list-query
        /// </summary>
        [HttpPost("execute-list-query")]
        [ProducesResponseType(typeof(MethodResult<IList<CourseModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        //[Permission]
        public async Task<IActionResult> ExecuteList([FromBody] BaseQueryModel query)
        {
            var result = await _courseRepository.GetListResultAsync<CourseModel>(query);
            return result.GetActionResult();
        }

        /// <summary>
        /// Execute-list-query
        /// </summary>
        [HttpPost("execute-query")]
        [ProducesResponseType(typeof(MethodResult<CourseModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        //[Permission]
        public async Task<IActionResult> Execute([FromBody] BaseQueryModel query)
        {
            var result = await _courseRepository.GetResultAsync<CourseModel>(query);
            return result.GetActionResult();
        }

        /// <summary>
        /// Get units by course Id
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<CourseModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(role: nameof(EnumRole.Student))]
        public async Task<IActionResult> Get()
        {
            MethodResult<CourseModel> commandResult = await _mediator.Send(new GetCourseQuery()).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Start Course Result
        /// </summary>
        [HttpPost("start/{courseResultId}")]
        [ProducesResponseType(typeof(MethodResult<CourseResultModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(role: nameof(EnumRole.Student))]
        public async Task<IActionResult> StartCourseResult([FromRoute] Guid courseResultId)
        {
            MethodResult<CourseResultModel> commandResult = await _mediator.Send(new StartCourseResultCommand { CourseResultId = courseResultId }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
