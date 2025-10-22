// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Queries.NavigationCmd;
    using Fsel.Shared.Attributes;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersions(ApiSettings.APIVersion1)]
    [Route(Settings.APIDefaultRoute + "/navigation-module")]
    [ApiController]
   [Common.Attributes.Permission(roles: new string[] { nameof(EnumRole.Student), nameof(EnumRole.StudentCampus) })]
    public class NavigationModuleController : ControllerBase
    {
        private readonly IMediator _mediator;

        public NavigationModuleController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>s
        /// Get Navigation to complete the lesson
        /// </summary>
        [HttpGet("{courseId}")]
        [ProducesResponseType(typeof(MethodResult<ModuleNavigationModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetNavigationCompleteTheLesson([FromRoute] Guid courseId)
        {
            MethodResult<ModuleNavigationModel> queryResult = await _mediator.Send(new GetNavigationModuleQuery { CourseId = courseId }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
