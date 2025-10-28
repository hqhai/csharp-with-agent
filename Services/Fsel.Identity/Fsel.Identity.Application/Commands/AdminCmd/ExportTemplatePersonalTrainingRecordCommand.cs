// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.AdminCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Identity.Domain.Models.CommandModels.Admins;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class ExportTemplatePersonalTrainingRecordCommand : IRequest<MethodResult<Stream>>
    {
    }

    public class ExportTemplatePersonalTrainingRecordCommandHandler : IRequestHandler<ExportTemplatePersonalTrainingRecordCommand, MethodResult<Stream>>
    {
        public ExportTemplatePersonalTrainingRecordCommandHandler()
        {
        }

        public async Task<MethodResult<Stream>> Handle(ExportTemplatePersonalTrainingRecordCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<Stream>();

            return await Task.Run(() =>
            {
                var template = new List<string>();
                var stream = template.ExportExcelTemplate<PtImportRowCommandModel>();
                methodResult.Result = stream;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }, cancellationToken);
        }
    }
}
