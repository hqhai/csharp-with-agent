// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Master.Api.Controllers
{
    using System.Net;
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Attributes;
    using Fsel.Common.Constants;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Master.Application.Queries.ProgressMetrics;
    using Fsel.Master.Domain.Models.EntityModels;
    using Fsel.Shared.Constants;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/progress-metric")]
    [ApiController]
    public class ProgressMetricController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProgressMetricController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("get-placement-test-overview")]
        [ProducesResponseType(typeof(MethodResult<PlacementTestDashboardModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetPlacementTestOverview([FromQuery] GetPlacementTestOverviewQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        [HttpGet("get-province-level-distribution")]
        [ProducesResponseType(typeof(MethodResult<ProvinceLevelResponse>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetProvinceLevelDistribution([FromQuery] GetProvinceLevelDistributionQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        [HttpGet("get-placement-test-student")]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<PlacementTestStudentModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetPlacementTestStudentDetail([FromQuery] GetPlacementTestStudentQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        [HttpPost("export-placement-test")]
        [ProducesResponseType(typeof(MethodResult<byte[]>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ExportFile([FromQuery] ExportPlacementTestQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            if (!queryResult.IsOK || queryResult.Result == null)
            {
                return queryResult.GetActionResult();
            }
            var currentDate = DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam);
            return File(queryResult.Result, Settings.Excels.ContentType, $"export_placement_test_{currentDate.Day}_{currentDate.Month}.xlsx");
        }

        [HttpGet("get-placement-test-detail")]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<PlacementTestDetailModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetPlacementTestDetail([FromQuery] GetPlacementTestDetailQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        [HttpGet("get-goal-progress")]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<IList<GoalProgressModel>>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetGoalProgress([FromQuery] GetGoalProgressQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        [HttpGet("get-learning-goal-result")]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<IList<GoalProgressModel>>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetLearningGoalResult([FromQuery] GetLearningGoalResultQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        [HttpGet("get-goal-overall")]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<IList<GoalProgressModel>>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetGoalOverview([FromQuery] GetGoalOverallQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        [HttpGet("get-goal-overview")]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<GoalOverviewModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetGoalOverview([FromQuery] GetGoalOverviewQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
