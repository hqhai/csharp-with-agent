// Copyright (c) Atlantic. All rights reserved.

using System.Net;
using Asp.Versioning;
using Fsel.Common.ActionResults;
using Fsel.Common.Attributes;
using Fsel.Common.Constants;
using Fsel.Common.Helpers;
using Fsel.Core.Base.Interfaces;
using Fsel.Course.Lms.Application.Commands.OtherFeatureCmd;
using Fsel.Course.Lms.Application.Commands.TestCmd;
using Fsel.Course.Lms.Application.Commands.VideoTimeCodeAnswerCmd;
using Fsel.Course.Lms.Application.Queries.OtherFeatureQuery;
using Fsel.Course.Lms.Application.Services.AIService.SpeakingAIService.Interface;
using Fsel.Shared.Attributes;
using Fsel.Shared.Constants;
using Fsel.Shared.Enums;
using Fsel.Shared.Models.ShareModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Fsel.Course.Lms.Api.Controllers
{
    [ApiVersions(ApiSettings.APIVersion1)]
    [Route(Settings.APIDefaultRoute + "/test")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IQueueProvider _queueProvider;
        private readonly ILogger<TestController> _logger;
        private readonly ISpeakingEvaluationAIService _speakingEvaluationAIService;

        public TestController(IMediator mediator, IQueueProvider queueProvider, ILogger<TestController> logger, ISpeakingEvaluationAIService speakingEvaluationAIService)
        {
            _mediator = mediator;
            _queueProvider = queueProvider;
            _logger = logger;
            _speakingEvaluationAIService = speakingEvaluationAIService;
        }

        /// <summary>
        /// Search Course
        /// </summary>
        [HttpGet("get-curl")]
        [ProducesResponseType(typeof(MethodResult<string>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [MapToApiVersion(ApiSettings.APIVersion1)]
        public IActionResult GetCurl()
        {
            _logger.LogError(Request.HttpContext.ToCurl());
            MethodResult<string> queryResult = new MethodResult<string> { Result = nameof(Search) };
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Search Course
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<string>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [MapToApiVersion(ApiSettings.APIVersion1)]
        public IActionResult Search()
        {
            var a = TimeZoneInfo.GetSystemTimeZones();
            MethodResult<string> queryResult = new MethodResult<string> { Result = nameof(Search) };
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Delete Video Time Code Answers
        /// </summary>
        [HttpPut("delete-video-time-code-answers")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Delete([FromQuery] DeleteVideoTimeCodeAnswersCommand command)
        {
            MethodResult<bool> queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Delete Video Time Code Answers
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Post()
        {
            await _queueProvider.Publish(QueueSettings.RealtimeQueue.NameQueue.DiscussionBoard, new DiscussionBoardQueueModel
            {
                ObjectId = Guid.NewGuid(),
            }, CancellationToken.None);

            MethodResult<bool> queryResult = new MethodResult<bool>();
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Update Module Process
        /// </summary>
        [HttpPut("module-process")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Common.Attributes.Permission(role: nameof(EnumRole.Admin))]
        public async Task<IActionResult> UpdateModuleProcess([FromBody] UpdateModuleProcessCommand command)
        {
            MethodResult<bool> queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Import Module Process
        /// </summary>
        [HttpPost("import-module-process")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Common.Attributes.Permission(role: nameof(EnumRole.Admin))]
        public async Task<IActionResult> ImportModuleProcess([FromForm] ImportUpdateModuleProgressCommand command)
        {
            MethodResult<Stream> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            if (!commandResult.IsOK || commandResult.Result == null)
            {
                return commandResult.GetActionResult();
            }
            return File(commandResult.Result, Settings.Excels.ContentType, "Export_File_Error.xlsx");
        }

        /// <summary>
        /// Export Module Process
        /// </summary>
        [HttpPost("export-template-module-process")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Common.Attributes.Permission(role: nameof(EnumRole.Admin))]
        public async Task<IActionResult> ExportModuleProcess()
        {
            MethodResult<Stream> commandResult = await _mediator.Send(new ExportFileTemplateUpdateModuleCommand()).ConfigureAwait(false);
            if (!commandResult.IsOK || commandResult.Result == null)
            {
                return commandResult.GetActionResult();
            }
            return File(commandResult.Result, Settings.Excels.ContentType, "Export_File_Template_ModuleProgess.xlsx");
        }

        /// <summary>
        /// Update Module Process
        /// </summary>
        [HttpPost("overall-score")]
        [ProducesResponseType(typeof(MethodResult<double>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Common.Attributes.Permission(role: nameof(EnumRole.Admin))]
        public async Task<IActionResult> GetOverall([FromBody] ToolTestOverallScoreCommand command)
        {
            MethodResult<double> queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Update Module Process
        /// </summary>
        [HttpGet("overall-skill-score")]
        [ProducesResponseType(typeof(MethodResult<object>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Common.Attributes.Permission(role: nameof(EnumRole.Admin))]
        public async Task<IActionResult> GetOverallParam([FromQuery] GetParamOverallScoreQuery query)
        {
            MethodResult<object> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Update goal aggregate
        /// </summary>
        [HttpPost("goal-aggregate")]
        [ProducesResponseType(typeof(MethodResult<object>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(roles: new string[] { nameof(EnumRole.Admin) })]
        public async Task<IActionResult> UpdateAggregate()
        {
            var queryResult = await _mediator.Send(new RebuildLearningGoalAggregateCommand()).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Update goal aggregate
        /// </summary>
        [HttpPost("test-goal-aggregate")]
        [ProducesResponseType(typeof(MethodResult<object>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(roles: new string[] { nameof(EnumRole.Admin) })]
        public async Task<IActionResult> UpdateAggregateTest()
        {
            var queryResult = await _mediator.Send(new TestDataRebuildCommand()).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Import Module Process
        /// </summary>
        [HttpGet("unauthorized")]
        [ProducesResponseType(typeof(MethodResult<string>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public IActionResult ActionUnauthorized()
        {
            MethodResult<string> commandResult = new MethodResult<string>();
            commandResult.AddError(StatusCodes.Status401Unauthorized, "Unauthorized");
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Import Module Process
        /// </summary>
        [HttpGet("test-ai")]
        [ProducesResponseType(typeof(MethodResult<string>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Common.Attributes.Permission(roles: new string[] { nameof(EnumRole.Admin) })]
        public async Task<double> TestAi([FromQuery] string? question, [FromQuery] string? file)
        {
            return await _speakingEvaluationAIService.EvaluationSpeakingV1(question, file);
        }
    }

    public class QueueTestModel
    {
        public object? Data { get; set; }
    }
}
