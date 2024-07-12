// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Api.Controllers.Cso
{
    using System.Net;
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Training.Application.Queries.ClassQuery.Cso;
    using Fsel.Training.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/cso/class")]
    [Common.Attributes.Permission(role: nameof(EnumRole.CSO))]
    [ApiController]
    public class ClassController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ClassController(IMediator mediator)
        {
            _mediator = mediator;
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
    }
}
