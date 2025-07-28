// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lcms.Api.Controllers
{
    using System.Net;
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Course.Application.Queries.LevelQuery;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/level")]
    [ApiController]
    [Common.Attributes.Permission(role: nameof(EnumRole.MasterAdmin))]
    public class LevelController : ControllerBase
    {
        private readonly IMediator _mediator;

        public LevelController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get Level By Program
        /// </summary>
        [HttpGet("{programId}")]
        [ProducesResponseType(typeof(MethodResult<IList<LevelModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get([FromRoute] Guid programId)
        {
            MethodResult<IList<LevelModel>> queryResult = await _mediator.Send(new GetLevelByProgramIdQuery { ProgramId = programId }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get Level By Category
        /// </summary>
        [HttpGet("category/{categoryId}")]
        [ProducesResponseType(typeof(MethodResult<IList<LevelModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetLevelByCategory([FromRoute] Guid categoryId)
        {
            MethodResult<IList<LevelModel>> queryResult = await _mediator.Send(new GetLevelByCategoryIdQuery { CategoryId = categoryId }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
