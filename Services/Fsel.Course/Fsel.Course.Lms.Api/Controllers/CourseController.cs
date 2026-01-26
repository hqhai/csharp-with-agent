// Copyright (c) Atlantic. All rights reserved.

using System.Net;
using Asp.Versioning;
using Fsel.Common.ActionResults;
using Fsel.Common.Attributes;
using Fsel.Common.Constants;
using Fsel.Core.Base;
using Fsel.Core.Base.BaseModels;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.EntityModels;
using Fsel.Course.Lms.Application.Commands.CourseResultCmd;
using Fsel.Course.Lms.Application.Queries.CourseQuery;
using Fsel.Course.Lms.Application.Queries.CourseQuery.V1i1;
using Fsel.Shared.Attributes;
using Fsel.Shared.Constants;
using Fsel.Shared.Enums;
using Fsel.Shared.Models.ShareModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Fsel.Course.Lms.Api.Controllers
{
    using Application.Commands.CourseCmd;

    [ApiVersions(ApiSettings.APIVersion1)]
    [Route(Settings.APIDefaultRoute + "/course")]
    [ApiController]
    [Permission(roles: new string[] { nameof(EnumRole.Student), nameof(EnumRole.StudentCampus) })]
    public class CourseController : BaseController
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
        [Permission]
        public async Task<IActionResult> ExecuteList([FromBody] BaseQueryModel query)
        {
            var result = await _courseRepository.GetListResultAsync<CourseModel>(query);
            return result.GetActionResult();
        }

        /// <summary>
        /// Execute-list-query
        /// </summary>
        [HttpGet("execute-query")]
        [ProducesResponseType(typeof(MethodResult<CourseModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission]
        public async Task<IActionResult> Execute([FromQuery] BaseQueryModel query)
        {
            SetQuery(query);
            var result = await _courseRepository.GetResultAsync<CourseModel>(query);
            return result.GetActionResult();
        }

        /// <summary>
        /// Get units by course Id
        /// </summary>
        [HttpGet]
        [MapToApiVersion(ApiSettings.APIVersion1)]
        [MapToApiVersion(ApiSettings.APIVersion1i1)]
        [ProducesResponseType(typeof(MethodResult<CourseModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(roles: new string[] { nameof(EnumRole.Student), nameof(EnumRole.StudentCampus) })]
        public async Task<IActionResult> Get([FromQuery] GetCourseQuery query)
        {
            MethodResult<CourseModel> commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get Course Studying
        /// </summary>
        [HttpGet("get-course-studying")]
        [ProducesResponseType(typeof(MethodResult<CourseModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(roles: new string[] { nameof(EnumRole.Student), nameof(EnumRole.StudentCampus) })]
        public async Task<IActionResult> GetCourseStudying()
        {
            MethodResult<CourseModel> commandResult = await _mediator.Send(new GetCourseStudyingQuery()).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get Course studied
        /// </summary>
        [HttpGet("get-course-studied")]
        [ProducesResponseType(typeof(MethodResult<CourseModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(roles: new string[] { nameof(EnumRole.Student), nameof(EnumRole.StudentCampus) })]
        public async Task<IActionResult> GetCourseStudied()
        {
            MethodResult<CourseModel> commandResult = await _mediator.Send(new GetCourseStudiedQuery()).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Start Course Result
        /// </summary>
        [HttpPost("start/{courseResultId}")]
        [MapToApiVersion(ApiSettings.APIVersion1)]
        [MapToApiVersion(ApiSettings.APIVersion1i1)]
        [ProducesResponseType(typeof(MethodResult<CourseResultModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(roles: new string[] { nameof(EnumRole.Student), nameof(EnumRole.StudentCampus) })]
        public async Task<IActionResult> StartCourseResult([FromRoute] Guid courseResultId)
        {
            MethodResult<CourseResultModel> commandResult = await _mediator.Send(new StartCourseResultCommand { CourseResultId = courseResultId }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// get course for choose level
        /// </summary>
        [HttpGet("get-course-for-choose-level")]
        [ProducesResponseType(typeof(MethodResult<CourseForChooseLevelModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(roles: new string[] { nameof(EnumRole.Student), nameof(EnumRole.StudentCampus) })]
        public async Task<IActionResult> GetCourseForChooseLevel([FromQuery] GetCourseForChooseLevelQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        [HttpPost("select-course-by-choose-level")]
        [Permission(role: nameof(EnumRole.Student))]
        public async Task<IActionResult> GetLevelsBySelectedProgram([FromBody] ChooseCourseByLevelCommand request)
        {
            var queryResult = await _mediator.Send(request).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        [HttpPost("change-subject")]
        [Permission(role: nameof(EnumRole.Student))]
        public async Task<IActionResult> ChangeSubject([FromBody] ChangeSubjectCommand request)
        {
            var queryResult = await _mediator.Send(request).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }


        [HttpGet("get-levels-for-change")]
        [Permission(role: nameof(EnumRole.Student))]
        public async Task<IActionResult> GetLevelsForChange()
        {
            var queryResult = await _mediator.Send(new GetLevelsForChangeQuery()).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
