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
    using Fsel.Identity.Application.Commands.UserCmd;
    using Fsel.Identity.Application.Services.InteractionService;
    using Fsel.Identity.Application.Services.OrderService;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.CommandModels.Students;
    using Fsel.Identity.Infrastructure.Repositories;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using MassTransit;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using OfficeOpenXml;
    using OfficeOpenXml.Style;

    public class CreateStudentsToEventFromFileCommand : BaseImportCommandModel, IRequest<MethodResult<Stream>>
    {
        public Guid DistrictId { get; set; }
        public Guid SchoolId { get; set; }
        public string? SchoolName { get; set; }
    }

    public class CreateStudentsToEventFromFileCommandHandler : IRequestHandler<CreateStudentsToEventFromFileCommand, MethodResult<Stream>>
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

        public CreateStudentsToEventFromFileCommandHandler(UserManager<User> userManager, IOrderService orderService, IPlatformRepository platformRepository, IInteractionService interactionService, IMediator mediator, IHumanRepository humanRepository, ICompetitionEventsRepository competitionEventsRepository, IStudentCompetitionEventsRepository studentCompetitionEventsRepository, IStudentRepository studentRepository, IServiceProvider serviceProvider)
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
        }

        public async Task<MethodResult<Stream>> Handle(CreateStudentsToEventFromFileCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<Stream>();

            if (request.FormFile == null)
            {
                methodResult.AddError(nameof(EnumSystemErrorCode.ImportFileRequired));
                return methodResult;
            }

            var emails = new List<string>();
            var phoneNumbers = new List<string>();

            var competitionEvent = await _competitionEventsRepository.Queryable.FirstOrDefaultAsync(p => p.LocationId == request.DistrictId, cancellationToken);
            if (competitionEvent == null)
            {
                methodResult.AddError(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            Action<ExcelWorksheet, Dictionary<string, int?>?, IList<ValidateExcelModel>> errorHandlerAction = (worksheet, columnIndexes, errors) =>
            {
                foreach (var error in errors.GroupBy(x => x.RowIndex).Select(x => x).OrderBy(x => x.Key))
                {
                    var row = error.Key;

                    int lastColumn = worksheet.Dimension.End.Column - 1;

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
                            }
                        }
                    }
                    var messages = Shared.Helpers.StringHelper.JoinWithComma(errorMessages.Distinct().ToList());
                    worksheet.Cells[targetRow, lastColumn + 1].Value = messages;
                }
            };

            var result = request.FormFile.ImportAndValidateExcel(async (CreateStudentToEventFromFileModel x, IList<CreateStudentToEventFromFileModel> models, int rowIndex, IList<ValidateExcelModel> errors) =>
            {
                if (string.IsNullOrEmpty(x.FullName))
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.FullName), Message = ErrorMassageSetting.EmptyFullNameVN });
                }
                if (string.IsNullOrEmpty(x.PhoneNumber))
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.PhoneNumber), Message = ErrorMassageSetting.EmptyPhoneNumberVN });
                }
                else if (!x.PhoneNumber.Trim().IsValidPhoneNumber())
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.PhoneNumber), Message = ErrorMassageSetting.InvalidPhoneNumberVN });
                }
                else if (phoneNumbers.Contains(x.PhoneNumber))
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.PhoneNumber), Message = ErrorMassageSetting.PhoneNumberAlreadyExistInListVN });
                }
                else
                {
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

                if (string.IsNullOrEmpty(x.DateOfBirth))
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.DateOfBirth), Message = ErrorMassageSetting.EmptyBirthDayVN });
                }
                else if (!DateTime.TryParse(x.DateOfBirth, out DateTime dob))
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.DateOfBirth), Message = ErrorMassageSetting.InvalidBirthDayVN });
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
                    var dataByEmail = datas.Values.FirstOrDefault(x => x.Email == user.Email);
                    if (dataByEmail != null)
                    {
                        var index = datas.FirstOrDefault(x => x.Value == dataByEmail).Key;
                        errors.Add(new ValidateExcelModel { RowIndex = index, ColumnName = nameof(dataByEmail.Email), Message = ErrorMassageSetting.EmailAlreadyExistVN });
                    }

                    var dataByPhoneNumber = datas.Values.FirstOrDefault(x => x.PhoneNumber == user.PhoneNumber);
                    if (dataByPhoneNumber != null)
                    {
                        var index = datas.FirstOrDefault(x => x.Value == dataByPhoneNumber).Key;
                        errors.Add(new ValidateExcelModel { RowIndex = index, ColumnName = nameof(dataByPhoneNumber.PhoneNumber), Message = ErrorMassageSetting.PhoneNumberAlreadyExistVN });
                    }
                }

                return await Task.FromResult(errors.Count == 0);
            },
            errorHandlerAction);

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
                methodResult.AddErrorBadRequest("File tải lên không có dữ liệu");
                return methodResult;
            }

            var users = new List<User>();
            var studentIds = new List<Guid>();

            try
            {
                Parallel.ForEach(students, async student =>
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
                        var studentRepository = scope.ServiceProvider.GetRequiredService<IStudentRepository>();
                        Microsoft.AspNetCore.Identity.IdentityResult identityStudentResult;
                        int age = Shared.Helpers.DateTimeHelper.GetYearOld(Convert.ToDateTime(student.DateOfBirth, CultureInfo.CurrentCulture));
                        var user = new User()
                        {
                            UserName = student.Email!.Trim(),
                            Email = student.Email.Trim(),
                            FullName = student.FullName!.Trim(),
                            PhoneNumber = student.PhoneNumber!.Trim(),
                            EmailConfirmed = false,
                            PhoneNumberConfirmed = false,
                            Status = EnumUserStatus.Inactive,
                            Human = new Human()
                            {
                                FullName = student.FullName.Trim(),
                                PhoneNumber = student.PhoneNumber.Trim(),
                                Birthday = Convert.ToDateTime(student.DateOfBirth, CultureInfo.CurrentCulture),
                                Email = student.Email.Trim(),
                                Code = GeneratorCodeAsync(studentRepository, Convert.ToDateTime(student.DateOfBirth, CultureInfo.CurrentCulture), null),
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
                            studentIds.Add(user.Human.Student.Id);
                        }

                        //return result;
                    }
                });

                //var resultUsers = await Task.WhenAll(tasks);

                //foreach (var student in students)
                //{
                //    Microsoft.AspNetCore.Identity.IdentityResult identityStudentResult;
                //    int age = Shared.Helpers.DateTimeHelper.GetYearOld(Convert.ToDateTime(student.DateOfBirth, CultureInfo.CurrentCulture));
                //    var user = new User()
                //    {
                //        UserName = student.Email!.Trim(),
                //        Email = student.Email.Trim(),
                //        FullName = student.FullName!.Trim(),
                //        PhoneNumber = student.PhoneNumber!.Trim(),
                //        EmailConfirmed = false,
                //        PhoneNumberConfirmed = false,
                //        Status = EnumUserStatus.Inactive,
                //        Human = new Human()
                //        {
                //            FullName = student.FullName.Trim(),
                //            PhoneNumber = student.PhoneNumber.Trim(),
                //            Birthday = Convert.ToDateTime(student.DateOfBirth, CultureInfo.CurrentCulture),
                //            Email = student.Email.Trim(),
                //            Code = GeneratorCodeAsync(Convert.ToDateTime(student.DateOfBirth, CultureInfo.CurrentCulture), null),
                //            Student = new Student()
                //            {
                //                CreatedByParent = false,
                //                Occupation = nameof(Student),
                //                School = request.SchoolName,
                //                SchoolClass = student.SchoolClass,
                //                SchoolGrade = student.SchoolGrade,
                //                SchoolId = request.SchoolId,
                //                CourseLevel = age <= 13 ? EnumCourseLevel.A2 : EnumCourseLevel.B1,
                //            }
                //        },
                //        UserPlatforms = new List<UserPlatform>()
                //                            {
                //                                new UserPlatform()
                //                                {
                //                                    PlatformId = platform.Id
                //                                }
                //                            },
                //        UserSettings = new List<UserSetting>()
                //            {
                //                new UserSetting(true)
                //            }
                //    };

                //    var password = Shared.Helpers.StringHelper.GeneratePassword(8);

                //    identityStudentResult = await _userManager.CreateAsync(user, password);
                //    if (identityStudentResult.Succeeded)
                //    {
                //        await _userManager.AddToRoleAsync(user, EnumRole.Student.ToString());
                //        studentIds.Add(user.Human.Student.Id);
                //    }
                //};

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
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            catch (Exception ex)
            {
                methodResult.AddErrorBadRequest(ex.Message);
            }

            return methodResult;
        }

        private string GeneratorCodeAsync(IStudentRepository studentRepository, DateTime birthDay, EnumGender? gender)
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
