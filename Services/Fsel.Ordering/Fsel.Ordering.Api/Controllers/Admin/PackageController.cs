// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Api.Controllers.Admin
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Attributes;
    using Fsel.Common.Constants;
    using Fsel.Core.Base;
    using Fsel.Ordering.Application.Commands.PackageCmds;
    using Fsel.Shared.Attributes;
    using Fsel.Shared.Constants;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersions(ApiSettings.APIVersion1)]
    [Route(Settings.APIDefaultRoute + "/admin/package")]
    [ApiController]
    public class PackageController : BaseController
    {
        private readonly IMediator _mediator;

        public PackageController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Create Order for student
        /// </summary>
        [HttpPost("save-packages")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(new[] { PriceManagement.AddPackage, PriceManagement.UpdatePackage })]
        public async Task<IActionResult> SavePackages([FromBody] SavePackagesCommand command)
        {
            var result = await _mediator.Send(command).ConfigureAwait(false);
            return result.GetActionResult();
        }
    }
}
