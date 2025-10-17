// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Lms.Api.Controllers
{
    using System.Net;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Core.Base;
    using Fsel.ExamPractice.Domain.Models.EntityModels.ExamPractices;
    using Fsel.ExamPractice.Lms.Application.Commands.ExamPracticeCmd;
    using Fsel.ExamPractice.Lms.Application.Queries.ExamPracticeQuery;
    using Fsel.ExamPractice.Lms.Application.Queries.ReportQuery;
    using Fsel.Shared.Attributes;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore.Metadata.Internal;

    [ApiVersions(ApiSettings.APIVersion1)]
    [ApiController]
    [Route(Settings.APIDefaultRoute + "/exam-practice")]
   [Common.Attributes.Permission(roles: new string[] { nameof(EnumRole.Student), nameof(EnumRole.StudentCampus) })]
    public class ExamPracticeController : BaseController
    {
        private readonly IMediator _mediator;

        public ExamPracticeController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Reset ExamPractice
        /// </summary>
        [HttpPut("reset/{id}")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ResetExamPractice([FromRoute] Guid id)
        {
            MethodResult<bool> queryResult = await _mediator.Send(new ResetExamPracticeCommand { Id = id }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Start ExamPractice
        /// </summary>
        [HttpPost("start")]
        [ProducesResponseType(typeof(MethodResult<ExamPracticeResultModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> StartExamPractice([FromBody] StartExamPracticeCommand command)
        {
            MethodResult<ExamPracticeResultModel> queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Search ExamPractice
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<IList<ExamPracticeGroupTypeModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Search([FromQuery] SearchExamPracticeQuery query)
        {
            SetQuery(query);
            MethodResult<IList<ExamPracticeGroupTypeModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get ExamPractice Part
        /// </summary>
        [HttpGet("parts")]
        [ProducesResponseType(typeof(MethodResult<ExamPracticeConfigModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetPart([FromQuery] GetExamPracticePartsQuery query)
        {
            MethodResult<ExamPracticeConfigModel> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get ExamPractice
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(MethodResult<ExamPracticeDetailModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get([FromRoute] Guid id)
        {
            MethodResult<ExamPracticeDetailModel> queryResult = await _mediator.Send(new GetExamPracticeQuery { Id = id }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get ExamPracticeSection
        /// </summary>
        [HttpGet("examPracticeSection")]
        [ProducesResponseType(typeof(MethodResult<ExamPracticeSectionDetailModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get([FromQuery] GetExamPracticeSectionQuery query)
        {
            MethodResult<ExamPracticeSectionDetailModel> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Create Answers
        /// </summary>
        [HttpPost("create-answers")]
        [ProducesResponseType(typeof(MethodResult<ExamPracticeSectionResultModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreateAnswers([FromBody] CreateExamPracticeAnswerCommand command)
        {
            MethodResult<ExamPracticeSectionResultModel> queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get Report ExamPracticeSection
        /// </summary>
        [HttpGet("report-exam-practice")]
        [ProducesResponseType(typeof(MethodResult<ExamPracticeResultReportModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetReport([FromQuery] GetReportExamPracticeResultQuery query)
        {
            MethodResult<ExamPracticeResultReportModel> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Get Report ExamPracticeSection
        /// </summary>
        [HttpGet("report-exam-practice-section")]
        [ProducesResponseType(typeof(MethodResult<ExamPracticeSectionResultModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetReport([FromQuery] GetExamPracticeSectionResultReportQuery query)
        {
            MethodResult<ExamPracticeSectionResultModel> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
