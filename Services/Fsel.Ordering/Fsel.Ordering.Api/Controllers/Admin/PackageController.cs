// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Api.Controllers.Admin
{
    using System.Net;
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Ordering.Application.Commands.PackageCmds;
    using Fsel.Shared.Bases.V1;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [Route(Settings.APIDefaultRoute + "/admin/package")]
    [Common.Attributes.Permission(role: nameof(EnumRole.Admin))]
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
        public async Task<IActionResult> SavePackages([FromBody] SavePackagesCommand command)
        {
            var result = await _mediator.Send(command).ConfigureAwait(false);
            return result.GetActionResult();
        }
    }
}
