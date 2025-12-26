// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.StudentCmd
{
    using System.Drawing;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Common.Models.Excels;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.CommandModels.Students;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Kros.Extensions;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using OfficeOpenXml;
    using OfficeOpenXml.Style;

    public class ImportStudentsIntoPlatformCommand : BaseImportCommandModel, IRequest<MethodResult<Stream>>
    {
    }

    public class ImportStudentsIntoPlatformCommandHandler : IRequestHandler<ImportStudentsIntoPlatformCommand, MethodResult<Stream>>
    {
        private readonly UserManager<User> _userManager;
        private readonly IPlatformRepository _platformRepository;
        private readonly IServiceProvider _serviceProvider;
        private const string ErrorMessage = "Thông báo lỗi";
        private const string FileNull = "File tải lên không có dữ liệu";

        private const int StartYear = 1900;

        public ImportStudentsIntoPlatformCommandHandler(UserManager<User> userManager, IPlatformRepository platformRepository, IServiceProvider serviceProvider)
        {
            _userManager = userManager;
            _platformRepository = platformRepository;
            _serviceProvider = serviceProvider;
        }

        public async Task<MethodResult<Stream>> Handle(ImportStudentsIntoPlatformCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<Stream>();

            try
            {
                if (request.FormFile == null)
                {
                    methodResult.AddError(nameof(EnumSystemErrorCode.ImportFileRequired));
                    return methodResult;
                }

                Action<ExcelWorksheet, Dictionary<string, int?>?, IList<ValidateExcelModel>> errorHandlerAction = (worksheet, columnIndexes, errors) =>
                {
                    worksheet.Cells[1, 9].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    worksheet.Cells[1, 9].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    worksheet.Cells[1, 9].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    worksheet.Cells[1, 9].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    worksheet.Cells[1, 9].Value = ErrorMessage;
                    worksheet.Cells[1, 9].Style.Font.Bold = true;
                    foreach (var error in errors.GroupBy(x => x.RowIndex).Select(x => x).OrderBy(x => x.Key))
                    {
                        var row = error.Key;

                        int lastColumn = 9;

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
                        worksheet.Cells[targetRow, lastColumn].Value = messages;
                    }
                };

                Action<ExcelWorksheet, Dictionary<string, int?>?, int, int, ImportStudentToPlatformModel> defaultHandlerAction = (worksheet, columnIndexes, row, num, model) =>
                {
                    worksheet.Cells[row, num].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[row, num].Style.Fill.BackgroundColor.SetColor(Color.White);
                    worksheet.Cells[row, num].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    worksheet.Cells[row, num].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    worksheet.Cells[row, num].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    worksheet.Cells[row, num].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    if (num == 6)
                    {
                        worksheet.Cells[row, num + 1].Value = null;
                    }
                };

                var cultureInfo = CultureInfo.InvariantCulture;

                var result = request.FormFile.ImportAndValidateExcel(async (ImportStudentToPlatformModel x, IList<ImportStudentToPlatformModel> models, int rowIndex, IList<ValidateExcelModel> errors) =>
                {
                    if (!string.IsNullOrEmpty(x.FullName?.Trim()) || x.DateOfBirth.HasValue || !string.IsNullOrEmpty(x.Email?.Trim()))
                    {
                        if (string.IsNullOrEmpty(x.FullName?.Trim()))
                        {
                            errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.FullName), Message = ErrorMassageSetting.EmptyFullNameVN });
                        }
                        else if (!Shared.Helpers.StringHelper.ContainsSpecialChars(x.FullName.Trim()))
                        {
                            errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.FullName), Message = ErrorMassageSetting.InvalidFullNameVN });
                        }

                        if (!string.IsNullOrEmpty(x.PhoneNumber?.Trim()) && !Shared.Helpers.StringHelper.IsValidPhoneNumber(x.PhoneNumber?.Trim()))
                        {
                            errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.PhoneNumber), Message = ErrorMassageSetting.InvalidPhoneNumberVN });
                        }

                        if (string.IsNullOrEmpty(x.Email?.Trim()))
                        {
                            errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.Email), Message = ErrorMassageSetting.EmptyEmailVN });
                        }
                        else if (!x.Email.Trim().IsValidEmail())
                        {
                            errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.Email), Message = ErrorMassageSetting.InvalidEmailVN });
                        }

                        if (!x.DateOfBirth.HasValue)
                        {
                            errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.DateOfBirth), Message = ErrorMassageSetting.EmptyBirthDayVN });
                        }
                        else if (x.DateOfBirth.HasValue && x.DateOfBirth.Value.Year < StartYear)
                        {
                            errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.DateOfBirth), Message = ErrorMassageSetting.InvalidBirthDayVN });
                        }
                    }
                    return await Task.FromResult(errors.Count == 0);
                },
                async (Dictionary<int, ImportStudentToPlatformModel> datas, IList<ValidateExcelModel> errors) =>
                {
                    var emails = datas.Values.Where(p => p.Email != null && !string.IsNullOrEmpty(p.Email.Trim())).Select(n => n.Email!.Trim());

                    var emailQuery = _userManager.Users.Where(x => emails.Contains(x.Email));
                    var userNameQuery = _userManager.Users.Where(x => emails.Contains(x.UserName));

                    var usersExist = await emailQuery
                        .Union(userNameQuery)
                        .ToArrayAsync(cancellationToken);

                    usersExist.ForEach(user =>
                    {
                        var dataByEmail = datas.Values.Where(x => !x.Email.IsNullOrEmpty()).FirstOrDefault(x => (!string.IsNullOrEmpty(user.Email) && x.Email.ToLower() == user.Email.ToLower()) || (!string.IsNullOrEmpty(user.UserName) && x.Email.ToLower() == user.UserName.ToLower()));
                        if (dataByEmail != null)
                        {
                            var index = datas.FirstOrDefault(x => x.Value == dataByEmail).Key;
                            errors.Add(new ValidateExcelModel { RowIndex = index, ColumnName = nameof(dataByEmail.Email), Message = ErrorMassageSetting.EmailAlreadyExistVN });
                        }
                    });

                    return await Task.FromResult(errors.Count == 0);
                },
                defaultHandlerAction,
                errorHandlerAction,
                true);

                if (!result.IsValidHeader)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat));
                    return methodResult;
                }

                if (result.Stream != null)
                {
                    methodResult.Result = result.Stream;
                    methodResult.StatusCode = StatusCodes.Status400BadRequest;
                    return methodResult;
                }

                var platform = await _platformRepository.GetPlatformAsync(EnumPlatformCode.LMS, cancellationToken);
                if (platform == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                    return methodResult;
                }

                var students = result.Datas.ToList();
                if (students == null || students.Count == 0)
                {
                    methodResult.AddErrorBadRequest(FileNull);
                    return methodResult;
                }

                var parallelOptions = new ParallelOptions
                {
                    MaxDegreeOfParallelism = 50
                };

                var password = $"Fsel@{DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam).Year}";

                await Parallel.ForEachAsync(students, parallelOptions, async (student, cancellationToken) =>
                {
                    if (!string.IsNullOrEmpty(student.Email?.Trim()))
                    {
                        try
                        {
                            using (var scope = _serviceProvider.CreateScope())
                            {
                                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
                                var studentRepository = scope.ServiceProvider.GetRequiredService<IStudentRepository>();
                                Microsoft.AspNetCore.Identity.IdentityResult identityStudentResult;
                                int age = Shared.Helpers.DateTimeHelper.GetYearOld(student.DateOfBirth);
                                var user = new User()
                                {
                                    UserName = student.Email.ToLower(cultureInfo).Trim(),
                                    Email = !string.IsNullOrEmpty(student.Email) ? student.Email.ToLower(cultureInfo).Trim() : null,
                                    LastName = student.FullName?.Trim().ParseFullName().LastName,
                                    FirstName = student.FullName?.Trim().ParseFullName().FirstName,
                                    PhoneNumber = !string.IsNullOrEmpty(student.PhoneNumber?.Trim()) ? Shared.Helpers.StringHelper.NormalizeToDomesticFormat(student.PhoneNumber.Trim()) : null,
                                    Birthday = student.DateOfBirth,
                                    Code = GeneratorCodeAsync(studentRepository, student.DateOfBirth ?? DateTime.MinValue, null),
                                    EmailConfirmed = true,
                                    PhoneNumberConfirmed = false,
                                    Status = EnumUserStatus.Active,
                                    DefaultPassword = password,
                                    Student = new Student()
                                    {
                                        CreatedByParent = false,
                                        Occupation = nameof(Student),
                                        SchoolClass = student.SchoolClass,
                                        SchoolGrade = student.SchoolGrade,
                                        CourseLevel = age <= 13 ? EnumCourseLevel.A2 : EnumCourseLevel.B1,
                                    },
                                    UserPlatforms = new List<UserPlatform>()
                                    {
                                        new UserPlatform()
                                        {
                                            PlatformId = platform.Id
                                        }
                                    },
                                    UserSettings = new List<UserSetting>()
                                    {
                                        new UserSetting(true)
                                    }
                                };

                                identityStudentResult = await userManager.CreateAsync(user, password);
                                if (identityStudentResult.Succeeded)
                                {
                                    await userManager.AddToRoleAsync(user, EnumRole.Student.ToString());
                                }
                            }
                        }
                        catch
                        {
                        }
                    }
                });

                return methodResult;
            }
            catch (Exception ex)
            {
                methodResult.AddErrorBadRequest(ex.Message);
            }

            return methodResult;
        }

        private static string GeneratorCodeAsync(IStudentRepository studentRepository, DateTime birthDay, EnumGender? gender)
        {
            var stt = studentRepository.GetNextSequenceValue<int>(SqlSettings.Sequence.UserSequence);
            var currentDate = DateTime.UtcNow;
            var weekNumber = (currentDate.DayOfYear - 1) / 7 + 1;
            var lastDigitOfYear = currentDate.Year % 10;
            var lastOfBirthDay = birthDay.Year % 100;
            var number = gender == EnumGender.Male ? 0 : gender == EnumGender.Female ? 1 : 2;
            var code = $"HN_{weekNumber}{lastDigitOfYear}{number}{lastOfBirthDay}{stt:D3}";
            return code;
        }
    }
}
