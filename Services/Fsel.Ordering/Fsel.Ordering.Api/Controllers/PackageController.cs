// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Api.Controllers
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Ordering.Application.Queries.PackageQuery;
    using Fsel.Ordering.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/package")]
    [ApiController]
    [Authorize(Roles = nameof(EnumRole.Student))]
    public class PackageController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PackageController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get Order Ramdom
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<List<PackageModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetOrderByPackageId()
        {
            MethodResult<List<PackageModel>> commandResult = await _mediator.Send(new GetPackagesQuery()).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
