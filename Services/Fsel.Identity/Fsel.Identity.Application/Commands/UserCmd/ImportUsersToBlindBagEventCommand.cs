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
    using Fsel.Identity.Application.Services.SystemService;
    using Fsel.Identity.Application.Services.SystemService.CommandModels;
    using Fsel.Identity.Application.Services.SystemService.QueryModels;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using OfficeOpenXml;
    using OfficeOpenXml.Attributes;
    using OfficeOpenXml.Style;

    public class ExcelUserBlindBagEventModel
    {
        [EpplusTableColumn(Header = "StudentId (mỗi dòng chỉ điền 1 StudentId)")]
        public string? HumanCode { get; set; }

        [EpplusTableColumn(Header = "Mã lỗi (hệ thống tự trả, không được điền)")]
        public string? Status { get; set; }
    }

    public class StudentUserModel
    {
        public string? HumanCode { get; set; }
        public Guid UserId { get; set; }
    }

    public class ImportUsersToBlindBagEventCommand : BaseImportCommandModel, IRequest<MethodResult<CreateStudentsToEventFromFileModel>>
    {
    }

    public class ImportUsersToBlindBagEventCommandHandler : IRequestHandler<ImportUsersToBlindBagEventCommand, MethodResult<CreateStudentsToEventFromFileModel>>
    {
        private readonly Microsoft.AspNetCore.Identity.UserManager<User> _userManager;
        private readonly IOrderService _orderService;
        private readonly ISystemService _systemService;
        private const string ErrorTemplate = "Định dạng File tải lên không hợp lệ. Vui lòng tải lại file template mẫu và thử lại.";
        private const string Success = "Thành công";
        private const string DataError = "Dữ liệu bị trống hoặc sai định dạng";
        private const string FileNull = "File tải lên không có dữ liệu. Vui lòng tải file template mẫu và thử lại.";
        private const string FileSizeExceededMessage = "Dung lượng File vượt quá 1MB";

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
            string allowedContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            if (request.FormFile == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.ImportFileRequired), nameof(request.FormFile));
                return methodResult;
            }

            long maxFileSize = 1 * 1024 * 1024; // 1MB
            if (request.FormFile.Length > maxFileSize)
            {
                methodResult.Result = new CreateStudentsToEventFromFileModel() { Message = FileSizeExceededMessage, StatusCode = StatusCodes.Status400BadRequest };
                return methodResult;
            }

            if (request.FormFile.ContentType != allowedContentType)
            {
                methodResult.Result = new CreateStudentsToEventFromFileModel() { Message = ErrorTemplate, StatusCode = StatusCodes.Status400BadRequest };
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

            var humanCodes = resultData.Datas.Where(x => !string.IsNullOrEmpty(x.HumanCode)).Select(x => x.HumanCode!.Trim().ToLower(System.Globalization.CultureInfo.CurrentCulture)).ToList();
            var studentUsers = new List<StudentUserModel>();
            if (humanCodes.Any())
            {
                studentUsers = await (from user in _userManager.Users.WhereBulkContains(humanCodes, x => x.Code)
                                      select new StudentUserModel
                                      {
                                          HumanCode = user.Code,
                                          UserId = user.Id
                                      }).ToListAsync(cancellationToken);
            }

            var userIds = studentUsers.Select(x => x.UserId).ToList();
            var userBlindBoxResults = await _systemService.GetBlindBoxesByUserIdsAsync(new GetBlindBoxesByUserIdsQueryModel
            {
                UserIds = userIds
            });

            var userBlindBoxIds = userBlindBoxResults.Content?.Result;

            //var orderResults = await _orderService.GetRecentOrdersAsync(new GetRecentOrdersToUserIdsQueryModel
            //{
            //    UserIds = userIds,
            //    StartDate = blindBox?.StartDate ?? DateTime.UtcNow,
            //});
            //var orders = orderResults.Content?.Result;

            var result = request.FormFile.ImportAndValidateExcel(async (ExcelUserBlindBagEventModel x, IList<ExcelUserBlindBagEventModel> models, int rowIndex, IList<ValidateExcelModel> errors) =>
            {
                if (string.IsNullOrEmpty(x.HumanCode))
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.Status), Message = $"Student ID không được để trống" });
                }
                else
                {
                    string humanCode = x.HumanCode.Trim().ToLower(System.Globalization.CultureInfo.CurrentCulture);
                    var studentUser = studentUsers.FirstOrDefault(y => y.HumanCode != null && y.HumanCode.Trim().ToLower(System.Globalization.CultureInfo.CurrentCulture) == humanCode);
                    //var order = orders?.FirstOrDefault(y => studentUser != null && y.UserId == studentUser.UserId);

                    if (models.Count(y => y.HumanCode != null && y.HumanCode.Trim().ToLower(System.Globalization.CultureInfo.CurrentCulture) == humanCode) > 1)
                    {
                        errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.Status), Message = $"Student ID bị trùng với Student ID đã có trong danh sách" });
                    }
                    else if (studentUser == null)
                    {
                        errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.Status), Message = $"User không tồn tại" });
                    }
                    //else if (order == null)
                    //{
                    //    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.Status), Message = $"User chưa mua gói học 12 tháng hoặc 24 tháng (tính từ thời điểm sự kiện túi mù bắt đầu)" });
                    //}
                    else if (userBlindBoxIds != null && userBlindBoxIds.Any(y => studentUser != null && y == studentUser.UserId))
                    {
                        errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.Status), Message = $"User đã được thêm vào sự kiện túi mù" });
                    }
                }

                return await Task.FromResult(errors.Count == 0);
            }, errorHandlerAction: errorHandlerAction);

            byte[] fileBytes;
            using (var memoryStream = new MemoryStream())
            {
                if (result.Stream != null)
                {
                    await result.Stream.CopyToAsync(memoryStream, cancellationToken);
                }
                else
                {
                    await request.FormFile.CopyToAsync(memoryStream, cancellationToken);
                }

                fileBytes = memoryStream.ToArray();
            }

            if (!result.IsValidHeader)
            {
                methodResult.Result = new CreateStudentsToEventFromFileModel() { File = fileBytes, Message = ErrorTemplate, StatusCode = StatusCodes.Status400BadRequest };
                return methodResult;
            }
            if (result.Stream != null)
            {
                methodResult.Result = new CreateStudentsToEventFromFileModel() { File = fileBytes, Message = DataError, StatusCode = StatusCodes.Status400BadRequest };
                return methodResult;
            }

            if (!humanCodes.Any())
            {
                methodResult.Result = new CreateStudentsToEventFromFileModel() { File = fileBytes, Message = FileNull, StatusCode = StatusCodes.Status400BadRequest };
                return methodResult;
            }

            var blindBoxsResult = await _systemService.CreateBlindBoxesAsync(new CreateBlindBoxesCommandModel
            {
                UserIds = userIds
            });
            methodResult.Result = new CreateStudentsToEventFromFileModel() { NumberOfStudent = blindBoxsResult.Content?.Result, Message = Success, StatusCode = StatusCodes.Status200OK };
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
