// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.CampusCmd.Classes
{
    using System.Collections.Concurrent;
    using System.Drawing;
    using System.Globalization;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Common.Models.Excels;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Application.Services.OrderService;
    using Fsel.Identity.Application.Services.SystemService;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models;
    using Fsel.Identity.Domain.Models.CommandModels.Campus;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels.CampusModel;
    using Kros.Extensions;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using OfficeOpenXml;
    using OfficeOpenXml.Style;

    public class AddStudentIntoSchoolClassCommand : BaseImportCommandModel, IRequest<MethodResult<AddStudentIntoSchoolClassCommandModel>>
    {
        public Guid SchoolClassId { get; set; }
    }

    public class AddStudentIntoSchoolClassCommandHandler : IRequestHandler<AddStudentIntoSchoolClassCommand, MethodResult<AddStudentIntoSchoolClassCommandModel>>
    {
        private readonly IHumanRepository _humanRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly UserManager<User> _userManager;
        private readonly IOrderService _orderService;
        private readonly IServiceProvider _serviceProvider;
        private readonly IPlatformRepository _platformRepository;
        private readonly ILogger<AddStudentIntoSchoolClassCommand> _logger;
        private readonly AuthContext _authContext;
        private readonly ISchoolClassRepository _schoolClassRepository;
        private readonly ISystemService _systemService;
        private const string DefaultPassword = "Fsel@";
        private const int MinYear = 1900;

        public AddStudentIntoSchoolClassCommandHandler(IHumanRepository humanRepository, IStudentRepository studentRepository, UserManager<User> userManager, IOrderService orderService, IServiceProvider serviceProvider, IPlatformRepository platformRepository, ILogger<AddStudentIntoSchoolClassCommand> logger, AuthContext authContext, ISchoolClassRepository schoolClassRepository, ISystemService systemService)
        {
            _humanRepository = humanRepository;
            _studentRepository = studentRepository;
            _userManager = userManager;
            _orderService = orderService;
            _serviceProvider = serviceProvider;
            _platformRepository = platformRepository;
            _logger = logger;
            _authContext = authContext;
            _schoolClassRepository = schoolClassRepository;
            _systemService = systemService;
        }

        public async Task<MethodResult<AddStudentIntoSchoolClassCommandModel>> Handle(AddStudentIntoSchoolClassCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<AddStudentIntoSchoolClassCommandModel>();

            var schoolIdStr = _authContext.ClaimsPrincipal?.FindFirstValue("SchoolId");

            if (string.IsNullOrEmpty(schoolIdStr) || !Guid.TryParse(schoolIdStr, out Guid schoolId))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(schoolId), _authContext.CurrentUserId);
                return methodResult;
            }

            var schoolResult = await _systemService.GetLocationByGlobalId(schoolIdStr);
            var school = schoolResult.Content?.Result;
            if (school == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(school), schoolIdStr);
                return methodResult;
            }

            var schoolClass = await _schoolClassRepository.GetByIdAsync(request.SchoolClassId);
            if (schoolClass == null)
            {
                methodResult.AddError(nameof(EnumSystemErrorCode.DataNotExist), nameof(schoolClass), request.SchoolClassId);
                return methodResult;
            }

            if (request.FormFile == null)
            {
                methodResult.AddError(nameof(EnumSystemErrorCode.ImportFileRequired));
                return methodResult;
            }

            Action<ExcelWorksheet, Dictionary<string, int?>?, IList<ValidateExcelModel>> errorHandlerAction = (worksheet, columnIndexes, errors) =>
            {
                worksheet.Cells[1, 5].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[1, 5].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[1, 5].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[1, 5].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[1, 5].Value = ErrorMassageSetting.ErrorMessage;
                worksheet.Cells[1, 5].Style.Font.Bold = true;
                foreach (var error in errors.GroupBy(x => x.RowIndex).Select(x => x).OrderBy(x => x.Key))
                {
                    var row = error.Key;

                    int lastColumn = 5;

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

            Action<ExcelWorksheet, Dictionary<string, int?>?, int, int, AddStudentsToSchoolClassModel> defaultStudentHandlerAction = (worksheet, columnIndexes, row, num, model) =>
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

            var students = new List<AddStudentsToSchoolClassModel>();

            int currentYear = DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam).Year;

            var result = request.FormFile.ImportAndValidateExcel(async (AddStudentsToSchoolClassModel x, IList<AddStudentsToSchoolClassModel> models, int rowIndex, IList<ValidateExcelModel> errors) =>
            {
                if (!string.IsNullOrEmpty(x.FullName?.Trim()) || !string.IsNullOrEmpty(x.PhoneNumber?.Trim()) || !string.IsNullOrEmpty(x.Email?.Trim()) || !x.DateOfBirth.HasValue)
                {
                    if (string.IsNullOrEmpty(x.FullName?.Trim()))
                    {
                        errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.FullName), Message = ErrorMassageSetting.EmptyFullNameVN });
                    }
                    if (string.IsNullOrEmpty(x.Email?.Trim()))
                    {
                        errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.Email), Message = ErrorMassageSetting.EmptyEmailVN });
                    }
                    else if (!x.Email.IsValidEmail())
                    {
                        errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.Email), Message = ErrorMassageSetting.InvalidEmailVN });
                    }
                    else if (x.Email.Length > 70)
                    {
                        errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.Email), Message = ErrorMassageSetting.InvalidEmailVN });
                    }
                    if (!string.IsNullOrEmpty(x.PhoneNumber?.Trim()) && !Shared.Helpers.StringHelper.IsValidPhoneNumber(x.PhoneNumber?.Trim()))
                    {
                        errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.PhoneNumber), Message = ErrorMassageSetting.InvalidPhoneNumberVN });
                    }

                    if (!x.DateOfBirth.HasValue)
                    {
                        errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.PhoneNumber), Message = ErrorMassageSetting.EmptyBirthDayVN });
                    }
                    else if (x.DateOfBirth <= MinYear || x.DateOfBirth >= currentYear)
                    {
                        errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.DateOfBirth), Message = ErrorMassageSetting.InvalidBirthDayVN });
                    }

                    if (!string.IsNullOrEmpty(x.PhoneNumber) && students.Where(p => !string.IsNullOrEmpty(p.PhoneNumber)).Select(p => p.PhoneNumber).Contains(x.PhoneNumber))
                    {
                        errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.PhoneNumber), Message = ErrorMassageSetting.PhoneNumberAlreadyExistInListVN });
                    }

                    if (students.Select(p => p.Email).Contains(x.Email))
                    {
                        errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.Email), Message = ErrorMassageSetting.EmailAlreadyExistInListVN });
                    }

                    students.Add(new AddStudentsToSchoolClassModel()
                    {
                        FullName = x.FullName,
                        Email = x.Email,
                        PhoneNumber = x.PhoneNumber,
                        DateOfBirth = x.DateOfBirth,
                    });
                }

                return await Task.FromResult(errors.Count == 0);
            },
            async (Dictionary<int, AddStudentsToSchoolClassModel> datas, IList<ValidateExcelModel> errors) =>
            {
                var emails = datas.Values.Where(p => p.Email != null && !string.IsNullOrEmpty(p.Email.Trim())).Select(n => n.Email?.Trim() ?? string.Empty);

                var phoneNumbers = datas.Values.Where(p => p.PhoneNumber != null && !string.IsNullOrEmpty(p.PhoneNumber.Trim())).Select(n => Shared.Helpers.StringHelper.NormalizeToDomesticFormat(n.PhoneNumber?.Trim()));

                var emailsAlreadyExist = _userManager.Users.Where(x => emails.Contains(x.UserName)).Select(p => p.UserName);

                var phoneNumbersAlreadyExist = _userManager.Users.Where(x => phoneNumbers.Contains(x.PhoneNumber)).Select(p => p.PhoneNumber);

                emailsAlreadyExist.ForEach(user =>
                {
                    var dataByEmail = datas.Values.Where(x => !x.Email.IsNullOrEmpty())
                                                  .FirstOrDefault
                                                  (x =>
                                                  !string.IsNullOrEmpty(user)
                                                  &&
                                                  x.Email?.ToLower(CultureInfo.InvariantCulture).Trim() == user.ToLower(CultureInfo.InvariantCulture).Trim()
                                                  );

                    if (dataByEmail != null)
                    {
                        var index = datas.FirstOrDefault(x => x.Value == dataByEmail).Key;
                        errors.Add(new ValidateExcelModel { RowIndex = index, ColumnName = nameof(dataByEmail.Email), Message = ErrorMassageSetting.DataAlreadyExistVN });
                    }
                });

                phoneNumbersAlreadyExist.ForEach(user =>
                {
                    var dataByPhoneNumber = datas.Values.Where(x => !x.PhoneNumber.IsNullOrEmpty())
                                                        .FirstOrDefault
                                                        (x =>
                                                        !string.IsNullOrEmpty(user)
                                                        && x.PhoneNumber?.Trim() == user.Trim()
                                                        );

                    if (dataByPhoneNumber != null)
                    {
                        var index = datas.FirstOrDefault(x => x.Value == dataByPhoneNumber).Key;
                        errors.Add(new ValidateExcelModel { RowIndex = index, ColumnName = nameof(dataByPhoneNumber.PhoneNumber), Message = ErrorMassageSetting.PhoneNumberAlreadyExistVN });
                    }
                });

                return await Task.FromResult(errors.Count == 0);
            },
             defaultStudentHandlerAction,
             errorHandlerAction,
             true);

            if (!result.IsValidHeader)
            {
                methodResult.AddErrorBadRequest(ErrorMassageSetting.ErrorTemplate);
                return methodResult;
            }

            if (result.Stream != null)
            {
                methodResult.Result = new AddStudentIntoSchoolClassCommandModel() { Stream = result.Stream };
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }

            if (!students.Any())
            {
                methodResult.AddErrorBadRequest(ErrorMassageSetting.FileNull);
                return methodResult;
            }

            var platform = await _platformRepository.GetPlatformAsync(EnumPlatformCode.LMS, cancellationToken);
            if (platform == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = 50
            };

            var createOrdersForStudentCampusModel = new ConcurrentBag<CreateOrdersForStudentCampusCommandModel>();

            await Parallel.ForEachAsync(students, parallelOptions, async (student, cancellationToken) =>
            {
                if (!string.IsNullOrEmpty(student.Email?.Trim()))
                {
                    try
                    {
                        using (var scope = _serviceProvider.CreateScope())
                        {
                            var password = DefaultPassword + Shared.Helpers.StringHelper.GenerateLaterPartPassword(4);

                            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
                            var studentRepository = scope.ServiceProvider.GetRequiredService<IStudentRepository>();
                            Microsoft.AspNetCore.Identity.IdentityResult identityStudentResult;

                            DateTime dateOfBirth = new DateTime(student.DateOfBirth ?? (currentYear - 1), 01, 01);

                            int age = Shared.Helpers.DateTimeHelper.GetYearOld(dateOfBirth);

                            var user = new User()
                            {
                                UserName = !string.IsNullOrEmpty(student.Email) ? student.Email.ToLower(cultureInfo).Trim() : null,
                                Email = !string.IsNullOrEmpty(student.Email) ? student.Email.ToLower(cultureInfo).Trim() : null,
                                FullName = student.FullName?.Trim() ?? string.Empty,
                                EmailConfirmed = true,
                                PhoneNumber = !string.IsNullOrEmpty(student.PhoneNumber) ? Shared.Helpers.StringHelper.NormalizeToDomesticFormat(student.PhoneNumber.Trim()) : null,
                                PhoneNumberConfirmed = false,
                                Status = EnumUserStatus.Active,
                                DefaultPassword = password,
                                Human = new Human()
                                {
                                    FullName = student.FullName?.Trim(),
                                    PhoneNumber = !string.IsNullOrEmpty(student.PhoneNumber) ? Shared.Helpers.StringHelper.NormalizeToDomesticFormat(student.PhoneNumber.Trim()) : null,
                                    Birthday = dateOfBirth,
                                    Email = !string.IsNullOrEmpty(student.Email) ? student.Email.ToLower(cultureInfo).Trim() : null,
                                    Code = GeneratorCodeAsync(studentRepository, dateOfBirth, null),
                                    Student = new Student()
                                    {
                                        CreatedByParent = false,
                                        Occupation = nameof(Student),
                                        SchoolClass = schoolClass.Name,
                                        SchoolClassId = request.SchoolClassId,
                                        SchoolId = schoolId,
                                        School = school.LocationName,
                                        CourseLevel = age <= 13 ? EnumCourseLevel.A2 : EnumCourseLevel.B1,
                                    }
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
                                await userManager.AddToRoleAsync(user, EnumRole.StudentCampus.ToString());

                                createOrdersForStudentCampusModel.Add(new CreateOrdersForStudentCampusCommandModel()
                                {
                                    UserId = user.Id,
                                    StudentId = user.Human.Student.Id,
                                    Email = user.Email,
                                    PhoneNumber = user.PhoneNumber,
                                    FullName = user.FullName,
                                    StudentCode = user.Human.Code
                                });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "CreateAsync error");
                    }
                }
            });

            var createOrdersResult = await _orderService.CreateOrderForStudentCampus(new CreateOrdersForStudentCampusCommandModels()
            {
                Students = createOrdersForStudentCampusModel.ToList()
            });

            methodResult.Result = new AddStudentIntoSchoolClassCommandModel() { NumberOfStudent = createOrdersForStudentCampusModel.Count };

            methodResult.StatusCode = StatusCodes.Status200OK;

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
