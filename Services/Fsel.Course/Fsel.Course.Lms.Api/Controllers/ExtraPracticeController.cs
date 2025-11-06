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
    using Microsoft.AspNetCore.Mvc;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Attributes;

    [ApiVersions(ApiSettings.APIVersion1)]
    [Route(Settings.APIDefaultRoute + "/extraPractice")]
    [ApiController]
    [Common.Attributes.Permission(roles: new string[] { nameof(EnumRole.Student), nameof(EnumRole.StudentCampus) })]
    public class ExtraPracticeController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ExtraPracticeController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Search ExtraPractice
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<ExtraPracticeSearchModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Search([FromQuery] SearchExtraPracticeQuery query)
        {
            MethodResult<PagingItemsModel<ExtraPracticeSearchModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Search ExtraPractice Suggest
        /// </summary>
        [HttpGet("suggest")]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<ExtraPracticeSearchModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SearchSuggest([FromQuery] SearchSuggestExtraPracticeQuery query)
        {
            MethodResult<PagingItemsModel<ExtraPracticeSuggestModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
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
        [HttpGet("start-extra-practice/{id}")]
        [ProducesResponseType(typeof(MethodResult<ExtraPracticeResultModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetAndStartExtraPractice([FromRoute] Guid id)
        {
            MethodResult<ExtraPracticeResultModel> queryResult = await _mediator.Send(new StartExtraPracticeCommand { ExtraPracticeId = id }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get ExtraPractice Detail
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(MethodResult<ExtraPracticeModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetExtraPracticeDetail([FromRoute] Guid id)
        {
            MethodResult<ExtraPracticeModel> queryResult = await _mediator.Send(new GetExtraPracticeDetailQuery { ExtraPracticeId = id }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get ExtraPractice Detail MockTest
        /// </summary>
        [HttpGet("get-detail-mock-test")]
        [ProducesResponseType(typeof(MethodResult<ExtraPracticeModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetExtraPracticeDetailMockTest([FromQuery] GetMockTestByExtraPracticeQuery query)
        {
            MethodResult<ExtraPracticeModel> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get ExtraPractice Detail PlacementTest
        /// </summary>
        [HttpGet("get-detail-placement-test")]
        [ProducesResponseType(typeof(MethodResult<ExtraPracticeModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetExtraPracticeDetailPlacementTest([FromQuery] GetPlacementTestByExtraPracticeQuery query)
        {
            MethodResult<ExtraPracticeModel> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get TimeCodes Detail by ExtraPractice
        /// </summary>
        [HttpGet("get-detail-time-code")]
        [ProducesResponseType(typeof(MethodResult<VideoTimeCodeModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetTimeCodeByExtraPractice([FromQuery] GetTimeCodeDetailByExtraPracticeQuery query)
        {
            MethodResult<VideoTimeCodeModel> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get Exercise By ExtraPractice
        /// </summary>
        [HttpGet("get-exercises")]
        [ProducesResponseType(typeof(MethodResult<IList<ExtraPracticeExerciseModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetExerciseByExtraPractice([FromQuery] GetExerciseByExtraPracticeQuery query)
        {
            MethodResult<IList<ExtraPracticeExerciseModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get ExtraPractice Detail Questions
        /// </summary>
        [HttpGet("get-questions")]
        [ProducesResponseType(typeof(MethodResult<ExerciseModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetExtraPracticeDetailQuestions([FromQuery] GetQuestionByExtraPracticeQuery query)
        {
            MethodResult<ExerciseModel> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get ExtraPractice Detail Questions
        /// </summary>
        [HttpGet("report-test")]
        [ProducesResponseType(typeof(MethodResult<ExtraPracticeResultModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetReportTest([FromQuery] GetReportTestExtraPracticeQuery query)
        {
            MethodResult<ExtraPracticeResultModel> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Create ExtraPractice Answer Book
        /// </summary>
        [HttpPost("create-book-video-embed-answer")]
        [ProducesResponseType(typeof(MethodResult<ExtraPracticeExerciseResultModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreateAnswerBook([FromBody] CreateExtraPracticeAnswerBookCommand command)
        {
            MethodResult<ExtraPracticeExerciseResultModel> queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Create ExtraPractice Answer MockTest
        /// </summary>
        [HttpPost("create-mock-test-answer")]
        [ProducesResponseType(typeof(MethodResult<ExtraPracticeResultModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreateAnswerMockTest([FromBody] CreateExtraPracticeAnswerMockTestCommand command)
        {
            MethodResult<ExtraPracticeResultModel> queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Create ExtraPractice Answer PlacementTest
        /// </summary>
        [HttpPost("create-placement-test-answer")]
        [ProducesResponseType(typeof(MethodResult<ExtraPracticeResultModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreateAnswerPlacementTest([FromBody] CreateExtraPracticeAnswerPlacementTestCommand command)
        {
            MethodResult<ExtraPracticeResultModel> queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Create ExtraPractice Answer Video
        /// </summary>
        [HttpPost("create-time-code-video-answer")]
        [ProducesResponseType(typeof(MethodResult<ExtraPracticeResultModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreateAnswerVideo([FromBody] CreateExtraPracticeAnswerVideoCommand command)
        {
            MethodResult<ExtraPracticeResultModel> queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Restart ExtraPractice Answer
        /// </summary>
        [HttpPost("restart-answer")]
        [ProducesResponseType(typeof(MethodResult<ExtraPracticeResultModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> RestartAnswer([FromQuery] Guid extraPracticeResultId)
        {
            MethodResult<ExtraPracticeResultModel> queryResult = await _mediator.Send(new RestartExtraPracticeAnswerCommand { ExtraPracticeResultId = extraPracticeResultId }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
