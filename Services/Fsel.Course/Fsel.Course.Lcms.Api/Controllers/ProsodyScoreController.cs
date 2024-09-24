// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using Asp.Versioning;
    using Fsel.Shared.Constants;
    using Fsel.Course.Application.Commands.ProsodyCmd;
    using Fsel.Course.Application.Queries.ProsodyQuery;
    using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/prosody-range")]
    [ApiController]
    [Common.Attributes.Permission(role: nameof(EnumRole.MasterAdmin))]
    public class ProsodyScoreController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProsodyScoreController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// get Class forum
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<IList<ProsodyScoreModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get()
        {
            MethodResult<IList<ProsodyScoreModel>> queryResult = await _mediator.Send(new GetProsodyQuery()).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }


        /// <summary>
        /// get Class forum
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<IList<ProsodyScoreModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Save([FromBody] CreateProsodyRangeCommand cmd)
        {
            MethodResult<IList<ProsodyScoreModel>> queryResult = await _mediator.Send(cmd).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }


        /// <summary>
        /// get Class forum
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(MethodResult<ProsodyScoreModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateProsodyScoreCommand cmd)
        {
            ArgumentNullException.ThrowIfNull(cmd);
            cmd.Id = id;
            MethodResult<ProsodyScoreModel> queryResult = await _mediator.Send(cmd).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
