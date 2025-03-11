// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lcms.Api.Controllers
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Attributes;
    using Fsel.Common.Constants;
    using Fsel.Course.Application.Queries.ReportEventHaNoiQuery;
    using Fsel.Course.Domain.Models.EntityModels.ReportEventHaNoi;
    using Fsel.Shared.Attributes;
    using Fsel.Shared.Constants;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersions(ApiSettings.APIVersion1)]
    [Route(Settings.APIDefaultRoute + "/admin/report-event-hanoi")]
    [ApiController]
    public class ReportEventHaNoiController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ReportEventHaNoiController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get 
        /// </summary>
        [ServerCache(CacheSettings.TimeCache.ThreeHour)]
        [HttpPost("evaluat-input-result")]
        [ProducesResponseType(typeof(MethodResult<IList<TotalEvaluateInputResultModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> TotalEvaluateInputResultQuery([FromBody] TotalEvaluateInputResultQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get 
        /// </summary>
        [ServerCache(CacheSettings.TimeCache.ThreeHour)]
        [HttpPost("evaluat-detail-input-result")]
        [ProducesResponseType(typeof(MethodResult<IList<TotalDetailEvaluateInputResultModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> TotalDetailEvaluateInputResult([FromBody] TotalDetailEvaluateInputResultQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get 
        /// </summary>
        [ServerCache(CacheSettings.TimeCache.ThreeHour)]
        [HttpPost("percent-evaluat-input-result")]
        [ProducesResponseType(typeof(MethodResult<IList<PercentEvaluateInputResultModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> PercentEvaluateInputResult([FromBody] PercentEvaluateInputResultQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get 
        /// </summary>
        [ServerCache(CacheSettings.TimeCache.ThreeHour)]
        [HttpPost("level-input-result")]
        [ProducesResponseType(typeof(MethodResult<IList<LevelEvaluateInputResultModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> LevelEvaluateInputResultQuery([FromBody] LevelEvaluateInputResultQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get 
        /// </summary>
        [ServerCache(CacheSettings.TimeCache.ThreeHour)]
        [HttpPost("school-summary")]
        [ProducesResponseType(typeof(MethodResult<EvaluateInputResultModel>), (int)HttpStatusCode.OK)]
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
        [HttpPost("total-learning-progress")]
        [ProducesResponseType(typeof(MethodResult<IList<TotalLearningProgressModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> TotalLearningProgress([FromBody] TotalLearningProgressQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get 
        /// </summary>
        [ServerCache(CacheSettings.TimeCache.ThreeHour)]
        [HttpPost("average-learning-progress")]
        [ProducesResponseType(typeof(MethodResult<IList<AverageLearningProgressModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> AverageLearningProgres([FromBody] AverageLearningProgresQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get 
        /// </summary>
        [ServerCache(CacheSettings.TimeCache.ThreeHour)]
        [HttpPost("unit-aca-learning-progress")]
        [ProducesResponseType(typeof(MethodResult<IList<UnitDoneLearningProgressAcademicModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UnitDoneLearningProgressAcademic([FromBody] UnitDoneLearningProgressAcademicQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get 
        /// </summary>
        [ServerCache(CacheSettings.TimeCache.ThreeHour)]
        [HttpPost("unit-ielts-learning-progress")]
        [ProducesResponseType(typeof(MethodResult<IList<UnitDoneLearningProgressIeltsModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UnitDoneLearningProgressIelts([FromBody] UnitDoneLearningProgressIeltsQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get 
        /// </summary>
        [ServerCache(CacheSettings.TimeCache.ThreeHour)]
        [HttpPost("lesson-ielts-learning-progress")]
        [ProducesResponseType(typeof(MethodResult<IList<LessonDoneLearningProgressIeltsModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> LessonDoneLearningProgressIeltsQuery([FromBody] LessonDoneLearningProgressIeltsQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get 
        /// </summary>
        [ServerCache(CacheSettings.TimeCache.ThreeHour)]
        [HttpPost("lesson-aca-learning-progress")]
        [ProducesResponseType(typeof(MethodResult<IList<LessonDoneLearningProgressAcademicModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> LessonDoneLearningProgressAcademic([FromBody] LessonDoneLearningProgressAcademicQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get 
        /// </summary>
        [ServerCache(CacheSettings.TimeCache.ThreeHour)]
        [HttpPost("total-learning-quality")]
        [ProducesResponseType(typeof(MethodResult<IList<TotalLearningModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> LearningQuality([FromBody] TotalLearningQualityQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get 
        /// </summary>
        [ServerCache(CacheSettings.TimeCache.ThreeHour)]
        [HttpPost("rate-learning-quality")]
        [ProducesResponseType(typeof(MethodResult<IList<RateLearningModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> RateLearningQuality([FromBody] RateLearningQualityQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get 
        /// </summary>
        [ServerCache(CacheSettings.TimeCache.ThreeHour)]
        [HttpPost("chart-learning-quality")]
        [ProducesResponseType(typeof(MethodResult<IList<TotalLearningQualityModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> TotalLearningQualityChart([FromBody] TotalLearningQualityChartQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get 
        /// </summary>
        [ServerCache(CacheSettings.TimeCache.ThreeHour)]
        [HttpPost("total-detail-learning-quality")]
        [ProducesResponseType(typeof(MethodResult<IList<TotalDetailLearningQualityModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> TotalDetailLearningQuality([FromBody] TotalDetailLearningQualityQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get 
        /// </summary>
        [HttpPost("school-info")]
        [ProducesResponseType(typeof(MethodResult<IList<SchoolInfoModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetSchoolInfo([FromBody] GetSchoolInfoQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
