// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Commands.ExtraPracticeCmd;
    using Fsel.Course.Lms.Application.Queries.ExtraPracticeQuery;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(Settings.APIVersion)]
    [Route(Settings.APIDefaultRoute + "/extraPractice")]
    [ApiController]
    [Authorize(Roles = nameof(EnumRole.Student))]
    public class ExtraPracticeController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ExtraPracticeController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get Filter
        /// </summary>
        [HttpGet("level-units")]
        [ProducesResponseType(typeof(MethodResult<object>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetLevelByUnits()
        {
            MethodResult<object> queryResult = await _mediator.Send(new GetListLevelByUnitQuery()).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get And Start ExtraPractice
        /// </summary>
        [HttpGet("start-extraPractice/{id}")]
        [ProducesResponseType(typeof(MethodResult<ExtraPracticeModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetAndStartExtraPractice([FromRoute] Guid id)
        {
            MethodResult<ExtraPracticeModel> queryResult = await _mediator.Send(new StartExtraPracticeCommand { ExtraPracticeId = id }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Search ExtraPractice
        /// </summary>
        [HttpGet()]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<ExtraPracticeSearchModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Search([FromQuery] SearchExtraPracticeQuery query)
        {
            MethodResult<PagingItemsModel<ExtraPracticeSearchModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Create ExtraPractice Answer
        /// </summary>
        [HttpPost("create-extraPractice-answer")]
        [ProducesResponseType(typeof(MethodResult<ExtraPracticeExerciseResultModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreateAnswer([FromBody] CreateExtraPracticeAnswerCommand command)
        {
            MethodResult<ExtraPracticeExerciseResultModel> queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
