// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers.Teacher
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Queries.UnitQuery;
    using Fsel.Shared.Enums;
    using MediatR;
    using Asp.Versioning;
    using Fsel.Shared.Constants;
    using Microsoft.AspNetCore.Mvc;
    using System.Net;

    [ApiVersion(ApiSettings.APIVersion1)][ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/teacher/unit")]
    [ApiController]
    [Common.Attributes.Permission(role: nameof(EnumRole.Teacher))]
    public class UnitController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UnitController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get course
        /// </summary>
        [HttpGet("unit-display-order")]
        [ProducesResponseType(typeof(MethodResult<IList<UnitModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetListUnitByCourse([FromQuery] GetListUnitByCourseIdQuery query)
        {
            MethodResult<IList<UnitModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
