// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.AdminCmd
{
    using System.Globalization;
    using System.Threading;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Common.Models.Excels;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Models.CommandModels.Admins;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using Kros.Extensions;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Net.Http.Headers;

    public class CreateUserStudentsToAdminCommand : BaseImportCommandModel, IRequest<MethodResult<Stream>>
    {
        public bool IsTrialRegistration { get; set; }
        public Guid? PackageId { get; set; }
        public Guid CourseId { get; set; }
        public DateTime? ExpireDate { get; set; }
        public EnumPaymentRevenueType PaymentRevenueType { get; set; }
    }

    public class CreateUserStudentsToAdminCommandHandler : IRequestHandler<CreateUserStudentsToAdminCommand, MethodResult<Stream>>
    {
        private readonly IMediator _mediator;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHostEnvironment _environment;
        private readonly UserManager<User> _userManager;

        public CreateUserStudentsToAdminCommandHandler(IMediator mediator, IHttpContextAccessor httpContextAccessor, IHostEnvironment environment, UserManager<User> userManager)
        {
            _mediator = mediator;
            _httpContextAccessor = httpContextAccessor;
            _environment = environment;
            _userManager = userManager;
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
                if (string.IsNullOrEmpty(x.PhoneNumber) || !x.PhoneNumber.IsValidPhoneNumber())
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.PhoneNumber), Message = "PhoneNumber is null or malformed" });
                }
                if (string.IsNullOrEmpty(x.Email) || !x.Email.IsValidEmail())
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.Email), Message = "Email is null or malformed" });
                }
                else if (_userManager.Users.Any(p => p.Email == x.Email || p.UserName == x.Email))
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.Email), Message = "Email Already exist" });
                }
                if (string.IsNullOrEmpty(x.DateOfBirth) || (!string.IsNullOrEmpty(x.DateOfBirth) && !DateTime.TryParse(x.DateOfBirth, new CultureInfo("vi-VN"), DateTimeStyles.None, out DateTime dob)))
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.DateOfBirth), Message = "Date of birth is null or malformed" });
                }
                if (string.IsNullOrEmpty(x.School))
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.School), Message = "School not exist" });
                }
                return await Task.FromResult(errors.Count == 0);
            });

            var duplicateEmails = result.Datas.GroupBy(user => user.Email).Where(group => group.Count() > 1).Select(group => group.Key);

            if (duplicateEmails.Any())
            {
                methodResult.AddErrorBadRequest("Duplicate Emails");
                return methodResult;
            }

            if (result.Stream != null)
            {
                methodResult.Result = result.Stream;
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }

            var tokenAdmin = _httpContextAccessor.HttpContext?.Request.Headers[HeaderNames.Authorization].ToString();
            var listUser = new List<UserModel>();
            foreach (var item in result.Datas)
            {
                var userResult = await _mediator.Send(new CreateUserStudentToAdminCommand
                {
                    CourseId = request.CourseId,
                    Email = item.Email,
                    IsTrialRegistration = request.IsTrialRegistration,
                    ExpireDate = request.ExpireDate,
                    PaymentRevenueType = request.PaymentRevenueType,
                    PhoneNumber = item.PhoneNumber,
                    School = item.School,
                    DateOfBirth = item.DateOfBirth,
                    FullName = item.FullName,
                    Gender = item.Gender,
                    PackageId = request.PackageId,
                    SchoolId = item.SchoolId
                }, cancellationToken);
                if (!userResult.IsOK)
                {
                    methodResult.AddErrorBadRequest(userResult.ErrorMessages);
                    return methodResult;
                }
                if (userResult.Result != null)
                {
                    listUser.Add(userResult.Result);
                }
                if (_httpContextAccessor.HttpContext != null)
                {
                    _httpContextAccessor.HttpContext.Request.Headers[HeaderNames.Authorization] = tokenAdmin;
                }
            }
            methodResult.Result = listUser.ExportExcel();
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
