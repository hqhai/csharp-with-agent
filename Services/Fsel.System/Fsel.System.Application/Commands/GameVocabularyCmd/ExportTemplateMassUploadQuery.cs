// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.GameVocabularyCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.System.Domain.Models.CommandModels.GameVocabularies;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class ExportTemplateMassUploadQuery : IRequest<MethodResult<Stream>>
    {
    }

    public class ExportTemplateMassUploadQueryHandler : IRequestHandler<ExportTemplateMassUploadQuery, MethodResult<Stream>>
    {
        public ExportTemplateMassUploadQueryHandler()
        {
        }

        public async Task<MethodResult<Stream>> Handle(ExportTemplateMassUploadQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<Stream>();

            return await Task.Run(() =>
            {
                var template = new List<string>();
                var stream = template.ExportExcelTemplate<MassUploadVocabularyCommandModel>();
                methodResult.Result = stream;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }, cancellationToken);
        }
    }
}
