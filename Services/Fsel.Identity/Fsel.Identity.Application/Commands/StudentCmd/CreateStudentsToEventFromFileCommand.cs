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
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Application.Queues.Publishers;
    using Fsel.Identity.Application.Services.InteractionService;
    using Fsel.Identity.Application.Services.OrderService;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums.ErrorCodes;
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

        public CreateStudentsToEventFromFileCommandHandler(UserManager<User> userManager, IOrderService orderService, IPlatformRepository platformRepository, IInteractionService interactionService, IMediator mediator, IHumanRepository humanRepository, ICompetitionEventsRepository competitionEventsRepository, IStudentCompetitionEventsRepository studentCompetitionEventsRepository, IStudentRepository studentRepository, IServiceProvider serviceProvider, SendStudentsFromFilePublisher sendStudentsFromFilePublisher, ISchoolImportHistoryRepository schoolImportHistoryRepository)
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
        }

        public async Task<MethodResult<CreateStudentsToEventFromFileModel>> Handle(CreateStudentsToEventFromFileCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<CreateStudentsToEventFromFileModel>();

            if (request.File == null)
            {
                methodResult.AddError(nameof(EnumSystemErrorCode.ImportFileRequired));
                return methodResult;
            }

            var emails = new List<string>();
            var phoneNumbers = new List<string>();

            var competitionEvent = await _competitionEventsRepository.Queryable.FirstOrDefaultAsync(p => p.Id == request.DistrictId, cancellationToken);
            if (competitionEvent == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(competitionEvent));
                return methodResult;
            }

            // check trường đã thực hiện import chưa
            var checkImportSchool = await _schoolImportHistoryRepository.Queryable.AnyAsync(x => x.SchoolId == request.SchoolId && x.CompetitionEventId == competitionEvent.ParentEventId, cancellationToken);
            if (checkImportSchool)
            {
                methodResult.AddErrorBadRequest(nameof(EnumUserSchoolErrorCode.SchoolAlreadyImported), nameof(request.SchoolId), request.SchoolId);
                return methodResult;
            }

            Action<ExcelWorksheet, Dictionary<string, int?>?, IList<ValidateExcelModel>> errorHandlerAction = (worksheet, columnIndexes, errors) =>
            {
                worksheet.Cells[1, 7].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[1, 7].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[1, 7].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[1, 7].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[1, 7].Value = "Error Message";
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

            var result = formFile.ImportAndValidateExcel(async (CreateStudentToEventFromFileModel x, IList<CreateStudentToEventFromFileModel> models, int rowIndex, IList<ValidateExcelModel> errors) =>
            {
                if (string.IsNullOrEmpty(x.FullName))
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.FullName), Message = ErrorMassageSetting.EmptyFullNameVN });
                }
                if (string.IsNullOrEmpty(x.PhoneNumber))
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.PhoneNumber), Message = ErrorMassageSetting.EmptyPhoneNumberVN });
                }
                else if (!Shared.Helpers.StringHelper.IsValidPhoneNumber(x.PhoneNumber))
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.PhoneNumber), Message = ErrorMassageSetting.InvalidPhoneNumberVN });
                }
                else if (phoneNumbers.Contains(Shared.Helpers.StringHelper.NormalizeToDomesticFormat(x.PhoneNumber)))
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.PhoneNumber), Message = ErrorMassageSetting.PhoneNumberAlreadyExistInListVN });
                }
                else
                {
                    x.PhoneNumber = Shared.Helpers.StringHelper.NormalizeToDomesticFormat(x.PhoneNumber);
                    phoneNumbers.Add(x.PhoneNumber);
                }

                if (string.IsNullOrEmpty(x.Email))
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.Email), Message = ErrorMassageSetting.EmptyEmailVN });
                }
                else if (!x.Email.Trim().IsValidEmail())
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.Email), Message = ErrorMassageSetting.InvalidEmailVN });
                }
                else if (emails.Contains(x.Email))
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.Email), Message = ErrorMassageSetting.EmailAlreadyExistInListVN });
                }
                else
                {
                    emails.Add(x.Email);
                }
                int yearOfBirth = 0;
                if (string.IsNullOrEmpty(x.YearOfBirth))
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.YearOfBirth), Message = ErrorMassageSetting.EmptyBirthDayVN });
                }
                else if (!int.TryParse(x.YearOfBirth, out yearOfBirth))
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.YearOfBirth), Message = ErrorMassageSetting.InvalidBirthDayVN });
                }
                else if (yearOfBirth <= 1920 || yearOfBirth > DateTime.UtcNow.Year)
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.YearOfBirth), Message = ErrorMassageSetting.InvalidBirthDayVN });
                }
                if (string.IsNullOrEmpty(x.SchoolGrade))
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.SchoolGrade), Message = ErrorMassageSetting.EmptyGradeVN });
                }
                if (string.IsNullOrEmpty(x.SchoolClass))
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.SchoolClass), Message = ErrorMassageSetting.EmptyClassVN });
                }

                return await Task.FromResult(errors.Count == 0);
            },
            async (Dictionary<int, CreateStudentToEventFromFileModel> datas, IList<ValidateExcelModel> errors) =>
            {
                var usersExist = await _userManager.Users.Where(x => datas.Values.Select(n => n.Email).Contains(x.Email) || datas.Values.Select(n => n.PhoneNumber).Contains(x.PhoneNumber)).ToArrayAsync(cancellationToken);

                foreach (var user in usersExist)
                {
                    var dataByEmail = datas.Values.Where(x => !x.Email.IsNullOrEmpty()).FirstOrDefault(x => x.Email == user.Email);
                    if (dataByEmail != null)
                    {
                        var index = datas.FirstOrDefault(x => x.Value == dataByEmail).Key;
                        errors.Add(new ValidateExcelModel { RowIndex = index, ColumnName = nameof(dataByEmail.Email), Message = ErrorMassageSetting.EmailAlreadyExistVN });
                    }

                    var dataByPhoneNumber = datas.Values.Where(x => !x.PhoneNumber.IsNullOrEmpty()).FirstOrDefault(x => x.PhoneNumber == user.PhoneNumber);
                    if (dataByPhoneNumber != null)
                    {
                        var index = datas.FirstOrDefault(x => x.Value == dataByPhoneNumber).Key;
                        errors.Add(new ValidateExcelModel { RowIndex = index, ColumnName = nameof(dataByPhoneNumber.PhoneNumber), Message = ErrorMassageSetting.PhoneNumberAlreadyExistVN });
                    }
                }

                return await Task.FromResult(errors.Count == 0);
            }, defaultHandlerAction,
            errorHandlerAction);

            if (result.Stream != null)
            {
                var file = ConvertHelper.StreamToByteArray(result.Stream);
                //methodResult.Result = new CreateStudentsToEventFromFileModel() { Stream = result.Stream };
                await _sendStudentsFromFilePublisher.Publish(new CreateStudentsToEventFromFileModel()
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    File = file,
                    Key = request.Key,
                    Message = "Dữ liệu lỗi"
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
                methodResult.AddErrorBadRequest("File tải lên không có dữ liệu");
                await _sendStudentsFromFilePublisher.Publish(new CreateStudentsToEventFromFileModel()
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Key = request.Key,
                    Message = "File tải lên không có dữ liệu"
                }, cancellationToken);
                return methodResult;
            }

            var users = new List<User>();
            var studentIds = new List<Guid>();

            var maxDegreeOfParallelism = 100;
            using var semaphore = new SemaphoreSlim(maxDegreeOfParallelism);

            try
            {
                await Parallel.ForEachAsync(students, async (student, cancellationToken) =>
                {
                    // Chờ để có slot trống trong Semaphore
                    await semaphore.WaitAsync(cancellationToken);

                    try
                    {
                        using (var scope = _serviceProvider.CreateScope())
                        {
                            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
                            var studentRepository = scope.ServiceProvider.GetRequiredService<IStudentRepository>();
                            Microsoft.AspNetCore.Identity.IdentityResult identityStudentResult;
                            int yearOfBirth = DateTime.UtcNow.Year;
                            var isYearOfBirth = int.TryParse(student.YearOfBirth, out yearOfBirth);
                            var birthDay = new DateTime(yearOfBirth, 1, 1);
                            int age = Shared.Helpers.DateTimeHelper.GetYearOld(Convert.ToDateTime(birthDay, CultureInfo.CurrentCulture));
                            var user = new User()
                            {
                                UserName = Shared.Helpers.StringHelper.NormalizeToDomesticFormat(student.PhoneNumber),
                                Email = student.Email!.Trim(),
                                FullName = student.FullName!.Trim(),
                                PhoneNumber = Shared.Helpers.StringHelper.NormalizeToDomesticFormat(student.PhoneNumber),
                                EmailConfirmed = false,
                                PhoneNumberConfirmed = false,
                                Status = EnumUserStatus.Inactive,
                                Human = new Human()
                                {
                                    FullName = student.FullName.Trim(),
                                    PhoneNumber = Shared.Helpers.StringHelper.NormalizeToDomesticFormat(student.PhoneNumber),
                                    Birthday = birthDay,
                                    Email = student.Email.Trim(),
                                    Code = GeneratorCodeAsync(studentRepository, Convert.ToDateTime(birthDay, CultureInfo.CurrentCulture), null),
                                    Student = new Student()
                                    {
                                        CreatedByParent = false,
                                        Occupation = nameof(Student),
                                        School = request.SchoolName,
                                        SchoolClass = student.SchoolClass,
                                        SchoolGrade = student.SchoolGrade,
                                        SchoolId = request.SchoolId,
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
                    finally
                    {
                        // Giải phóng Semaphore để cho phép task khác tiếp tục
                        semaphore.Release();
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
                    Message = "Thành công",
                    NumberOfStudent = studentIds.Count,
                }, cancellationToken);

                if (competitionEvent.ParentEventId.HasValue)
                {
                    await CreateSchoolImportHistory(request.SchoolId, competitionEvent.ParentEventId.Value);
                }

                return methodResult;
            }
            catch (Exception ex)
            {
                methodResult.AddErrorBadRequest(ex.Message);
            }

            return methodResult;
        }

        public static bool IsValidDateTime(string input, string format = "dd-MM-yyyy")
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            DateTime parsedDate;
            var culture = CultureInfo.InvariantCulture;

            return DateTime.TryParseExact(input, format, culture, DateTimeStyles.None, out parsedDate);
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

        private async Task CreateSchoolImportHistory(Guid schoolId, Guid competitionEventId)
        {
            var schoolImportHistory = new SchoolImportHistory
            {
                SchoolId = schoolId,
                CompetitionEventId = competitionEventId
            };

            _schoolImportHistoryRepository.Add(schoolImportHistory);
            await _schoolImportHistoryRepository.UnitOfWork.SaveChangesAsync();
        }
    }
}
