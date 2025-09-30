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
    using OfficeOpenXml;

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
                var newStream = AddDataToExcel(stream);
                methodResult.Result = newStream;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }, cancellationToken);
        }

        public MemoryStream AddDataToExcel(Stream stream)
        {
            ArgumentNullException.ThrowIfNull(stream);

            stream.Position = 0;

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets[0];

            int row = 2;

            worksheet.Cells[row, 1].Value = "Sonxs@fpt.edu.vn";
            worksheet.Cells[row, 2].Value = "Sonxs@fpt.edu.vn";

            var output = new MemoryStream();
            package.SaveAs(output);
            output.Position = 0;
            return output;
        }
    }
}
