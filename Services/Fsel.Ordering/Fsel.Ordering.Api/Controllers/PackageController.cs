// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Api.Controllers
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Attributes;
    using Fsel.Common.Constants;
    using Fsel.Core.Base;
    using Fsel.Ordering.Application.Queries.PackageQuery;
    using Fsel.Ordering.Domain.Models.EntityModels;
    using Fsel.Shared.Attributes;
    using Fsel.Shared.Constants;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersions(ApiSettings.APIVersion1)]
    [Route(Settings.APIDefaultRoute + "/package")]
    [ApiController]
    public class PackageController : BaseController
    {
        private readonly IMediator _mediator;

        public PackageController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get Packages
        /// </summary>
        [HttpGet]
        [ServerCache(CacheSettings.TimeCache.OneMinutes)]
        [ProducesResponseType(typeof(MethodResult<List<PackageModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission]
        public async Task<IActionResult> GetPackages()
        {
            MethodResult<List<PackageModel>> commandResult = await _mediator.Send(new GetPackagesQuery()).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get Packages by status
        /// </summary>
        [HttpGet("get-by-status")]
        [ProducesResponseType(typeof(MethodResult<List<PackageModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(PriceManagement.ViewPackage)]
        public async Task<IActionResult> GetPackagesByStatus([FromQuery] GetPackageByStatusQuery query)
        {
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
