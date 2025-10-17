// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Attributes;
    using Fsel.Common.Constants;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Queries.OtherFeatureQuery;
    using Fsel.Course.Lms.Application.Queries.Reports;
    using Fsel.Course.Lms.Application.Queries.Reports.Sales;
    using Fsel.Shared.Attributes;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersions(ApiSettings.APIVersion1)]
    [Route(Settings.APIDefaultRoute + "/report")]
    [ApiController]
    [Permission]
    public class ReportController : ControllerBase
    {
        private readonly IMediator _mediator;
        private ILogger<ReportController> _logger;

        public ReportController(IMediator mediator, ILogger<ReportController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Get Overall Report By Student
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<OverallReportModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get([FromQuery] GetOverallReportByStudentQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Export Report Progress Student
        /// </summary>
        [HttpPost("export-progress-student")]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(role: nameof(EnumRole.Admin))]
        public async Task<IActionResult> Export([FromForm] ExportFileProgressStudentQuery query)
        {
            MethodResult<Stream> commandResult = await _mediator.Send(query).ConfigureAwait(false);
            if (!commandResult.IsOK || commandResult.Result == null)
            {
                return commandResult.GetActionResult();
            }
            return File(commandResult.Result, Settings.Excels.ContentType, "Export_Progress_Student.xlsx");
        }

        /// <summary>
        /// Export Report
        /// </summary>
        [HttpPost("export-overall")]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(role: nameof(EnumRole.Admin))]
        public async Task<IActionResult> Export([FromQuery] ExportFileProgressStudentsToEmailsQuery query)
        {
            MethodResult<Stream> commandResult = await _mediator.Send(query).ConfigureAwait(false);
            if (!commandResult.IsOK || commandResult.Result == null)
            {
                return commandResult.GetActionResult();
            }
            return File(commandResult.Result, Settings.Excels.ContentType, "progress_overall_student_export.xlsx");
        }

        /// <summary>
        /// Export Report Time Report
        /// </summary>
        [HttpPost("export-time-report")]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(role: nameof(EnumRole.Admin))]
        public async Task<IActionResult> Export([FromForm] ExportTimeReportStudentQuery query)
        {
            MethodResult<Stream> commandResult = await _mediator.Send(query).ConfigureAwait(false);
            if (!commandResult.IsOK || commandResult.Result == null)
            {
                return commandResult.GetActionResult();
            }
            return File(commandResult.Result, Settings.Excels.ContentType, "Export_Time_Report_Student.xlsx");
        }

        /// <summary>
        /// Export Report Time Report
        /// </summary>
        [HttpPost("export-progress-ielts")]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(role: nameof(EnumRole.Admin))]
        public async Task<IActionResult> Export([FromForm] ExportFileProgressStudentIELTSToEmailsQuery query)
        {
            MethodResult<Stream> commandResult = await _mediator.Send(query).ConfigureAwait(false);
            if (!commandResult.IsOK || commandResult.Result == null)
            {
                return commandResult.GetActionResult();
            }
            return File(commandResult.Result, Settings.Excels.ContentType, "Export_Student_Progress_IELTS.xlsx");
        }

        /// <summary>
        /// Expot File ExplanationQuestion
        /// </summary>
        [HttpPost("export-file-explanation-question")]
        [ProducesResponseType(typeof(MethodResult<Stream>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ExportFile([FromQuery] ExportFileReportExplanationLogQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            if (!queryResult.IsOK || queryResult.Result == null)
            {
                return queryResult.GetActionResult();
            }
            return File(queryResult.Result, Settings.Excels.ContentType, "export_file_explanation_question.xlsx");
        }

        /// <summary>
        /// Expot File PlacementTest Event
        /// </summary>
        [HttpPost("export-file-placement-test-event")]
        [ProducesResponseType(typeof(MethodResult<Stream>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(role: nameof(EnumRole.Admin))]
        public async Task<IActionResult> ExportFile([FromQuery] ExportReportPlacementTestEventQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            if (!queryResult.IsOK || queryResult.Result == null)
            {
                return queryResult.GetActionResult();
            }
            return File(queryResult.Result, Settings.Excels.ContentType, "export_report_placement_test_event.xlsx");
        }

        /// <summary>
        /// Expot File PlacementTest Event
        /// </summary>
        [HttpPost("export-file-placement-test-school-event")]
        [ProducesResponseType(typeof(MethodResult<Stream>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(role: nameof(EnumRole.Admin))]
        public async Task<IActionResult> ExportFile([FromQuery] ExportReportPlacementTestEventSchoolQuery query)
        {
            _logger.LoggerRequest($"ExportReportPlacementTestEventSchoolQuery : {query.Serialize()}");
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            if (!queryResult.IsOK || queryResult.Result == null)
            {
                return queryResult.GetActionResult();
            }
            return File(queryResult.Result, Settings.Excels.ContentType, "export_report_placement_test_school_event.xlsx");
        }

        /// <summary>
        /// Expot File PlacementTest Event
        /// </summary>
        [HttpPost("export-file-placement-test-district-school-event")]
        [ProducesResponseType(typeof(MethodResult<Stream>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(role: nameof(EnumRole.Admin))]
        public async Task<IActionResult> ExportFile([FromQuery] ExportReportPlacementTestEventDistrictSchoolQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);

            if (!queryResult.IsOK || queryResult.Result == null)
            {
                return queryResult.GetActionResult();
            }
            return File(queryResult.Result, Settings.Excels.ContentType, "export_report_placement_test_district_school_event.xlsx");
        }

        /// <summary>
        /// Expot File Learning Process District
        /// </summary>
        [HttpPost("export-file-learning-process-district")]
        [ProducesResponseType(typeof(MethodResult<Stream>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(role: nameof(EnumRole.Admin))]
        public async Task<IActionResult> ExportFile([FromQuery] ExportReportLearningProcessToDistrictQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            if (!queryResult.IsOK || queryResult.Result == null)
            {
                return queryResult.GetActionResult();
            }
            string url = $"export-file-learning-process-district-{query.EventCodeStr}-{query.CourseType}-{query.CourseLevel}-{query.EducationLevel}-{NumberHelper.GenerateCodeNumber(5)}.xlsx";
            return File(queryResult.Result, Settings.Excels.ContentType, url);
        }

        ///// <summary>
        ///// Expot File Learning Process Schools
        ///// </summary>
        //[HttpPost("export-file-learning-process-schools")]
        //[ProducesResponseType(typeof(MethodResult<Stream>), (int)HttpStatusCode.OK)]
        //[ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        //public async Task<IActionResult> ExportFile([FromQuery] ExportReportLearningProcessToSchoolsQuery query)
        //{
        //    var queryResult = await _mediator.Send(query).ConfigureAwait(false);
        //    if (!queryResult.IsOK || queryResult.Result == null)
        //    {
        //        return queryResult.GetActionResult();
        //    }
        //    return File(queryResult.Result, Settings.Excels.ContentType, $"export_file_learning_process_schools_{query?.EventCodeStr}_{query?.CourseType}.xlsx");
        //}

        /// <summary>
        /// Get Overall Report By Student
        /// </summary>
        [HttpGet("get-file-learning-process-students")]
        [ProducesResponseType(typeof(MethodResult<string>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(role: nameof(EnumRole.Admin))]
        public async Task<IActionResult> Get([FromQuery] GetFileExcelStudentLearningReportQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get Overall Report By Student
        /// </summary>
        [HttpGet("get-file-learning-process-school")]
        [ProducesResponseType(typeof(MethodResult<string>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(role: nameof(EnumRole.Admin))]
        public async Task<IActionResult> Get([FromQuery] GetFileExcelSchoolLearningProcessQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// aggregate data students in event
        /// </summary>
        [HttpPost("aggregate-data-students-in-event")]
        [ProducesResponseType(typeof(MethodResult<IList<AggregateDataStudentsInEventModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(role: nameof(EnumRole.Admin))]
        public async Task<IActionResult> AggregateDataStudentsInEvent([FromBody] AggregateDataStudentsInEventQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Expot File Learning Process District
        /// </summary>
        [HttpPost("export-file")]
        [ProducesResponseType(typeof(MethodResult<Stream>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(role: nameof(EnumRole.Admin))]
        public async Task<IActionResult> ExportFile([FromQuery] ExportReportSelfStudyMonthlyQuery query)
        {
            var queryResult = await _mediator.Send(query).ConfigureAwait(false);
            if (!queryResult.IsOK || queryResult.Result == null)
            {
                return queryResult.GetActionResult();
            }
            return File(queryResult.Result, Settings.Excels.ContentType, $"{query.FileName}_{query.EducationLevel.GetDescription()}.xlsx");
        }

        /// <summary>
        /// aggregate data students in event
        /// </summary>
        [HttpPost("export-file-report-sale-progress")]
        [ProducesResponseType(typeof(MethodResult<Stream>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(role: nameof(EnumRole.Admin))]
        public async Task<IActionResult> ExportReportSaleProgress()
        {
            var queryResult = await _mediator.Send(new ExportCustomerSupportSummaryQuery()).ConfigureAwait(false);
            if (!queryResult.IsOK || queryResult.Result == null)
            {
                return queryResult.GetActionResult();
            }
            return File(queryResult.Result, Settings.Excels.ContentType, $"ExportReportSaleProgress_{DateTime.Now.Ticks}.xlsx");
        }

        /// <summary>
        /// Get Overall Report By Student
        /// </summary>
        [HttpGet("get-file-report-sale-support")]
        [ProducesResponseType(typeof(MethodResult<string>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission(roles: new string[] { nameof(EnumRole.Admin), nameof(EnumRole.AdminSchool), nameof(EnumRole.CSO) })]
        public async Task<IActionResult> Get()
        {
            var queryResult = await _mediator.Send(new GetFileExcelUserInformationSupportSaleQuery()).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
