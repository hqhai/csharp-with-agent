// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers.Campus
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Attributes;
    using Fsel.Common.Constants;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Commands.CurriculumCmd;
    using Fsel.Course.Lms.Application.Queries.CourseQuery.V1i1;
    using Fsel.Course.Lms.Application.Queries.CurriculumQuery;
    using Fsel.Shared.Attributes;
    using Fsel.Shared.Constants;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using Refit;

    [ApiVersions(ApiSettings.APIVersion1)]
    [Route(Settings.APIDefaultRoute + "/curriculum")]
    [ApiController]
    public class CurriculumController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CurriculumController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Create curriculum
        /// </summary>
        [HttpPost("create")]
        [ProducesResponseType(typeof(MethodResult<CurriculumModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(CurriculumManagement.Add)]
        public async Task<IActionResult> CreateCurriculum([FromBody] CreateCurriculumCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Update curriculum
        /// </summary>
        [HttpPost("update")]
        [ProducesResponseType(typeof(MethodResult<CurriculumModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(CurriculumManagement.Update)]
        public async Task<IActionResult> UpdateCurriculum([FromBody] UpdateCurriculumCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Delete curriculum
        /// </summary>
        [HttpPost("delete")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(CurriculumManagement.Delete)]
        public async Task<IActionResult> DeleteCurriculum([FromBody] DeleteCurriculumCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Delete curriculum
        /// </summary>
        [HttpPost("delete-students")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(CurriculumManagement.DeleteStudents)]
        public async Task<IActionResult> DeleteStudentsFromCurriculum([FromBody] DeleteStudentsFromCurriculumCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// add students to curriculum
        /// </summary>
        [HttpPost("add-students-to-curriculum")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(CurriculumManagement.AddStudents)]
        public async Task<IActionResult> AddStudents([FromBody] AddStudentsToCurriculumCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// search
        /// </summary>
        [HttpGet("search")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(CurriculumManagement.View)]
        public async Task<IActionResult> Search([FromQuery] SearchCurriculumQuery query)
        {
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// get by id
        /// </summary>
        [HttpGet("get-by-id")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(CurriculumManagement.View)]
        public async Task<IActionResult> GetById([FromQuery] GetCurriculumByIdQuery query)
        {
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// search students in curriculum
        /// </summary>
        [HttpGet("search-students-in-curriculum")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(CurriculumManagement.View)]
        public async Task<IActionResult> SearchStudents([FromQuery] SearchStudentsByCurriculumIdQuery query)
        {
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// get students learning progress
        /// </summary>
        [HttpPost("get-students-learning-progress")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(CurriculumManagement.View)]
        public async Task<IActionResult> GetStudentsLearningProgress([FromBody] GetStudentsLearningProgressQuery query)
        {
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// delete curriculums by student ids
        /// </summary>
        [HttpPost("delete-curriculums-by-student-ids")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(CurriculumManagement.Delete)]
        public async Task<IActionResult> DeleteCurriculumsByStudentIds([FromBody] DeleteCurriculumsByStudentIdsCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
