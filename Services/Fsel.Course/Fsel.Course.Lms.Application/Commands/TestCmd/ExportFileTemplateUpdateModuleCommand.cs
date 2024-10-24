// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.TestCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Models.CommandModels.Tests;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class ExportFileTemplateUpdateModuleCommand : IRequest<MethodResult<Stream>>
    {
    }

    public class ExportFileTemplateUpdateModuleCommandHandler : IRequestHandler<ExportFileTemplateUpdateModuleCommand, MethodResult<Stream>>
    {
        public ExportFileTemplateUpdateModuleCommandHandler()
        {
        }

        public async Task<MethodResult<Stream>> Handle(ExportFileTemplateUpdateModuleCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<Stream>();
            return await Task.Run(() =>
            {
                var template = new List<string>();
                var stream = template.ExportExcelTemplate<UpdateStudentModuleProgressCommandModel>();
                methodResult.Result = stream;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }, cancellationToken);
        }
    }
}
