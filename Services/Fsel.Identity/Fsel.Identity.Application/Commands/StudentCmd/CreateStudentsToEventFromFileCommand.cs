// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.StudentCmd
{
    using System;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Common.Models.Excels;
    using Fsel.Core.Base;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Application.Queues.Publishers;
    using Fsel.Identity.Application.Services.InteractionService;
    using Fsel.Identity.Application.Services.OrderService;
    using Fsel.Identity.Application.Services.SystemService;
    using Fsel.Identity.Application.Services.SystemService.QueryModels;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.CommandModels.Students;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.IdentityModel.Tokens;
    using OfficeOpenXml;
    using OfficeOpenXml.Style;

    public class CreateStudentsToEventFromFileCommand : CreateStudentsToEventFromByteModel, IRequest<MethodResult<CreateStudentsToEventFromFileModel>>
    {
    }

    public class CreateStudentsToEventFromFileCommandHandler : IRequestHandler<CreateStudentsToEventFromFileCommand, MethodResult<CreateStudentsToEventFromFileModel>>
    {
        private readonly UserManager<User> _userManager;
        private readonly IOrderService _orderService;
        private readonly IPlatformRepository _platformRepository;
        private readonly IInteractionService _interactionService;
        private readonly IMediator _mediator;
        private readonly IHumanRepository _humanRepository;
        private readonly ICompetitionEventsRepository _competitionEventsRepository;
        private readonly IStudentCompetitionEventsRepository _studentCompetitionEventsRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly IServiceProvider _serviceProvider;
        private readonly SendStudentsFromFilePublisher _sendStudentsFromFilePublisher;
        private readonly ISchoolImportHistoryRepository _schoolImportHistoryRepository;
        private readonly ILogger<CreateStudentsToEventFromFileCommand> _logger;
        private readonly AuthContext _authContext;
        private readonly ISystemService _systemService;

        private const string ErrorTemplate = "Template bị sai, kiểm tra lại tên cột, bạn cần download template ở nút Tải Template mẫu";
        private const string Success = "Thành công";
        private const string ErrorMessage = "Error Message\n(Thông báo lỗi)";
        private const string FileNull = "File tải lên không có dữ liệu";
        private const string DataError = "Dữ liệu bị trống hoặc sai định dạng";

        public CreateStudentsToEventFromFileCommandHandler(UserManager<User> userManager, IOrderService orderService, IPlatformRepository platformRepository, IInteractionService interactionService, IMediator mediator, IHumanRepository humanRepository, ICompetitionEventsRepository competitionEventsRepository, IStudentCompetitionEventsRepository studentCompetitionEventsRepository, IStudentRepository studentRepository, IServiceProvider serviceProvider, SendStudentsFromFilePublisher sendStudentsFromFilePublisher, ISchoolImportHistoryRepository schoolImportHistoryRepository, ILogger<CreateStudentsToEventFromFileCommand> logger, AuthContext authContext, ISystemService systemService)
        {
            _userManager = userManager;
            _orderService = orderService;
            _platformRepository = platformRepository;
            _interactionService = interactionService;
            _mediator = mediator;
            _humanRepository = humanRepository;
            _competitionEventsRepository = competitionEventsRepository;
            _studentCompetitionEventsRepository = studentCompetitionEventsRepository;
            _studentRepository = studentRepository;
            _serviceProvider = serviceProvider;
            _sendStudentsFromFilePublisher = sendStudentsFromFilePublisher;
            _schoolImportHistoryRepository = schoolImportHistoryRepository;
            _logger = logger;
            _authContext = authContext;
            _systemService = systemService;
        }

        public async Task<MethodResult<CreateStudentsToEventFromFileModel>> Handle(CreateStudentsToEventFromFileCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<CreateStudentsToEventFromFileModel>();

            try
            {
                if (request.File == null)
                {
                    methodResult.AddError(nameof(EnumSystemErrorCode.ImportFileRequired));
                    return methodResult;
                }

                var currentUser = await _userManager.Users.Include(p => p.UserSchools).FirstOrDefaultAsync(p => p.Id == _authContext.CurrentUserId, cancellationToken);
                if (currentUser == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(currentUser));
                    return methodResult;
                }

                var schoolId = currentUser.UserSchools.FirstOrDefault()?.SchoolId;
                if (!schoolId.HasValue)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(schoolId));
                    return methodResult;
                }

                var schoolResult = await _systemService.GetLocationByIdsAsync(new GetLocationsByIdsQueryModel()
                {
                    Ids = new List<Guid>() { schoolId.Value }
                });
                var school = schoolResult.Content?.Result?.FirstOrDefault();
                if (school == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(school));
                    return methodResult;
                }

                var competitionEvents = _competitionEventsRepository.Queryable;
                var competitionEvent = competitionEvents.Where(p => p.SchoolIds != null && p.SchoolIds.Contains(schoolId.Value) && p.Category == request.Category).FirstOrDefault();
                if (competitionEvent == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(competitionEvent));
                    return methodResult;
                }

                Action<ExcelWorksheet, Dictionary<string, int?>?, IList<ValidateExcelModel>> errorHandlerAction = (worksheet, columnIndexes, errors) =>
                {
                    worksheet.Cells[1, 7].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    worksheet.Cells[1, 7].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    worksheet.Cells[1, 7].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    worksheet.Cells[1, 7].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    worksheet.Cells[1, 7].Value = ErrorMessage;
                    worksheet.Cells[1, 7].Style.Font.Bold = true;
                    foreach (var error in errors.GroupBy(x => x.RowIndex).Select(x => x).OrderBy(x => x.Key))
                    {
                        var row = error.Key;

                        int lastColumn = 7;

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

                Action<ExcelWorksheet, Dictionary<string, int?>?, int, int, CreateStudentToEventFromFileModel> defaultHandlerAction = (worksheet, columnIndexes, row, num, model) =>
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

                var stream = new MemoryStream(request.File);
                IFormFile formFile = new FormFile(stream, 0, request.File.Length, "name", "fileName");

                var cultureInfo = CultureInfo.InvariantCulture;

                var emails = new List<string>();
                var phoneNumbers = new List<string>();

                var result = formFile.ImportAndValidateExcel(async (CreateStudentToEventFromFileModel x, IList<CreateStudentToEventFromFileModel> models, int rowIndex, IList<ValidateExcelModel> errors) =>
                {
                    if (!string.IsNullOrEmpty(x.FullName?.Trim()) || !string.IsNullOrEmpty(x.PhoneNumber?.Trim()) || x.DateOfBirth != null || !string.IsNullOrEmpty(x.SchoolGrade?.Trim()) || !string.IsNullOrEmpty(x.SchoolClass?.Trim()))
                    {
                        if (string.IsNullOrEmpty(x.FullName?.Trim()))
                        {
                            errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.FullName), Message = ErrorMassageSetting.EmptyFullNameVN });
                        }
                        if (string.IsNullOrEmpty(x.PhoneNumber?.Trim()))
                        {
                            errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.PhoneNumber), Message = ErrorMassageSetting.EmptyPhoneNumberVN });
                        }
                        else if (!Shared.Helpers.StringHelper.IsValidPhoneNumber(x.PhoneNumber?.Trim()))
                        {
                            errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.PhoneNumber), Message = ErrorMassageSetting.InvalidPhoneNumberVN });
                        }
                        else if (phoneNumbers.Contains(Shared.Helpers.StringHelper.NormalizeToDomesticFormat(x.PhoneNumber?.Trim())))
                        {
                            errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.PhoneNumber), Message = ErrorMassageSetting.PhoneNumberAlreadyExistInListVN });
                        }
                        else
                        {
                            x.PhoneNumber = Shared.Helpers.StringHelper.NormalizeToDomesticFormat(x.PhoneNumber?.Trim());
                            phoneNumbers.Add(x.PhoneNumber.Trim());
                        }
                        if (!string.IsNullOrEmpty(x.Email?.Trim()) && !x.Email.Trim().IsValidEmail())
                        {
                            errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.Email), Message = ErrorMassageSetting.InvalidEmailVN });
                        }
                        else if (!string.IsNullOrEmpty(x.Email?.Trim()) && emails.Contains(x.Email.ToLower(cultureInfo).Trim()))
                        {
                            errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.Email), Message = ErrorMassageSetting.EmailAlreadyExistInListVN });
                        }
                        else if (!string.IsNullOrEmpty(x.Email?.Trim()))
                        {
                            emails.Add(x.Email.ToLower(cultureInfo).Trim());
                        }
                        if (x.DateOfBirth == null)
                        {
                            errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.DateOfBirth), Message = ErrorMassageSetting.EmptyBirthDayVN });
                        }
                        if (string.IsNullOrEmpty(x.SchoolGrade?.Trim()))
                        {
                            errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.SchoolGrade), Message = ErrorMassageSetting.EmptyGradeVN });
                        }
                        if (string.IsNullOrEmpty(x.SchoolClass?.Trim()))
                        {
                            errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.SchoolClass), Message = ErrorMassageSetting.EmptyClassVN });
                        }
                    }
                    return await Task.FromResult(errors.Count == 0);
                },
                async (Dictionary<int, CreateStudentToEventFromFileModel> datas, IList<ValidateExcelModel> errors) =>
                {
                    var emails = datas.Values.Where(p => p.Email != null && !string.IsNullOrEmpty(p.Email.Trim())).Select(n => n.Email!.Trim());
                    var phoneNumbers = datas.Values.Where(p => p.PhoneNumber != null && !string.IsNullOrEmpty(p.PhoneNumber.Trim())).Select(n => n.PhoneNumber!.Trim());

                    var emailQuery = _userManager.Users.Where(x => emails.Contains(x.Email));
                    var phoneQuery = _userManager.Users.Where(x => phoneNumbers.Contains(x.PhoneNumber));

                    var usersExist = await emailQuery
                        .Union(phoneQuery)
                        .ToArrayAsync(cancellationToken);

                    usersExist.ForEach(user =>
                    {
                        var dataByEmail = datas.Values.Where(x => !x.Email.IsNullOrEmpty()).FirstOrDefault(x => (!string.IsNullOrEmpty(user.Email) && x.Email.ToLower() == user.Email.ToLower()) || (!string.IsNullOrEmpty(user.UserName) && x.Email.ToLower() == user.UserName.ToLower()));
                        if (dataByEmail != null)
                        {
                            var index = datas.FirstOrDefault(x => x.Value == dataByEmail).Key;
                            errors.Add(new ValidateExcelModel { RowIndex = index, ColumnName = nameof(dataByEmail.Email), Message = ErrorMassageSetting.EmailAlreadyExistVN });
                        }

                        var dataByPhoneNumber = datas.Values.Where(x => !x.PhoneNumber.IsNullOrEmpty()).FirstOrDefault(x => x.PhoneNumber == user.PhoneNumber || x.PhoneNumber == user.UserName);
                        if (dataByPhoneNumber != null)
                        {
                            var index = datas.FirstOrDefault(x => x.Value == dataByPhoneNumber).Key;
                            errors.Add(new ValidateExcelModel { RowIndex = index, ColumnName = nameof(dataByPhoneNumber.PhoneNumber), Message = ErrorMassageSetting.PhoneNumberAlreadyExistVN });
                        }
                    });

                    return await Task.FromResult(errors.Count == 0);
                },
                defaultHandlerAction,
                errorHandlerAction,
                true);

                if (!result.IsValidHeader)
                {
                    await _sendStudentsFromFilePublisher.Publish(new CreateStudentsToEventFromFileModel()
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Key = request.Key,
                        Message = ErrorTemplate,
                    }, cancellationToken);
                    return methodResult;
                }

                if (result.Stream != null)
                {
                    var file = ConvertHelper.StreamToByteArray(result.Stream);
                    await _sendStudentsFromFilePublisher.Publish(new CreateStudentsToEventFromFileModel()
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        File = file,
                        Key = request.Key,
                        Message = DataError
                    }, cancellationToken);
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
                    await _sendStudentsFromFilePublisher.Publish(new CreateStudentsToEventFromFileModel()
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Key = request.Key,
                        Message = FileNull
                    }, cancellationToken);
                    return methodResult;
                }

                var users = new List<User>();
                var studentIds = new List<Guid>();
                var parallelOptions = new ParallelOptions
                {
                    MaxDegreeOfParallelism = 50
                };

                await Parallel.ForEachAsync(students, parallelOptions, async (student, cancellationToken) =>
                {
                    if (!string.IsNullOrEmpty(student.PhoneNumber?.Trim()))
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
                                    UserName = Shared.Helpers.StringHelper.NormalizeToDomesticFormat(student.PhoneNumber),
                                    Email = !string.IsNullOrEmpty(student.Email) ? student.Email.ToLower(cultureInfo).Trim() : null,
                                    FullName = student.FullName!.Trim(),
                                    PhoneNumber = Shared.Helpers.StringHelper.NormalizeToDomesticFormat(student.PhoneNumber),
                                    EmailConfirmed = false,
                                    PhoneNumberConfirmed = false,
                                    Status = EnumUserStatus.Inactive,
                                    Human = new Human()
                                    {
                                        FullName = student.FullName.Trim(),
                                        PhoneNumber = Shared.Helpers.StringHelper.NormalizeToDomesticFormat(student.PhoneNumber),
                                        Birthday = student.DateOfBirth,
                                        Email = !string.IsNullOrEmpty(student.Email) ? student.Email.ToLower(cultureInfo).Trim() : null,
                                        Code = GeneratorCodeAsync(studentRepository, student.DateOfBirth ?? DateTime.MinValue, null),
                                        Student = new Student()
                                        {
                                            CreatedByParent = false,
                                            Occupation = nameof(Student),
                                            School = school.Name,
                                            SchoolClass = student.SchoolClass,
                                            SchoolGrade = student.SchoolGrade,
                                            SchoolId = schoolId,
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

                                var password = Shared.Helpers.StringHelper.GeneratePassword(8);

                                identityStudentResult = await userManager.CreateAsync(user, password);
                                if (identityStudentResult.Succeeded)
                                {
                                    await userManager.AddToRoleAsync(user, EnumRole.Student.ToString());

                                    lock (studentIds)
                                    {
                                        studentIds.Add(user.Human.Student.Id);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "CreateAsync error");
                        }
                    }
                });

                var studentCompetitionEvents = new List<StudentCompetitionEvent>();
                studentIds.ForEach(p =>
                {
                    studentCompetitionEvents.Add(new StudentCompetitionEvent()
                    {
                        StudentId = p,
                        CompetitionEventId = competitionEvent.Id,
                    });
                });

                await _studentCompetitionEventsRepository.ExecuteTransactionAsync(async () =>
                {
                    await _studentCompetitionEventsRepository.AddList(studentCompetitionEvents);
                    await _studentCompetitionEventsRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                    return methodResult;
                });

                methodResult.Result = new CreateStudentsToEventFromFileModel() { NumberOfStudent = studentIds.Count };
                methodResult.StatusCode = StatusCodes.Status200OK;

                await _sendStudentsFromFilePublisher.Publish(new CreateStudentsToEventFromFileModel()
                {
                    StatusCode = StatusCodes.Status200OK,
                    Key = request.Key,
                    Message = Success,
                    NumberOfStudent = studentIds.Count,
                }, cancellationToken);

                return methodResult;
            }
            catch (Exception ex)
            {
                methodResult.AddErrorBadRequest(ex.Message);
                await _sendStudentsFromFilePublisher.Publish(new CreateStudentsToEventFromFileModel()
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Key = request.Key,
                    Message = ErrorTemplate
                }, cancellationToken);
                _logger.LogError(ex, "CreateStudentsToEventFromFileCommandHandler error");
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
