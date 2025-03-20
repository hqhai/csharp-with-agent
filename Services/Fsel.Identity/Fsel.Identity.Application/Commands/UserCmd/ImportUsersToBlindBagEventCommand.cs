// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.UserCmd
{
    using System.Drawing;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Common.Models.Excels;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Identity.Application.Services.OrderService;
    using Fsel.Identity.Application.Services.OrderService.QueryModels;
    using Fsel.Identity.Application.Services.SystemService;
    using Fsel.Identity.Application.Services.SystemService.CommandModels;
    using Fsel.Identity.Application.Services.SystemService.QueryModels;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using OfficeOpenXml;
    using OfficeOpenXml.Attributes;
    using OfficeOpenXml.Style;

    public class ExcelUserBlindBagEventModel
    {
        [EpplusTableColumn(Header = "Username (mỗi dòng chỉ điền 1 username)")]
        public string? UserName { get; set; }

        [EpplusTableColumn(Header = "Mã lỗi (hệ thống tự trả, không được điền)")]
        public string? Status { get; set; }
    }

    public class ImportUsersToBlindBagEventCommand : BaseImportCommandModel, IRequest<MethodResult<CreateStudentsToEventFromFileModel>>
    {
    }

    public class ImportUsersToBlindBagEventCommandHandler : IRequestHandler<ImportUsersToBlindBagEventCommand, MethodResult<CreateStudentsToEventFromFileModel>>
    {
        private readonly Microsoft.AspNetCore.Identity.UserManager<User> _userManager;
        private readonly IOrderService _orderService;
        private readonly ISystemService _systemService;

        public ImportUsersToBlindBagEventCommandHandler(
            Microsoft.AspNetCore.Identity.UserManager<User> userManager,
            IOrderService orderService,
            ISystemService systemService)
        {
            _userManager = userManager;
            _orderService = orderService;
            _systemService = systemService;
        }

        public async Task<MethodResult<CreateStudentsToEventFromFileModel>> Handle(ImportUsersToBlindBagEventCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<CreateStudentsToEventFromFileModel>();

            if (request.FormFile == null)
            {
                methodResult.AddError(nameof(EnumSystemErrorCode.ImportFileRequired));
                return methodResult;
            }

            Action<ExcelWorksheet, Dictionary<string, int?>?, IList<ValidateExcelModel>> errorHandlerAction = (worksheet, columnIndexes, errors) =>
            {
                foreach (var error in errors.GroupBy(x => x.RowIndex).Select(x => x).OrderBy(x => x.Key))
                {
                    var row = error.Key;
                    // Tạo biến lưu trữ dữ liệu dòng hiện tại
                    List<object?> rowData = new List<object?>();
                    // Lưu dữ liệu của dòng vào biến rowData
                    for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
                    {
                        rowData.Add(worksheet.Cells[row, col].Value);
                    }

                    int targetRow = 2;
                    worksheet.DeleteRow(row, 1);
                    worksheet.InsertRow(targetRow, 1);

                    // Gắn lại dữ liệu đã lưu vào dòng mới
                    for (int col = 1; col <= rowData.Count; col++)
                    {
                        worksheet.Cells[targetRow, col].Value = rowData[col - 1];

                        // Nếu cần, có thể sao chép cả định dạng
                        worksheet.Cells[targetRow, col].StyleID = worksheet.Cells[row, col].StyleID;
                    }

                    var errorMessages = new List<string>();

                    foreach (var errorMessage in error.OrderBy(p => p.RowIndex))
                    {
                        var message = errorMessage.Message;
                        if (!string.IsNullOrEmpty(message))
                        {
                            errorMessages.Add(message);
                            int? num = columnIndexes?[errorMessage.ColumnName ?? string.Empty];
                            if (num.HasValue)
                            {
                                worksheet.Cells[targetRow, num.Value].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                worksheet.Cells[targetRow, num.Value].Style.Fill.BackgroundColor.SetColor(Color.Red);
                                worksheet.Cells[targetRow, num.Value].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                                worksheet.Cells[targetRow, num.Value].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                                worksheet.Cells[targetRow, num.Value].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                                worksheet.Cells[targetRow, num.Value].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                            }
                        }
                    }
                    var messages = Shared.Helpers.StringHelper.JoinWithComma(errorMessages.Distinct().ToList());
                    worksheet.Cells[targetRow, 2].Value = messages;
                }
            };
            var resultData = request.FormFile.ImportAndValidateExcel(async (ExcelUserBlindBagEventModel x, IList<ExcelUserBlindBagEventModel> models, int rowIndex, IList<ValidateExcelModel> errors) =>
            {
                return await Task.FromResult(errors.Count == 0);
            });

            var blindBoxResult = await _systemService.GetBlindBoxAsync();
            if (!blindBoxResult.IsSuccessStatusCode)
            {
                methodResult.AddError(blindBoxResult.Error);
                return methodResult;
            }
            var blindBox = blindBoxResult.Content?.Result;

            var userNames = resultData.Datas.Select(x => x.UserName).ToList();
            var users = await _userManager.Users.WhereBulkContains(userNames, x => x.UserName).ToListAsync(cancellationToken);

            var userBlindBoxResults = await _systemService.GetBlindBoxesByUserIdsAsync(new GetBlindBoxesByUserIdsQueryModel
            {
                UserIds = users.Select(x => x.Id).ToList()
            });

            var userBlindBoxIds = userBlindBoxResults.Content?.Result;

            var orderResults = await _orderService.GetRecentOrdersAsync(new GetRecentOrdersToUserIdsQueryModel
            {
                UserIds = users.Select(x => x.Id).ToList(),
                StartDate = blindBox?.StartDate ?? DateTime.UtcNow,
            });
            var orders = orderResults.Content?.Result;

            var result = request.FormFile.ImportAndValidateExcel(async (ExcelUserBlindBagEventModel x, IList<ExcelUserBlindBagEventModel> models, int rowIndex, IList<ValidateExcelModel> errors) =>
            {
                var user = users.FirstOrDefault(y => y.UserName == x.UserName);
                var order = orders?.FirstOrDefault(y => user != null && y.UserId == user.Id);

                if (string.IsNullOrEmpty(x.UserName))
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.Status), Message = $"{nameof(x.UserName)} không được để trống" });
                }
                else
                {
                    if (models.Count(y => y.UserName == x.UserName) > 1)
                    {
                        errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.Status), Message = $"{nameof(x.UserName)} bị trùng với username đã có trong danh sách" });
                    }
                    else if (user == null)
                    {
                        errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.Status), Message = $"{nameof(x.UserName)} không tồn tại" });
                    }
                    else if (order == null)
                    {
                        errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.Status), Message = $"{nameof(x.UserName)} chưa mua gói học 12 tháng hoặc 24 tháng (tính từ thời điểm sự kiện túi mù bắt đầu)" });
                    }
                    else if (userBlindBoxIds != null && userBlindBoxIds.Any(y => user != null && y == user.Id))
                    {
                        errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.Status), Message = $"{nameof(x.UserName)} đã được thêm vào sự kiện túi mù" });
                    }
                }
                return await Task.FromResult(errors.Count == 0);
            }, errorHandlerAction: errorHandlerAction);

            if (result.Stream != null)
            {
                byte[] fileBytes;
                using (var memoryStream = new MemoryStream())
                {
                    await result.Stream.CopyToAsync(memoryStream, cancellationToken);
                    fileBytes = memoryStream.ToArray();
                }
                methodResult.Result = new CreateStudentsToEventFromFileModel() { File = fileBytes };
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }

            var blindBoxsResult = await _systemService.CreateBlindBoxesAsync(new CreateBlindBoxesCommandModel
            {
                UserIds = users.Select(x => x.Id).ToList()
            });
            methodResult.Result = new CreateStudentsToEventFromFileModel() { NumberOfStudent = blindBoxsResult.Content?.Result };
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
