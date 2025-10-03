// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.CampusCmd.Classes
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Identity.Domain.Models.CommandModels.Campus;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using OfficeOpenXml;

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

            worksheet.Cells[row, 1].Value = "Nguyễn Văn A";
            worksheet.Cells[row, 2].Value = string.Empty;
            worksheet.Cells[row, 3].Value = "nguyenvana@gmail.com";
            worksheet.Cells[row, 4].Value = "01-01-2025";

            var output = new MemoryStream();
            package.SaveAs(output);
            output.Position = 0;
            return output;
        }
    }
}
