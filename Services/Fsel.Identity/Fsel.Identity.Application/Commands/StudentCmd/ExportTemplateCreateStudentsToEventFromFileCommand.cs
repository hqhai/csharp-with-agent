// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.StudentCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Identity.Domain.Models.CommandModels.Students;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class ExportTemplateCreateStudentsToEventFromFileCommand : IRequest<MethodResult<Stream>>
    {
    }

    public class ExportTemplateCreateStudentsToEventFromFileCommandHandler : IRequestHandler<ExportTemplateCreateStudentsToEventFromFileCommand, MethodResult<Stream>>
    {
        public ExportTemplateCreateStudentsToEventFromFileCommandHandler()
        {
        }

        public async Task<MethodResult<Stream>> Handle(ExportTemplateCreateStudentsToEventFromFileCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<Stream>();

            return await Task.Run(() =>
            {
                var template = new List<string>();
                var stream = template.ExportExcelTemplate<CreateStudentToEventFromFileModel>();
                methodResult.Result = stream;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }, cancellationToken);
        }
    }
}
