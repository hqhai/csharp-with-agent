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
        public string? StudentId { get; set; }

        [EpplusTableColumn(Header = "Mã lỗi (hệ thống tự trả, không được điền)")]
        public string? Status { get; set; }
    }

    public class StudentUserModel
    {
        public Guid StudentId { get; set; }
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
        private readonly IHumanRepository _humanRepository;
        private readonly IStudentRepository _studentRepository;

        public ImportUsersToBlindBagEventCommandHandler(
            Microsoft.AspNetCore.Identity.UserManager<User> userManager,
            IOrderService orderService,
            ISystemService systemService,
            IHumanRepository humanRepository,
            IStudentRepository studentRepository)
        {
            _userManager = userManager;
            _orderService = orderService;
            _systemService = systemService;
            _humanRepository = humanRepository;
            _studentRepository = studentRepository;
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

            var studentIds = resultData.Datas.Where(x => !string.IsNullOrEmpty(x.StudentId) && Guid.TryParse(x.StudentId, out Guid studentId)).Select(x => new Guid(x.StudentId!)).ToList();
            var studentUsers = new List<StudentUserModel>();
            if (studentIds.Any())
            {
                studentUsers = await (from baseQ in _studentRepository.Queryable.WhereBulkContains(studentIds, x => x.Id)
                                      join human in _humanRepository.Queryable on baseQ.HumanId equals human.Id
                                      join user in _userManager.Users on human.UserId equals user.Id
                                      select new StudentUserModel
                                      {
                                          StudentId = baseQ.Id,
                                          UserId = user.Id
                                      }).ToListAsync(cancellationToken);
            }

            var userIds = studentUsers.Select(x => x.UserId).ToList();
            var userBlindBoxResults = await _systemService.GetBlindBoxesByUserIdsAsync(new GetBlindBoxesByUserIdsQueryModel
            {
                UserIds = userIds
            });

            var userBlindBoxIds = userBlindBoxResults.Content?.Result;

            var orderResults = await _orderService.GetRecentOrdersAsync(new GetRecentOrdersToUserIdsQueryModel
            {
                UserIds = userIds,
                StartDate = blindBox?.StartDate ?? DateTime.UtcNow,
            });
            var orders = orderResults.Content?.Result;

            var result = request.FormFile.ImportAndValidateExcel(async (ExcelUserBlindBagEventModel x, IList<ExcelUserBlindBagEventModel> models, int rowIndex, IList<ValidateExcelModel> errors) =>
            {
                if (string.IsNullOrEmpty(x.StudentId))
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.Status), Message = $"StudentId không được để trống" });
                }
                else if (!Guid.TryParse(x.StudentId, out Guid studentId))
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.Status), Message = $"StudentId không đúng định dạng" });
                }
                else
                {
                    var studentUser = studentUsers.FirstOrDefault(y => y.StudentId.ToString() == x.StudentId.ToLower(System.Globalization.CultureInfo.CurrentCulture));
                    var order = orders?.FirstOrDefault(y => studentUser != null && y.UserId == studentUser.UserId);

                    if (models.Count(y => y.StudentId == x.StudentId) > 1)
                    {
                        errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.Status), Message = $"StudentId bị trùng với StudentId đã có trong danh sách" });
                    }
                    else if (studentUser == null)
                    {
                        errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.Status), Message = $"User không tồn tại" });
                    }
                    else if (order == null)
                    {
                        errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.Status), Message = $"User chưa mua gói học 12 tháng hoặc 24 tháng (tính từ thời điểm sự kiện túi mù bắt đầu)" });
                    }
                    else if (userBlindBoxIds != null && userBlindBoxIds.Any(y => studentUser != null && y == studentUser.UserId))
                    {
                        errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.Status), Message = $"User đã được thêm vào sự kiện túi mù" });
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
                methodResult.Result = new CreateStudentsToEventFromFileModel() { File = fileBytes, Message = nameof(EnumSystemErrorCode.InValidFormat), StatusCode = StatusCodes.Status400BadRequest };
                return methodResult;
            }

            var blindBoxsResult = await _systemService.CreateBlindBoxesAsync(new CreateBlindBoxesCommandModel
            {
                UserIds = userIds
            });
            methodResult.Result = new CreateStudentsToEventFromFileModel() { NumberOfStudent = blindBoxsResult.Content?.Result, StatusCode = StatusCodes.Status200OK };
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
