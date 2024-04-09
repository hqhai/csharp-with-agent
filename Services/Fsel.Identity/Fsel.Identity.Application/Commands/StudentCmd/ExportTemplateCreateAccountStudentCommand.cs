// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.StudentCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Identity.Domain.Models.CommandModels.Students;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class ExportTemplateCreateAccountStudentCommand : IRequest<MethodResult<Stream>>
    {
    }

    public class ExportTemplateCreateAccountStudentCommandHandler : IRequestHandler<ExportTemplateCreateAccountStudentCommand, MethodResult<Stream>>
    {
        public ExportTemplateCreateAccountStudentCommandHandler()
        {
        }

        public async Task<MethodResult<Stream>> Handle(ExportTemplateCreateAccountStudentCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<Stream>();

            return await Task.Run(() =>
            {
                var template = new List<string>();
                var stream = template.ExportExcelTemplate<ImportStudentToPlatformModel>();
                methodResult.Result = stream;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }, cancellationToken);
        }
    }
}
