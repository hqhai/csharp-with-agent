// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Api.Controllers
{
    using System.Net;
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Identity.Application.Queries.StudentQuery.ExportQuery;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Shared.Constants;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/student-export")]
    [ApiController]
    public class StudentExportController : ControllerBase
    {
        private readonly IMediator _mediator;

        public StudentExportController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get list user by Ids
        /// </summary>
        [HttpPost("get-by-user-ids")]
        [ProducesResponseType(typeof(MethodResult<IList<StudentModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetByIds([FromBody] IList<Guid> ids)
        {
            MethodResult<IList<StudentModel>> commandResult = await _mediator.Send(new GetStudentByUserIdsQuery { UserIds = ids }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
