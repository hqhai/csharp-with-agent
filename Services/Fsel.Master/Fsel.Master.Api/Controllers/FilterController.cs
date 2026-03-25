// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Master.Api.Controllers
{
    using System.Net;
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Master.Application.Queries.FilterQuery;
    using Fsel.Shared.Constants;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/filter")]
    [ApiController]
    public class FilterController : ControllerBase
    {
        private readonly IMediator _mediator;

        public FilterController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("get-subjects")]
        [ProducesResponseType(typeof(MethodResult<IList<SubjectModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetSubjects([FromQuery] GetSubjectsQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
