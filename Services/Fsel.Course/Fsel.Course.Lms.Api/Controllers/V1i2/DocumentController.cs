// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers.V1i2
{
    using System.Net;
    using Application.Queries.DocumentQuery;
    using Common.ActionResults;
    using Common.Attributes;
    using Common.Constants;
    using Domain.Models.EntityModels.V1i1;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using Shared.Attributes;
    using Shared.Constants;
    using Shared.Enums;

    [ApiVersions(ApiSettings.APIVersion1i2)]
    [Route(Settings.APIDefaultRoute + "/document")]
    [Permission(role: nameof(EnumRole.Student))]
    [ApiController]
    public class DocumentController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DocumentController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get List Document for lesson
        /// </summary>
        [HttpGet("{originalId}")]
        [ProducesResponseType(typeof(MethodResult<DocumentModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetDocument([FromRoute] Guid originalId)
        {
            var getDocument = new SearchDocumentQuery { OriginalId = originalId };
            var queryResult = await _mediator.Send(getDocument).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }
    }
}
