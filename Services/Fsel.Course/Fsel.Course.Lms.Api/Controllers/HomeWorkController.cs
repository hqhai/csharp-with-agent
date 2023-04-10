// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers
{
    using Fsel.Common.ActionResults;
    using System.Net;
    using Fsel.Common.Constants;
    using Fsel.Common.Enums;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Fsel.Course.Lms.Application.Queries.HomeWorkQuery;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/home-work")]
    [ApiController]
    [Authorize(Roles = nameof(EnumRole.Student))]
    public class HomeWorkController
    {
        private readonly IMediator _mediator;

        public HomeWorkController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get Home Work
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(MethodResult<HomeWorkModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get([FromRoute] Guid id)
        {
            MethodResult<HomeWorkModel> queryResult = await _mediator.Send(new GetHomeWorkQuery { HomeWorkId = id }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
