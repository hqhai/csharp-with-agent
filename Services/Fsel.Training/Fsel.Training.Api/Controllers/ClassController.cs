// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Api.Controllers
{
    using System.Collections.Generic;
    using System.Net;
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels;
    using Fsel.Training.Application.Commands.ClassCmd;
    using Fsel.Training.Application.Commands.ClassLiveCmd;
    using Fsel.Training.Application.Commands.ClassStudentCmd;
    using Fsel.Training.Application.Queries.Admins;
    using Fsel.Training.Application.Queries.ClassQuery;
    using Fsel.Training.Application.Queries.ClassQuery.Admin;
    using Fsel.Training.Application.Queries.ClassStudentQuery;
    using Fsel.Training.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/class")]
    [ApiController]
    public class ClassController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ClassController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// get class list status new
        /// </summary>
        [HttpPost("get-class-list-status-new")]
        [ProducesResponseType(typeof(MethodResult<IList<CourseClassModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetClassListStatusNew([FromBody] GetClassByStatusNewQuery query)
        {
            MethodResult<IList<CourseClassModel>> commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// get class list status new
        /// </summary>
        [HttpGet("get-class-by-student/{studentId}")]
        [ProducesResponseType(typeof(MethodResult<ClassModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetClassByStudentId([FromRoute] Guid studentId)
        {
            MethodResult<ClassModel> commandResult = await _mediator.Send(new GetClassByStudentIdQuery { StudentId = studentId }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// get class list status new
        /// </summary>
        [HttpGet("get-class-to-student/{studentId}")]
        [ProducesResponseType(typeof(MethodResult<ClassModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetClassToStudentId([FromRoute] Guid studentId)
        {
            MethodResult<ClassModel> commandResult = await _mediator.Send(new GetClassToStudentIdQuery { StudentId = studentId }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// get new class code
        /// </summary>
        [HttpGet("get-new-class-code")]
        [ProducesResponseType(typeof(MethodResult<string>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetNewClassCode([FromQuery] GetNewClassCodeQuery query)
        {
            MethodResult<string> commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Create a class
        /// </summary>
        [HttpPost("register-class")]
        [ProducesResponseType(typeof(MethodResult<ClassModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> RegisterClass([FromBody] RegisterClassCommand command)
        {
            var queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Search Class Forum
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<ClassModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Search([FromQuery] Application.Queries.ClassQuery.SearchClassQuery query)
        {
            MethodResult<PagingItemsModel<ClassModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// get class list status new
        /// </summary>
        [HttpDelete("delete-student-from-class/{id}")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> DeleteStudentFromClass([FromRoute] Guid id)
        {
            MethodResult<bool> commandResult = await _mediator.Send(new DeleteStudentFromClassCommand { UserId = id }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// get classid by studentid
        /// </summary>
        [HttpGet("get-new-class-by-student-id/{id}")]
        [ProducesResponseType(typeof(MethodResult<ClassModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetClassIdByStudentId([FromRoute] Guid id)
        {
            MethodResult<ClassModel> commandResult = await _mediator.Send(new GetNewClassByStudentIdQuery { StudentId = id }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Search Class Forum
        /// </summary>
        [HttpGet("search-class")]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<ClassSearchModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SearchClass([FromQuery] Application.Queries.ClassQuery.Admin.SearchClassQuery query)
        {
            MethodResult<PagingItemsModel<ClassSearchModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Class Course by StudentId
        /// </summary>
        [HttpGet("class-course-student/{studentId}")]
        [ProducesResponseType(typeof(MethodResult<IList<ClassStudentInfoModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetClassCourseStudent([FromRoute] Guid studentId)
        {
            MethodResult<IList<ClassStudentInfoModel>> queryResult = await _mediator.Send(new GetClassStudentInfoQuery { StudentId = studentId }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Class by StudentId
        /// </summary>
        [HttpGet("get-classes-by-csoId/{csoId}")]
        [ProducesResponseType(typeof(MethodResult<IList<ClassModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetClassesByCsoId([FromRoute] Guid csoId)
        {
            MethodResult<IList<ClassModel>> queryResult = await _mediator.Send(new GetClassesByCsoIdQuery { CsoId = csoId }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Class Course by StudentId
        /// </summary>
        [HttpGet("get-classes/{studentId}")]
        [ProducesResponseType(typeof(MethodResult<IList<ClassModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetListClassByStudentId([FromRoute] Guid studentId)
        {
            MethodResult<IList<ClassModel>> queryResult = await _mediator.Send(new GetListClassByStudentIdQuery { StudentId = studentId }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Class Course by StudentId
        /// </summary>
        [HttpPost("classes-by-studentids/diffirent-course")]
        [ProducesResponseType(typeof(MethodResult<IList<ClassModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetListClassByStudentIds([FromBody] GetListClassBySpecificStudentIdsQuery query)
        {
            MethodResult<IList<CompetitionClassStudentModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Classes by StudentIds
        /// </summary>
        [HttpPost("classes-by-studentids")]
        [ProducesResponseType(typeof(MethodResult<IList<ClassStudentDetailModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetClassByStudentIds([FromBody] GetListClassByStudentIdsQuery query)
        {
            MethodResult<IList<ClassStudentDetailModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Approve teacher
        /// </summary>
        [HttpPut("approve-auto")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ApproveAuto()
        {
            var queryResult = await _mediator.Send(new UpdateClassLiveAssignmentCommand()).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Add student into class
        /// </summary>
        [HttpPost("add-student-into-class")]
        [ProducesResponseType(typeof(MethodResult<Guid>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> AddStudentIntoClass([FromBody] AddStudentIntoClassCommand command)
        {
            var queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Add student into class
        /// </summary>
        [HttpPost("choose-level-by-student")]
        [ProducesResponseType(typeof(MethodResult<Guid>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ChooseLevelByStudent([FromBody] ChooseLevelByStudentCommand command)
        {
            var queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get List Classes To Course Ids
        /// </summary>
        [HttpGet("gets-by-course-ids")]
        [ProducesResponseType(typeof(MethodResult<IList<ClassModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetsByCourseIds([FromQuery] GetListClassByCourseIdsQuery query)
        {
            MethodResult<IList<ClassModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get students in 7 day choose level
        /// </summary>
        [HttpGet("get-students-in-7-day-choose-level")]
        [ProducesResponseType(typeof(MethodResult<IList<StudentsIn7DayChooseLevelModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetStudentsIn7DayChooseLevel()
        {
            var queryResult = await _mediator.Send(new GetStudentsIn7DayChooseLevelQuery()).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
