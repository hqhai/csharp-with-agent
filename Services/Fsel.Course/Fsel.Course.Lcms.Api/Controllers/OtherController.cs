// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lcms.Api.Controllers
{
    using System.Net;
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Attributes;
    using Fsel.Common.Constants;
    using Fsel.Course.Application.Queries.OtherQuery;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/other")]
    [Permission(role: nameof(EnumRole.MasterAdmin))]
    [ApiController]
    public class OtherController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OtherController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Export Report
        /// </summary>
        [HttpGet("export-all-course-question")]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Export()
        {
            MethodResult<Stream> commandResult = await _mediator.Send(new ExportAllCoursesQuestionsExcelQuery()).ConfigureAwait(false);
            if (!commandResult.IsOK || commandResult.Result == null)
            {
                return commandResult.GetActionResult();
            }
            return File(commandResult.Result, Settings.Excels.ContentType, "all_course_question.xlsx");
        }
    }
}
