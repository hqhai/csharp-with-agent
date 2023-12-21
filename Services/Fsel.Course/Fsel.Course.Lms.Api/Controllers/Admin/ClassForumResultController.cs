// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers.Admin
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Queries.ClassForumResultQuery;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using Asp.Versioning;
    using Fsel.Shared.Constants;

    [ApiVersion(ApiSettings.APIVersion1)][ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/admin/class-forum-result")]
    [ApiController]
    public class ClassForumResultController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ClassForumResultController(IMediator mediator)
        {
            _mediator = mediator;
        }



        /// <summary>
        /// Search class forum result
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<ClassForumResultSearchModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Search([FromQuery] SearchClassForumResultAdminQuery query)
        {
            MethodResult<PagingItemsModel<ClassForumResultSearchModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
