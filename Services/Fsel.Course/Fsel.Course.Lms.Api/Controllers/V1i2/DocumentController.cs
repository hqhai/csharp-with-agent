// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers.V1i2
{
    using System.Net;
    using Application.Commands.DocumentCmd;
    using Application.Queries.DocumentQuery;
    using Common.ActionResults;
    using Common.Constants;
    using Domain.Models.EntityModels.V1i1;
    using Domain.Models.EntityModels.V1i2;
    using Fsel.Common.Attributes;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using Shared.Attributes;
    using Shared.Constants;

    [ApiVersions(ApiSettings.APIVersion1i2)]
    [Route(Settings.APIDefaultRoute + "/document")]
    [Permission(roles: new string[] { nameof(EnumRole.Student), nameof(EnumRole.StudentCampus) })]
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
        [HttpGet("{documentId}")]
        [ProducesResponseType(typeof(MethodResult<DocumentModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetDocument([FromRoute] Guid documentId)
        {
            var getDocument = new GetDocumentQuery { DocumentId = documentId };
            var queryResult = await _mediator.Send(getDocument).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Update document result
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(MethodResult<DocumentResultModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Create([FromRoute] Guid id)
        {
            var commandResult = await _mediator.Send(new UpdateDocumentResultCommand { Id = id }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
