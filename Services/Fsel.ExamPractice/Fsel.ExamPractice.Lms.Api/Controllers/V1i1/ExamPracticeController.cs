// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Lms.Api.Controllers.V1i1
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Attributes;
    using Fsel.Common.Constants;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.ExamPractice.Domain.Models.EntityModels.ExamPractices;
    using Fsel.ExamPractice.Lms.Application.Queries.ExamPracticeQuery.V1i1;
    using Fsel.ExamPractice.Lms.Application.Queries.ReportQuery;
    using Fsel.Shared.Attributes;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersions(ApiSettings.APIVersion1i1)]
    [ApiController]
    [Route(Settings.APIDefaultRoute + "/exam-practice")]
    [Permission(role: nameof(EnumRole.Student))]
    public class ExamPracticeController : BaseController
    {
        private readonly IMediator _mediator;

        public ExamPracticeController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Search ExamPractice
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<ExamPracticeGroupModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Search([FromQuery] SearchExamPracticeQuery query)
        {
            SetQuery(query);
            MethodResult<PagingItemsModel<ExamPracticeGroupModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Search ExamPractice Result Done
        /// </summary>
        [HttpGet("result-done")]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<ExamPracticeReportModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Search([FromQuery] SearchResultDoneByTypeQuery query)
        {
            MethodResult<PagingItemsModel<ExamPracticeReportModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Dashboard Report
        /// </summary>
        [HttpGet("dashboard-report")]
        [ProducesResponseType(typeof(MethodResult<ExamDashboardModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Search([FromQuery] GetDashboardReportByTypeQuery query)
        {
            MethodResult<ExamDashboardModel> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Dashboard Report
        /// </summary>
        [HttpGet("report-view")]
        [ProducesResponseType(typeof(MethodResult<ExamPracticeReportViewModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ReportView([FromQuery] GetReportResultQuery query)
        {
            MethodResult<ExamPracticeReportViewModel> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
