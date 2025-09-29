// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.CampusCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Identity.Domain.Models.CommandModels.Campus;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class ExportTemplateAddStudentsToCurriculumCommand : IRequest<MethodResult<Stream>>
    {
    }

    public class ExportTemplateAddStudentsToCurriculumCommandHandler : IRequestHandler<ExportTemplateAddStudentsToCurriculumCommand, MethodResult<Stream>>
    {
        public ExportTemplateAddStudentsToCurriculumCommandHandler()
        {
        }

        public async Task<MethodResult<Stream>> Handle(ExportTemplateAddStudentsToCurriculumCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<Stream>();

            return await Task.Run(() =>
            {
                var template = new List<string>();
                var stream = template.ExportExcelTemplate<AddStudentsToCurriculumModel>();
                methodResult.Result = stream;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }, cancellationToken);
        }
    }
}
