// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lcms.Api.Controllers.V1i1
{
    using System.Net;
    using System.Threading.Tasks;
    using Application.Queries.LevelQuery;
    using Asp.Versioning;
    using Domain.Models.EntityModels;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Common.Models;
    using Fsel.Course.Application.Queries.CategoryQuery.V1i1;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/category")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CategoryController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Search Course Source Data
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<IList<EnumModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetEnumCourseSourceDatasAsync([FromQuery] EnumCourseSourceData courseSource)
        {
            var queryResult = await _mediator.Send(new GetEnumQuery { EnumCourseSourceData = courseSource }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        [HttpGet("all-course-level")]
        [ProducesResponseType(typeof(MethodResult<MethodResult<IList<SubjectModel>>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetLevelInCategories()
        {
            var queryResult = await _mediator.Send(new GetLevelInCategoryQuery()).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
