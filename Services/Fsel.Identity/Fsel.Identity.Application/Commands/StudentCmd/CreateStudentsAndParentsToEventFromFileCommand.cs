// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.StudentCmd
{
    using System.Collections.Concurrent;
    using System.Drawing;
    using System.Globalization;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Common.Models.Excels;
    using Fsel.Core.Base;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Application.Queues.Publishers;
    using Fsel.Identity.Application.Services.OrderService;
    using Fsel.Identity.Application.Services.OrderService.Model;
    using Fsel.Identity.Application.Services.SystemService;
    using Fsel.Identity.Application.Services.SystemService.QueryModels;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.CommandModels.Students;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using Kros.Extensions;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using OfficeOpenXml;
    using OfficeOpenXml.Style;

    public class CreateStudentsAndParentsToEventFromFileCommand : CreateStudentsToEventFromByteModel, IRequest<MethodResult<CreateStudentsToEventFromFileModel>>
    {
    }

    public class CreateStudentsAndParentsToEventFromFileCommandHandler : IRequestHandler<CreateStudentsAndParentsToEventFromFileCommand, MethodResult<CreateStudentsToEventFromFileModel>>
    {
        private readonly UserManager<User> _userManager;
        private readonly IOrderService _orderService;
        private readonly IPlatformRepository _platformRepository;
        private readonly ICompetitionEventsRepository _competitionEventsRepository;
        private readonly IStudentCompetitionEventsRepository _studentCompetitionEventsRepository;
        private readonly IServiceProvider _serviceProvider;
        private readonly SendStudentsFromFilePublisher _sendStudentsFromFilePublisher;
        private readonly ILogger<CreateStudentsToEventFromFileCommand> _logger;
        private readonly AuthContext _authContext;
        private readonly ISystemService _systemService;
        private readonly IHumanRepository _humanRepository;
        private readonly IStudentRepository _studentRepository;

        private const string ErrorTemplate = "Template bị sai, kiểm tra lại tên cột, bạn cần download template ở nút Tải Template mẫu";
        private const string Success = "Thành công";
        private const string ErrorMessage = "Error Message\n(Thông báo lỗi)";
        private const string FileNull = "File tải lên không có dữ liệu";
        private const string DataError = "Dữ liệu bị trống hoặc sai định dạng";
        private const string UserDoesNotExist = "Tài khoản không tồn tại";
        private const string UserDoesNotExistSchool = "Tài khoản chưa được gắn với trường";
        private const string SchoolDoesNotExist = "Trường học không tồn tại";
        private const string SchoolDoesNotExistInEvent = "Trường học chưa được gắn vào sự kiện";
        private const string ExpiredDate = "Đã hết thời gian tạo tài khoản";

        private const string Parent = "Phụ Huynh";

        private const string DefaultPassword = "Fsel@";
        private const int MinYear = 1900;
        private static readonly Random s_random = new Random();

        public CreateStudentsAndParentsToEventFromFileCommandHandler(UserManager<User> userManager, IOrderService orderService, IPlatformRepository platformRepository, ICompetitionEventsRepository competitionEventsRepository, IStudentCompetitionEventsRepository studentCompetitionEventsRepository, IServiceProvider serviceProvider, SendStudentsFromFilePublisher sendStudentsFromFilePublisher, ILogger<CreateStudentsToEventFromFileCommand> logger, AuthContext authContext, ISystemService systemService, IHumanRepository humanRepository, IStudentRepository studentRepository)
        {
            _userManager = userManager;
            _orderService = orderService;
            _platformRepository = platformRepository;
            _competitionEventsRepository = competitionEventsRepository;
            _studentCompetitionEventsRepository = studentCompetitionEventsRepository;
            _serviceProvider = serviceProvider;
            _sendStudentsFromFilePublisher = sendStudentsFromFilePublisher;
            _logger = logger;
            _authContext = authContext;
            _systemService = systemService;
            _humanRepository = humanRepository;
            _studentRepository = studentRepository;
        }

        public async Task<MethodResult<CreateStudentsToEventFromFileModel>> Handle(CreateStudentsAndParentsToEventFromFileCommand request, CancellationToken cancellationToken)
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
                    await SendNotify(request.Key ?? string.Empty, UserDoesNotExist, StatusCodes.Status400BadRequest, 0, null, cancellationToken);
                    return methodResult;
                }

                var schoolId = currentUser.UserSchools.FirstOrDefault()?.SchoolId;
                if (!schoolId.HasValue)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(schoolId));
                    await SendNotify(request.Key ?? string.Empty, UserDoesNotExistSchool, StatusCodes.Status400BadRequest, 0, null, cancellationToken);
                    return methodResult;
                }

                var schoolResult = await _systemService.GetSchoolsAsync(new GetListSchoolQueryModel()
                {
                    Ids = new List<Guid>() { schoolId.Value }
                });
                var school = schoolResult.Content?.Result?.FirstOrDefault();
                if (school == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(school));
                    await SendNotify(request.Key ?? string.Empty, SchoolDoesNotExist, StatusCodes.Status400BadRequest, 0, null, cancellationToken);
                    return methodResult;
                }

                var competitionEvents = _competitionEventsRepository.Queryable.ToList();
                var competitionEvent = competitionEvents.Where(p => p.SchoolIds != null && p.SchoolIds.Contains(schoolId.Value) && p.Category == request.Category).FirstOrDefault();
                if (competitionEvent == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(competitionEvent));
                    await SendNotify(request.Key ?? string.Empty, SchoolDoesNotExistInEvent, StatusCodes.Status400BadRequest, 0, null, cancellationToken);
                    return methodResult;
                }

                var expiredDate = competitionEvent.EventContent?.PaymentDate;

                var expiredDateImport = competitionEvent.EventContent?.ActionConfigs?.FirstOrDefault(p => p.Action == EnumSchoolEventRuleAction.ImportStudent);

                var currentDate = DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam);
                if (!expiredDate.HasValue || currentDate >= expiredDate.Value || expiredDateImport == null || !expiredDateImport.EndDate.HasValue || expiredDateImport.EndDate.Value < currentDate)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(expiredDate));
                    await SendNotify(request.Key ?? string.Empty, ExpiredDate, StatusCodes.Status400BadRequest, 0, null, cancellationToken);
                    return methodResult;
                }

                Action<ExcelWorksheet, Dictionary<string, int?>?, IList<ValidateExcelModel>> errorHandlerAction = (worksheet, columnIndexes, errors) =>
                {
                    worksheet.Cells[1, 11].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    worksheet.Cells[1, 11].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    worksheet.Cells[1, 11].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    worksheet.Cells[1, 11].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    worksheet.Cells[1, 11].Value = ErrorMessage;
                    worksheet.Cells[1, 11].Style.Font.Bold = true;
                    foreach (var error in errors.GroupBy(x => x.RowIndex).Select(x => x).OrderBy(x => x.Key))
                    {
                        var row = error.Key;

                        int lastColumn = 11;

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

                Action<ExcelWorksheet, Dictionary<string, int?>?, int, int, CreateStudentAndParentToEventFromFileModel> defaultStudentHandlerAction = (worksheet, columnIndexes, row, num, model) =>
                {
                    worksheet.Cells[row, num].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[row, num].Style.Fill.BackgroundColor.SetColor(Color.White);
                    worksheet.Cells[row, num].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    worksheet.Cells[row, num].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    worksheet.Cells[row, num].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    worksheet.Cells[row, num].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    if (num == 10)
                    {
                        worksheet.Cells[row, num + 1].Value = null;
                    }
                };

                var stream = new MemoryStream(request.File);
                IFormFile formFile = new FormFile(stream, 0, request.File.Length, "name", "fileName");

                var cultureInfo = CultureInfo.InvariantCulture;

                var students = new List<CreateStudentAndParentToEventFromFileModel>();

                var studentDatas = new List<CreateStudentAndParentToEventFromFileModel>();
                var parentDatas = new List<CreateStudentAndParentToEventFromFileModel>();

                var result = formFile.ImportAndValidateExcel(async (CreateStudentAndParentToEventFromFileModel x, IList<CreateStudentAndParentToEventFromFileModel> models, int rowIndex, IList<ValidateExcelModel> errors) =>
                {
                    if (!string.IsNullOrEmpty(x.FullName?.Trim()) || !string.IsNullOrEmpty(x.PhoneNumber?.Trim()) || x.DateOfBirth.HasValue || !string.IsNullOrEmpty(x.SchoolGrade?.Trim()) || !string.IsNullOrEmpty(x.SchoolClass?.Trim()))
                    {
                        if (string.IsNullOrEmpty(x.FullName?.Trim()))
                        {
                            errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.FullName), Message = ErrorMassageSetting.EmptyFullNameVN });
                        }
                        else
                        {
                            var fullName = GenerateFullName(x.FullName);
                            if (!Shared.Helpers.StringHelper.ContainsSpecialChars(fullName))
                            {
                                errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.FullName), Message = ErrorMassageSetting.InvalidFullNameVN });
                            }
                        }

                        if (string.IsNullOrEmpty(x.PhoneNumber?.Trim()))
                        {
                            errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.PhoneNumber), Message = ErrorMassageSetting.EmptyPhoneNumberVN });
                        }
                        else if (!Shared.Helpers.StringHelper.IsValidPhoneNumber(x.PhoneNumber?.Trim()))
                        {
                            errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.PhoneNumber), Message = ErrorMassageSetting.InvalidPhoneNumberVN });
                        }

                        if (!string.IsNullOrEmpty(x.Email?.Trim()) && !x.Email.Trim().IsValidEmail())
                        {
                            errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.Email), Message = ErrorMassageSetting.InvalidEmailVN });
                        }
                        if (!x.DateOfBirth.HasValue)
                        {
                            errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.DateOfBirth), Message = ErrorMassageSetting.EmptyBirthDayVN });
                        }
                        else if (x.DateOfBirth.HasValue && x.DateOfBirth.Value.Year < MinYear)
                        {
                            errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.DateOfBirth), Message = ErrorMassageSetting.InvalidBirthDayVN });
                        }
                        else if (x.DateOfBirth.HasValue && x.DateOfBirth.Value.Year > (DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam).Year - 1))
                        {
                            errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.DateOfBirth), Message = ErrorMassageSetting.InvalidBirthDayVN });
                        }
                        if (string.IsNullOrEmpty(x.SchoolGrade?.Trim()))
                        {
                            errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.SchoolGrade), Message = ErrorMassageSetting.EmptyGradeVN });
                        }
                        if (string.IsNullOrEmpty(x.SchoolClass?.Trim()))
                        {
                            errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.SchoolClass), Message = ErrorMassageSetting.EmptyClassVN });
                        }

                        if (!string.IsNullOrEmpty(x.ParentFullName?.Trim()) || !string.IsNullOrEmpty(x.ParentPhoneNumber?.Trim()) || x.ParentDateOfBirth.HasValue || !string.IsNullOrEmpty(x.ParentEmail?.Trim()))
                        {
                            if (string.IsNullOrEmpty(x.ParentFullName?.Trim()))
                            {
                                errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.FullName), Message = ErrorMassageSetting.EmptyFullNameVN });
                            }
                            else
                            {
                                var fullName = GenerateFullName(x.ParentFullName);
                                if (!Shared.Helpers.StringHelper.ContainsSpecialChars(fullName))
                                {
                                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.ParentFullName), Message = ErrorMassageSetting.InvalidFullNameVN });
                                }
                            }

                            if (string.IsNullOrEmpty(x.ParentPhoneNumber?.Trim()))
                            {
                                errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.ParentPhoneNumber), Message = ErrorMassageSetting.EmptyPhoneNumberVN });
                            }
                            else if (!Shared.Helpers.StringHelper.IsValidPhoneNumber(x.ParentPhoneNumber?.Trim()))
                            {
                                errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.ParentPhoneNumber), Message = ErrorMassageSetting.InvalidPhoneNumberVN });
                            }

                            if (!string.IsNullOrEmpty(x.ParentEmail?.Trim()) && !x.ParentEmail.Trim().IsValidEmail())
                            {
                                errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.ParentEmail), Message = ErrorMassageSetting.InvalidEmailVN });
                            }
                            if (!x.ParentDateOfBirth.HasValue)
                            {
                                errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.ParentDateOfBirth), Message = ErrorMassageSetting.EmptyBirthDayVN });
                            }
                            else if (x.ParentDateOfBirth.HasValue && x.ParentDateOfBirth.Value.Year < MinYear)
                            {
                                errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.ParentDateOfBirth), Message = ErrorMassageSetting.InvalidBirthDayVN });
                            }
                            else if (x.ParentDateOfBirth.HasValue && x.ParentDateOfBirth.Value.Year > (DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam).Year - 1))
                            {
                                errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.ParentDateOfBirth), Message = ErrorMassageSetting.InvalidBirthDayVN });
                            }

                            if (parentDatas.Any(p => p.ParentFullName == x.ParentFullName && p.ParentDateOfBirth == x.ParentDateOfBirth && p.ParentPhoneNumber == x.ParentPhoneNumber))
                            {
                                errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.ParentPhoneNumber), Message = ErrorMassageSetting.DataAlreadyExistInListVN });
                                errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.ParentDateOfBirth), Message = null });
                                errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.ParentFullName), Message = null });
                            }
                            else
                            {
                                parentDatas.Add(new CreateStudentAndParentToEventFromFileModel()
                                {
                                    ParentFullName = x.ParentFullName,
                                    ParentDateOfBirth = x.ParentDateOfBirth,
                                    ParentPhoneNumber = x.ParentPhoneNumber
                                });
                            }

                            if (studentDatas.Any(p => p.FullName == x.FullName && p.DateOfBirth == x.DateOfBirth && p.PhoneNumber == x.PhoneNumber))
                            {
                                errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.PhoneNumber), Message = ErrorMassageSetting.DataAlreadyExistInListVN });
                                errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.DateOfBirth), Message = null });
                                errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.FullName), Message = null });
                            }
                            else
                            {
                                studentDatas.Add(new CreateStudentAndParentToEventFromFileModel()
                                {
                                    FullName = x.FullName,
                                    DateOfBirth = x.DateOfBirth,
                                    PhoneNumber = x.PhoneNumber
                                });
                            }
                        }
                    }
                    return await Task.FromResult(errors.Count == 0);
                },
                     async (Dictionary<int, CreateStudentAndParentToEventFromFileModel> datas, IList<ValidateExcelModel> errors) =>
                     {
                         var query = await (from u in _userManager.Users
                                            join h in _humanRepository.Queryable on u.Id equals h.UserId
                                            join s in _studentRepository.Queryable on h.Id equals s.HumanId
                                            where s.SchoolId == schoolId
                                            select new
                                            {
                                                User = u,
                                                Human = h,
                                                Student = s
                                            }).ToListAsync(cancellationToken);

                         datas.ForEach(p =>
                         {
                             var phoneNumber = Shared.Helpers.StringHelper.NormalizeToDomesticFormat(p.Value.PhoneNumber?.Trim());

                             var data = query.FirstOrDefault(x => x.User.PhoneNumber == phoneNumber && x.Human.Birthday.HasValue && p.Value.DateOfBirth.HasValue && x.Human.Birthday.Value.Date == p.Value.DateOfBirth.Value.Date && !string.IsNullOrEmpty(x.User.FullName) && !string.IsNullOrEmpty(p.Value.FullName) && x.User.FullName.ToLower() == p.Value.FullName.ToLower());

                             if (data != null)
                             {
                                 errors.Add(new ValidateExcelModel { RowIndex = p.Key, ColumnName = nameof(CreateStudentAndParentToEventFromFileModel.PhoneNumber), Message = ErrorMassageSetting.DataAlreadyExistVN });
                                 errors.Add(new ValidateExcelModel { RowIndex = p.Key, ColumnName = nameof(CreateStudentAndParentToEventFromFileModel.FullName), Message = null });
                                 errors.Add(new ValidateExcelModel { RowIndex = p.Key, ColumnName = nameof(CreateStudentAndParentToEventFromFileModel.DateOfBirth), Message = null });
                             }

                             if (string.IsNullOrEmpty(p.Value.ParentPhoneNumber))
                             {
                                 var parentPhoneNumber = Shared.Helpers.StringHelper.NormalizeToDomesticFormat(p.Value.PhoneNumber?.Trim());

                                 var parentData = query.FirstOrDefault(x => x.User.PhoneNumber == parentPhoneNumber && x.Human.Birthday.HasValue && p.Value.DateOfBirth.HasValue && x.Human.Birthday.Value.Date == p.Value.DateOfBirth.Value.Date && !string.IsNullOrEmpty(x.User.FullName) && !string.IsNullOrEmpty(p.Value.FullName) && x.User.FullName.ToLower() == p.Value.FullName.ToLower());

                                 if (parentData != null)
                                 {
                                     errors.Add(new ValidateExcelModel { RowIndex = p.Key, ColumnName = nameof(CreateStudentAndParentToEventFromFileModel.ParentPhoneNumber), Message = ErrorMassageSetting.DataAlreadyExistVN });
                                     errors.Add(new ValidateExcelModel { RowIndex = p.Key, ColumnName = nameof(CreateStudentAndParentToEventFromFileModel.ParentFullName), Message = null });
                                     errors.Add(new ValidateExcelModel { RowIndex = p.Key, ColumnName = nameof(CreateStudentAndParentToEventFromFileModel.ParentDateOfBirth), Message = null });
                                 }
                             }
                         });

                         return await Task.FromResult(errors.Count == 0);
                     },
                     defaultStudentHandlerAction,
                     errorHandlerAction,
                     true);

                if (!result.IsValidHeader)
                {
                    await SendNotify(request.Key ?? string.Empty, ErrorTemplate, StatusCodes.Status400BadRequest, 0, null, cancellationToken);

                    return methodResult;
                }

                if (result.Stream != null)
                {
                    var file = ConvertHelper.StreamToByteArray(result.Stream);

                    await SendNotify(request.Key ?? string.Empty, DataError, StatusCodes.Status400BadRequest, 0, file, cancellationToken);

                    return methodResult;
                }

                students = result.Datas.ToList();

                var platform = await _platformRepository.GetPlatformAsync(EnumPlatformCode.LMS, cancellationToken);
                if (platform == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                    return methodResult;
                }

                if (students == null || students.Count == 0)
                {
                    methodResult.AddErrorBadRequest(FileNull);

                    await SendNotify(request.Key ?? string.Empty, FileNull, StatusCodes.Status400BadRequest, 0, null, cancellationToken);

                    return methodResult;
                }

                var studentIds = new ConcurrentBag<Guid>();
                var studentModels = new ConcurrentBag<CreateOrderForStudentsEventCommandModel>();

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
                                var password = DefaultPassword + Shared.Helpers.StringHelper.GenerateLaterPartPassword(4);

                                var studentManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

                                var studentRepository = scope.ServiceProvider.GetRequiredService<IStudentRepository>();

                                Microsoft.AspNetCore.Identity.IdentityResult identityStudentResult;

                                int age = Shared.Helpers.DateTimeHelper.GetYearOld(student.DateOfBirth);
                                var user = new User()
                                {
                                    UserName = GenerateUsername(student.FullName?.Trim() ?? string.Empty, Shared.Helpers.StringHelper.NormalizeToDomesticFormat(student.PhoneNumber.Trim())),
                                    Email = !string.IsNullOrEmpty(student.Email) ? student.Email.ToLower(cultureInfo).Trim() : null,
                                    FullName = student.FullName?.Trim() ?? string.Empty,
                                    PhoneNumber = Shared.Helpers.StringHelper.NormalizeToDomesticFormat(student.PhoneNumber.Trim()),
                                    EmailConfirmed = false,
                                    PhoneNumberConfirmed = false,
                                    Status = EnumUserStatus.Active,
                                    DefaultPassword = password,
                                    Human = new Human()
                                    {
                                        FullName = student.FullName?.Trim(),
                                        PhoneNumber = Shared.Helpers.StringHelper.NormalizeToDomesticFormat(student.PhoneNumber.Trim()),
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
                                            ParentPhoneNumber = !string.IsNullOrEmpty(student.ParentPhoneNumber) ? Shared.Helpers.StringHelper.NormalizeToDomesticFormat(student.ParentPhoneNumber.Trim()) : null,
                                            ParentEmail = !string.IsNullOrEmpty(student.ParentEmail) ? student.ParentEmail.Trim() : null,
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

                                identityStudentResult = await studentManager.CreateAsync(user, password);
                                if (identityStudentResult.Succeeded)
                                {
                                    await studentManager.AddToRoleAsync(user, EnumRole.Student.ToString());

                                    studentIds.Add(user.Human.Student.Id);

                                    studentModels.Add(new CreateOrderForStudentsEventCommandModel()
                                    {
                                        UserId = user.Id,
                                        StudentId = user.Human.Student.Id,
                                        Email = user.Email,
                                        PhoneNumber = user.PhoneNumber,
                                        FullName = user.FullName,
                                        StudentCode = user.Human.Code
                                    });
                                }
                                else
                                {
                                    user.UserName = GenerateUsername(student.FullName ?? string.Empty, Shared.Helpers.StringHelper.NormalizeToDomesticFormat(student.PhoneNumber.Trim()));
                                    identityStudentResult = await studentManager.CreateAsync(user, password);

                                    if (identityStudentResult.Succeeded)
                                    {
                                        await studentManager.AddToRoleAsync(user, EnumRole.Student.ToString());

                                        studentIds.Add(user.Human.Student.Id);

                                        studentModels.Add(new CreateOrderForStudentsEventCommandModel()
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

                                if (!string.IsNullOrEmpty(student.ParentPhoneNumber))
                                {
                                    var parentManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
                                    var parentRepository = scope.ServiceProvider.GetRequiredService<IStudentRepository>();
                                    Microsoft.AspNetCore.Identity.IdentityResult identityParentResult;

                                    age = Shared.Helpers.DateTimeHelper.GetYearOld(student.ParentDateOfBirth);
                                    var parent = new User()
                                    {
                                        UserName = GenerateUsername(student.ParentFullName?.Trim() ?? string.Empty, Shared.Helpers.StringHelper.NormalizeToDomesticFormat(student.ParentPhoneNumber.Trim())),
                                        Email = !string.IsNullOrEmpty(student.ParentEmail) ? student.ParentEmail.ToLower(cultureInfo).Trim() : null,
                                        FullName = student.ParentFullName?.Trim() ?? string.Empty,
                                        PhoneNumber = Shared.Helpers.StringHelper.NormalizeToDomesticFormat(student.ParentPhoneNumber.Trim()),
                                        EmailConfirmed = false,
                                        PhoneNumberConfirmed = false,
                                        Status = EnumUserStatus.Active,
                                        DefaultPassword = password,
                                        Human = new Human()
                                        {
                                            FullName = student.ParentFullName?.Trim(),
                                            PhoneNumber = Shared.Helpers.StringHelper.NormalizeToDomesticFormat(student.ParentPhoneNumber.Trim()),
                                            Birthday = student.ParentDateOfBirth,
                                            Email = !string.IsNullOrEmpty(student.ParentEmail) ? student.ParentEmail.ToLower(cultureInfo).Trim() : null,
                                            Code = GeneratorCodeAsync(parentRepository, student.ParentDateOfBirth ?? DateTime.MinValue, null),
                                            Student = new Student()
                                            {
                                                CreatedByParent = false,
                                                Occupation = nameof(Student),
                                                School = school.Name,
                                                SchoolClass = student.SchoolClass,
                                                SchoolGrade = Parent,
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

                                    identityParentResult = await parentManager.CreateAsync(parent, password);
                                    if (identityParentResult.Succeeded)
                                    {
                                        await parentManager.AddToRoleAsync(parent, EnumRole.Student.ToString());

                                        studentIds.Add(parent.Human.Student.Id);

                                        studentModels.Add(new CreateOrderForStudentsEventCommandModel()
                                        {
                                            UserId = parent.Id,
                                            StudentId = parent.Human.Student.Id,
                                            Email = parent.Email,
                                            PhoneNumber = parent.PhoneNumber,
                                            FullName = parent.FullName,
                                            StudentCode = parent.Human.Code
                                        });
                                    }
                                    else
                                    {
                                        parent.UserName = GenerateUsername(student.ParentFullName ?? string.Empty, Shared.Helpers.StringHelper.NormalizeToDomesticFormat(student.ParentPhoneNumber.Trim()));

                                        identityParentResult = await parentManager.CreateAsync(parent, password);

                                        if (identityParentResult.Succeeded)
                                        {
                                            await parentManager.AddToRoleAsync(parent, EnumRole.Student.ToString());

                                            studentIds.Add(parent.Human.Student.Id);

                                            studentModels.Add(new CreateOrderForStudentsEventCommandModel()
                                            {
                                                UserId = parent.Id,
                                                StudentId = parent.Human.Student.Id,
                                                Email = parent.Email,
                                                PhoneNumber = parent.PhoneNumber,
                                                FullName = parent.FullName,
                                                StudentCode = parent.Human.Code
                                            });
                                        }
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

                if (studentCompetitionEvents.Count == 0)
                {
                    methodResult.AddErrorBadRequest(FileNull);

                    await SendNotify(request.Key ?? string.Empty, FileNull, StatusCodes.Status400BadRequest, 0, null, cancellationToken);

                    return methodResult;
                }

                await _studentCompetitionEventsRepository.ExecuteTransactionAsync(async () =>
                {
                    await _studentCompetitionEventsRepository.AddList(studentCompetitionEvents);
                    await _studentCompetitionEventsRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                    return methodResult;
                });

                var createOrdersResult = await _orderService.CreateOrderForStudentsEvent(new CreateOrderForStudentsEventCommandModels()
                {
                    Students = studentModels.ToList(),
                    ExpiredDate = expiredDate.Value
                });

                methodResult.Result = new CreateStudentsToEventFromFileModel() { NumberOfStudent = studentIds.Count };
                methodResult.StatusCode = StatusCodes.Status200OK;

                await SendNotify(request.Key ?? string.Empty, Success, StatusCodes.Status200OK, studentIds.Count, null, cancellationToken);

                return methodResult;
            }
            catch (Exception ex)
            {
                methodResult.AddErrorBadRequest(ex.Message);

                await SendNotify(request.Key ?? string.Empty, ErrorTemplate, StatusCodes.Status400BadRequest, 0, null, cancellationToken);

                _logger.LogError(ex, "CreateStudentsToEventFromFileCommandHandler error");
            }

            return methodResult;
        }

        public static string GenerateUsername(string fullName, string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(phoneNumber))
            {
                throw new ArgumentException("Full name and phone number cannot be empty.");
            }

            string letters = "ABCDEFGHJKMNPQRSTUVWXYZ";
            string digits = "123456789";

            CultureInfo cultureInfo = CultureInfo.InvariantCulture;

            var newFullName = RemoveDiacritics(fullName);

            string initials = string.Join("", newFullName.Split(' ').Where(s => s.Length > 0).Select(s => s[0])).ToUpper(cultureInfo);

            char randomLetter1 = letters[s_random.Next(letters.Length)];
            char randomLetter2 = letters[s_random.Next(letters.Length)];
            char randomDigit = digits[s_random.Next(digits.Length)];

            return $"{initials}_{phoneNumber}_{randomLetter1}{randomLetter2}{randomDigit}";
        }

        public static string GenerateFullName(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(fullName))
            {
                throw new ArgumentException("Full name and phone number cannot be empty.");
            }

            CultureInfo cultureInfo = CultureInfo.InvariantCulture;

            var newFullName = RemoveDiacritics(fullName);

            string initials = string.Join("", newFullName.Split(' ').Where(s => s.Length > 0).Select(s => s[0])).ToUpper(cultureInfo);

            return initials;
        }

        public static string RemoveDiacritics(string text)
        {
            if (text.IsNullOrEmpty())
            {
                return string.Empty;
            }
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder(capacity: normalizedString.Length);

            for (int i = 0; i < normalizedString.Length; i++)
            {
                char c = normalizedString[i];
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            StringComparison stringComparison = StringComparison.InvariantCultureIgnoreCase;

            return stringBuilder
                .ToString().Replace("Đ", "D", stringComparison).Replace("đ", "d", stringComparison)
                .Normalize(NormalizationForm.FormC);
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

        private async Task SendNotify(string key, string message, int statusCodes, int numberOfStudent, byte[]? file, CancellationToken cancellationToken)
        {
            await _sendStudentsFromFilePublisher.Publish(new CreateStudentsToEventFromFileModel()
            {
                StatusCode = statusCodes,
                Key = key,
                Message = message,
                NumberOfStudent = numberOfStudent,
                File = file
            }, cancellationToken);
        }
    }
}
