// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Api.Controllers.Campus
{
    using System.Net;
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Attributes;
    using Fsel.Common.Constants;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Identity.Application.Commands.Campus;
    using Fsel.Identity.Application.Commands.StudentCmd;
    using Fsel.Identity.Application.Queries.CampusQuery;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels.CampusModel;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/campus")]
    [ApiController]
    public class CampusController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CampusController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Update expired date for students
        /// </summary>
        [HttpPost("update-expired-date-for-students")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(CurriculumManagement.Update)]
        public async Task<IActionResult> UpdateExpiredDateForStudentsCampus([FromBody] UpdateExpiredDateForStudentsCampusCommand command)
        {
            var queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Search students by student ids
        /// </summary>
        [HttpPost("search-students-by-student-ids")]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<StudentCampusModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(CurriculumManagement.View)]
        public async Task<IActionResult> SearchStudentByStudentIds([FromBody] SearchStudentsByStudentIdsQuery command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Add course id for student
        /// </summary>
        [HttpPost("add-course-id-for-students")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(CurriculumManagement.Update)]
        public async Task<IActionResult> AddCourseIdForStudents([FromBody] AddCourseIdForStudentsCampusCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
