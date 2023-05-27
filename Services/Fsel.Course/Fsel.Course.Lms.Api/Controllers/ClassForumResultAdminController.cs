// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers
{
    using Fsel.Common.Constants;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/admin/class-forum-result")]
    [ApiController]
    [Authorize(Roles = nameof(EnumRole.Admin))]
    public class ClassForumResultAdminController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ClassForumResultAdminController(IMediator mediator)
        {
            _mediator = mediator;
        }
    }
}
