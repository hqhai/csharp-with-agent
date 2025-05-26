// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lcms.Api.Controllers
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Attributes;
    using Fsel.Common.Constants;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Application.Commands.ReportEventCmd;
    using Fsel.Course.Application.Queries.V1i1.ReportEvent;
    using Fsel.Course.Domain.Models.EntityModels.ReportEventHaNoi;
    using Fsel.Shared.Attributes;
    using Fsel.Shared.Constants;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersions(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/admin/report-event")]
    [ApiController]
    [Permission]
    public class ReportEventController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ReportEventController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get 
        /// </summary>
        [ServerCache(CacheSettings.TimeCache.ThreeHour)]
        [HttpPost("school-summary")]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<SchoolSummaryModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SchoolSummary([FromBody] SchoolSummaryQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get 
        /// </summary>
        [ServerCache(CacheSettings.TimeCache.ThreeHour)]
        [HttpPost("input-evaluation-summary")]
        [ProducesResponseType(typeof(MethodResult<IList<TotalEvaluateInputResultModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> InputEvaluationSummary([FromBody] InputEvaluationSummaryQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get 
        /// </summary>
        [ServerCache(CacheSettings.TimeCache.ThreeHour)]
        [HttpPost("input-evaluation-detail")]
        [ProducesResponseType(typeof(MethodResult<IList<TotalDetailEvaluateInputResultModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> InputEvaluationDetail([FromBody] InputEvaluationDetailQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get 
        /// </summary>
        [ServerCache(CacheSettings.TimeCache.ThreeHour)]
        [HttpPost("input-evaluation-percent")]
        [ProducesResponseType(typeof(MethodResult<IList<PercentEvaluateInputResultModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> InputEvaluationPercent([FromBody] InputEvaluationPercentQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get 
        /// </summary>
        [ServerCache(CacheSettings.TimeCache.ThreeHour)]
        [HttpPost("input-evaluation-total-level")]
        [ProducesResponseType(typeof(MethodResult<IList<LevelEvaluateInputResultModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> InputEvaluationTotalLevel([FromBody] InputEvaluationTotalLevelQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get 
        /// </summary>
        [HttpPost("district-info-filter")]
        [ProducesResponseType(typeof(MethodResult<IList<DistrictInfoModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetDistrictInfo()
        {
            var queryResult = await _mediator.Send(new GetDistrictInfoQuery()).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get 
        /// </summary>
        [HttpPost("school-info-filter")]
        [ProducesResponseType(typeof(MethodResult<IList<SchoolInfoFilterModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetSchoolInfoFilter([FromBody] GetSchoolInfoFilterQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get 
        /// </summary>
        [HttpPost("learning-quality-summary")]
        [ProducesResponseType(typeof(MethodResult<IList<TotalLearningModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> LearningQualitySummary([FromBody] LearningQualitySummaryQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get 
        /// </summary>
        [HttpPost("learning-quality-percent")]
        [ProducesResponseType(typeof(MethodResult<IList<RateLearningModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> LearningQualityPercent([FromBody] LearningQualityPercentQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get 
        /// </summary>
        [HttpPost("learning-quality-chart")]
        [ProducesResponseType(typeof(MethodResult<IList<TotalLearningQualityModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> LearningQualityChart([FromBody] LearningQualityChartQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get 
        /// </summary>
        [HttpPost("learning-quality-detail")]
        [ProducesResponseType(typeof(MethodResult<IList<TotalDetailLearningQualityModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> LearningQualityDetail([FromBody] LearningQualityDetailQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get 
        /// </summary>
        [HttpPost("learning-progress-average-detail")]
        [ProducesResponseType(typeof(MethodResult<IList<AverageLearningProgressModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> LearningProgressAverageDetail([FromBody] LearningProgressAverageDetailQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get 
        /// </summary>
        [HttpPost("learning-progress-lesson-done-academic")]
        [ProducesResponseType(typeof(MethodResult<IList<LessonDoneLearningProgressAcademicModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> LearningProgressLessonDoneAcademic([FromBody] LearningProgressLessonDoneAcademicQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get 
        /// </summary>
        [HttpPost("learning-progress-lesson-done-ielts")]
        [ProducesResponseType(typeof(MethodResult<IList<LessonDoneLearningProgressIeltsModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> LearningProgressLessonDoneIelts([FromBody] LearningProgressLessonDoneIeltsQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get 
        /// </summary>
        [HttpPost("learning-progress-unit-done-academic")]
        [ProducesResponseType(typeof(MethodResult<IList<UnitDoneLearningProgressAcademicModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> LearningProgressUnitDoneAcademic([FromBody] LearningProgressUnitDoneAcademicQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get 
        /// </summary>
        [HttpPost("learning-progress-unit-done-ielts")]
        [ProducesResponseType(typeof(MethodResult<IList<UnitDoneLearningProgressIeltsModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> LearningProgressUnitDoneIelts([FromBody] LearningProgressUnitDoneIeltsQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get 
        /// </summary>
        [HttpPost("learning-progress-summary")]
        [ProducesResponseType(typeof(MethodResult<IList<TotalLearningProgressModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> LearningProgressSummary([FromBody] LearningProgressSummaryQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get 
        /// </summary>
        [HttpPost("export-summary-report")]
        [ProducesResponseType(typeof(MethodResult<Stream>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ExportSummaryReport([FromQuery] ExportSummaryReportCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            if (!commandResult.IsOK || commandResult.Result == null)
            {
                return commandResult.GetActionResult();
            }
            return File(commandResult.Result, Settings.Excels.ContentType, command.CheckByGroup == 1 ? "BaoCao_Capso.xlsx" : "BaoCao_CapPhong.xlsx");
        }
    }
}
