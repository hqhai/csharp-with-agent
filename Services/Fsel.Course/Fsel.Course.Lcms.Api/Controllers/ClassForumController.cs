// Copyright (c) Atlantic. All rights reserved.
using Fsel.Common.Constants;
using Fsel.Shared.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using Fsel.Shared.Constants;

namespace Fsel.Course.Lcms.Api.Controllers
{
    [ApiVersion(ApiSettings.APIVersion1)][ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/class-forum")]
    [ApiController]
    [Common.Attributes.Permission(role: nameof(EnumRole.MasterAdmin))]
    public class ClassForumController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ClassForumController(IMediator mediator)
        {
            _mediator = mediator;
        }
    }
}
