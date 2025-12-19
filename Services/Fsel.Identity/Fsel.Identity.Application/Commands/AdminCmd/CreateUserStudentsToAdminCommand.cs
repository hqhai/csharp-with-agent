// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.AdminCmd
{
    using System.Threading;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Common.Models.Excels;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Identity.Application.Services.LmsCourseService;
    using Fsel.Identity.Application.Services.LmsCourseService.Model;
    using Fsel.Identity.Application.Services.LmsCourseService.QueryModels;
    using Fsel.Identity.Application.Services.OrderService;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Models.CommandModels.Admins;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Net.Http.Headers;

    public class CreateUserStudentsToAdminCommand : BaseImportCommandModel, IRequest<MethodResult<Stream>>
    {
        public bool IsTrialRegistration { get; set; }
        public Guid? PackageId { get; set; }
        public DateTime? ExpireDate { get; set; }
        public EnumPaymentRevenueType RevenueType { get; set; }
    }

    public class CreateUserStudentsToAdminCommandHandler : IRequestHandler<CreateUserStudentsToAdminCommand, MethodResult<Stream>>
    {
        private readonly IMediator _mediator;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHostEnvironment _environment;
        private readonly Microsoft.AspNetCore.Identity.UserManager<User> _userManager;
        private readonly ILmsCourseService _lmsCourseService;
        private readonly IOrderService _orderService;

        public CreateUserStudentsToAdminCommandHandler(IMediator mediator,
            IHttpContextAccessor httpContextAccessor,
            IHostEnvironment environment,
            Microsoft.AspNetCore.Identity.UserManager<User> userManager,
            ILmsCourseService lmsCourseService,
            IOrderService orderService)
        {
            _mediator = mediator;
            _httpContextAccessor = httpContextAccessor;
            _environment = environment;
            _userManager = userManager;
            _lmsCourseService = lmsCourseService;
            _orderService = orderService;
        }

        public async Task<MethodResult<Stream>> Handle(CreateUserStudentsToAdminCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<Stream>();
            //if (_environment.IsProduction())
            //{
            //    methodResult.AddError(StatusCodes.Status401Unauthorized, "Not Have Access Production");
            //    return methodResult;
            //}

            if (request.FormFile == null)
            {
                methodResult.AddError(nameof(EnumSystemErrorCode.ImportFileRequired));
                return methodResult;
            }
            var result = request.FormFile.ImportAndValidateExcel(async (CreateUserStudentToAdminCommandModel x, IList<CreateUserStudentToAdminCommandModel> models, int rowIndex, IList<ValidateExcelModel> errors) =>
            {
                if (string.IsNullOrEmpty(x.FullName))
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.FullName), Message = "Full Name is null" });
                }
                if (!string.IsNullOrEmpty(x.PhoneNumber) && !x.PhoneNumber.IsValidPhoneNumber())
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.PhoneNumber), Message = "PhoneNumber is null or malformed" });
                }
                if (string.IsNullOrEmpty(x.Email) || !x.Email.IsValidEmail())
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.Email), Message = "Email is null or malformed" });
                }
                if (string.IsNullOrEmpty(x.DateOfBirth) || (!string.IsNullOrEmpty(x.DateOfBirth) && !DateTime.TryParse(x.DateOfBirth, out DateTime dob)))
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.DateOfBirth), Message = "Date of birth is null or malformed" });
                }
                if (string.IsNullOrEmpty(x.CourseLevel) && string.IsNullOrEmpty(x.CourseId))
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.CourseLevel), Message = "Course is null or malformed" });
                }
                if (!string.IsNullOrEmpty(x.CourseLevel) && !Enum.TryParse(x.CourseLevel, out EnumCourseLevel _))
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.CourseLevel), Message = $"CourseLevel is {EnumSystemErrorCode.InValidFormat}" });
                }
                if (!string.IsNullOrEmpty(x.CourseId) && !Guid.TryParse(x.CourseId, out _))
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.CourseId), Message = $"CourseId is {EnumSystemErrorCode.InValidFormat}" });
                }
                if (!string.IsNullOrEmpty(x.IsSendMail) && !bool.TryParse(x.IsSendMail, out _))
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.IsSendMail), Message = $"IsSendMail is {EnumSystemErrorCode.InValidFormat}" });
                }
                if (!string.IsNullOrEmpty(x.SchoolId) && !Guid.TryParse(x.SchoolId, out _))
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.SchoolId), Message = $"SchoolId is {EnumSystemErrorCode.InValidFormat}" });
                }
                if (!string.IsNullOrEmpty(x.Password))
                {
                    foreach (IPasswordValidator<User> passwordValidator in _userManager.PasswordValidators)
                    {
                        var resultData = await passwordValidator.ValidateAsync(_userManager, new User(), x.Password);
                        if (!resultData.Succeeded)
                        {
                            errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.Password), Message = $"Password is {EnumSystemErrorCode.InValidFormat}" });
                        }
                    }
                }
                return await Task.FromResult(errors.Count == 0);
            });
            if (result.Stream != null)
            {
                methodResult.Result = result.Stream;
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }
            if (result.Datas.Any(x => !string.IsNullOrEmpty(x.CourseLevel)))
            {
                var courseLevels = result.Datas.Where(x => !string.IsNullOrEmpty(x.CourseLevel)).Select(x => x.CourseLevel).Distinct().ToList();
                var courseResults = await _lmsCourseService.GetCoursesByLevelsAsync(new GetCoursesByCourseLevelsQueryModel { CourseLevels = string.Join(",", courseLevels) });
                if (!courseResults.IsSuccessStatusCode)
                {
                    methodResult.AddError(courseResults.Error);
                    return methodResult;
                }
                var courses = courseResults?.Content?.Result ?? new List<CourseModel>();
                result = request.FormFile.ImportAndValidateExcel(async (CreateUserStudentToAdminCommandModel x, IList<CreateUserStudentToAdminCommandModel> models, int rowIndex, IList<ValidateExcelModel> errors) =>
                {
                    if (!string.IsNullOrEmpty(x.CourseLevel) && !Enum.TryParse(x.CourseLevel, out EnumCourseLevel courseLevel) && courses.Any(y => y.CourseLevel != courseLevel))
                    {
                        errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.CourseLevel), Message = $"CourseLevel is not Active" });
                    }
                    return await Task.FromResult(errors.Count == 0);
                });
            }

            if (result.Datas.Any(x => !string.IsNullOrEmpty(x.CourseId)))
            {
                var courseIds = result.Datas.Where(x => !string.IsNullOrEmpty(x.CourseId)).Select(x =>
                {
                    if (Guid.TryParse(x.CourseId, out Guid courseId))
                    {
                        return courseId;
                    }
                    return new Guid(x.CourseId ?? string.Empty);
                }).Distinct().ToList();
                var courseResults = await _lmsCourseService.GetCoursesByIdsAsync(courseIds);
                if (!courseResults.IsSuccessStatusCode)
                {
                    methodResult.AddError(courseResults.Error);
                    return methodResult;
                }
                var courses = courseResults?.Content?.Result ?? new List<CourseModel>();
                result = request.FormFile.ImportAndValidateExcel(async (CreateUserStudentToAdminCommandModel x, IList<CreateUserStudentToAdminCommandModel> models, int rowIndex, IList<ValidateExcelModel> errors) =>
                {
                    if (!string.IsNullOrEmpty(x.CourseId) && !Guid.TryParse(x.CourseId, out Guid courseId) && courses.Any(y => y.Id != courseId && y.Status != EnumCourseStatus.New && y.Status != EnumCourseStatus.Clone))
                    {
                        errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.CourseId), Message = $"CourseId is not Active" });
                    }
                    return await Task.FromResult(errors.Count == 0);
                });
            }
            if (result.Stream != null)
            {
                methodResult.Result = result.Stream;
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }
            if (request.PackageId.HasValue)
            {
                var packageResults = await _orderService.GetPackages();
                if (!packageResults.IsSuccessStatusCode)
                {
                    methodResult.AddError(packageResults.Error);
                    return methodResult;
                }
                var package = packageResults.Content?.Result?.FirstOrDefault(x => x.Id == request.PackageId);
                if (package == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(package), request.PackageId);
                    return methodResult;
                }
            }

            var tokenAdmin = _httpContextAccessor.HttpContext?.Request.Headers[HeaderNames.Authorization].ToString();
            var listUser = new List<CreateUserStudentToAdminCommandModel>();
            foreach (var item in result.Datas)
            {
                if (!string.IsNullOrEmpty(item.Email))
                {
                    var user = await _userManager.FindByEmailAsync(item.Email);
                    if (user != null)
                    {
                        listUser.Add(item);
                        continue;
                    }
                }
                var userResult = await _mediator.Send(new CreateUserStudentToAdminCommand
                {
                    CourseId = item.CourseId,
                    Email = item.Email,
                    IsTrialRegistration = request.IsTrialRegistration,
                    ExpireDate = request.ExpireDate,
                    RevenueType = request.RevenueType,
                    PhoneNumber = item.PhoneNumber,
                    School = item.School,
                    DateOfBirth = item.DateOfBirth,
                    FullName = item.FullName,
                    Gender = item.Gender,
                    PackageId = request.PackageId,
                    SchoolId = item.SchoolId,
                    CourseLevel = item.CourseLevel,
                    Password = item.Password,
                    IsSendMail = item.IsSendMail
                }, cancellationToken);
                if (!userResult.IsOK)
                {
                    listUser.Add(item);
                }
                if (_httpContextAccessor.HttpContext != null)
                {
                    _httpContextAccessor.HttpContext.SetHeader(HeaderNames.Authorization, tokenAdmin);
                }
            }
            methodResult.Result = listUser.ExportExcel();
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
