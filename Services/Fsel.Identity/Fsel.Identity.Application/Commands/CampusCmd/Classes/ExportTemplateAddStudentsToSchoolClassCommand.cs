// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.CampusCmd.Classes
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Identity.Domain.Models.CommandModels.Campus;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class ExportTemplateAddStudentsToSchoolClassCommand : IRequest<MethodResult<Stream>>
    {
    }

    public class ExportTemplateAddStudentsToSchoolClassCommandHandler : IRequestHandler<ExportTemplateAddStudentsToSchoolClassCommand, MethodResult<Stream>>
    {
        public ExportTemplateAddStudentsToSchoolClassCommandHandler()
        {
        }

        public async Task<MethodResult<Stream>> Handle(ExportTemplateAddStudentsToSchoolClassCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<Stream>();

            return await Task.Run(() =>
            {
                var template = new List<string>();
                var stream = template.ExportExcelTemplate<AddStudentsToSchoolClassModel>();
                methodResult.Result = stream;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }, cancellationToken);
        }
    }
}
