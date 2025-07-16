using System.Net;
using Asp.Versioning;
using Fsel.Common.ActionResults;
using Fsel.Common.Constants;
using Fsel.Core.Base.BaseModels;
using Fsel.Interaction.Application.Commands.SurveyConfigCmd;
using Fsel.Interaction.Application.Queries.SurveyConfigQuery;
using Fsel.Interaction.Domain.Models.EntityModels;
using Fsel.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Fsel.Interaction.Api.Controllers.Admin
{
    [ApiVersion(ApiSettings.APIVersion1)]
    [Route(Settings.APIDefaultRoute + "/admin/survey-config")]
    [ApiController]
    public class SurveyConfigController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SurveyConfigController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// search
        /// </summary>
        [HttpGet("search")]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<SurveyConfigModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Search([FromQuery] SearchSurveyConfigQuery query)
        {
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Create
        /// </summary>
        [HttpPost("save")]
        [ProducesResponseType(typeof(MethodResult<SurveyConfigModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Create([FromBody] SaveSurveyConfigCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get by id
        /// </summary>
        [HttpGet("get-by-id")]
        [ProducesResponseType(typeof(MethodResult<SurveyConfigModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetById([FromQuery] GetSurveyConfigByIdQuery query)
        {
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Change status
        /// </summary>
        [HttpPost("change-status")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> ChangeStatus([FromBody] ChangeStatusSurveyConfigCommand command)
        {
            var commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Export students do survey
        /// </summary>
        [HttpPost("export-students-do-survey")]
        [ProducesResponseType(typeof(MethodResult<Stream>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ExportFile([FromQuery] ExportHistoriesDoSurveyCommand command)
        {
            var queryResult = await _mediator.Send(command).ConfigureAwait(false);
            if (!queryResult.IsOK || queryResult.Result == null)
            {
                return queryResult.GetActionResult();
            }
            return File(queryResult.Result, Settings.Excels.ContentType, "export_histories_do_survey.xlsx");
        }

        /// <summary>
        /// search student survey record
        /// </summary>
        [HttpGet("search-student-survey-record")]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<StudentSurveyRecordModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> SearchStudentSurveyRecord([FromQuery] SearchStudentSurveyRecordQuery query)
        {
            var commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
