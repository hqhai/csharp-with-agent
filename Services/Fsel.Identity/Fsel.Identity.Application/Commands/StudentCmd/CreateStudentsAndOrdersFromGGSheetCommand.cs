// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.StudentCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Identity.Application.Services.GoogleSheetServices;
    using Fsel.Identity.Domain.Models.CommandModels.Students;
    using Fsel.Identity.Infrastructure.ValueSettings;
    using Fsel.Shared.Constants;
    using MediatR;

    public class CreateStudentsAndOrdersFromGGSheetCommand : IRequest<MethodResult<CreateStudentsAndOrdersByAdminCommandResultModel>>
    {
        public string? Sheet { get; set; }
    }

    public class CreateStudentsAndOrdersFromGGSheetCommandHandler : IRequestHandler<CreateStudentsAndOrdersFromGGSheetCommand, MethodResult<CreateStudentsAndOrdersByAdminCommandResultModel>>
    {
        private readonly IGoogleSheetService _googleSheetService;
        private readonly AppSetting _appSetting;
        private readonly IMediator _mediator;

        public CreateStudentsAndOrdersFromGGSheetCommandHandler(AppSetting appSetting, IMediator mediator)
        {
            _googleSheetService = new GoogleSheetService(ResourceSettings.StudentCredentialsFilePath);
            _appSetting = appSetting;
            _mediator = mediator;
        }

        public async Task<MethodResult<CreateStudentsAndOrdersByAdminCommandResultModel>> Handle(CreateStudentsAndOrdersFromGGSheetCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<CreateStudentsAndOrdersByAdminCommandResultModel>();

            var googleSheetId = _appSetting.GoogleSheetConfig?.CreateUsersAndOrders;

            if (string.IsNullOrEmpty(googleSheetId) || string.IsNullOrEmpty(request.Sheet))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var dataVN = _googleSheetService.ReadDataFromSheet(googleSheetId, request.Sheet);
            dataVN.RemoveAt(0);

            var models = new CreateStudentsAndOrdersByAdminCommandModel();
            models.Users = new List<CreateStudentAndOrderByAdminCommandModel>();
            foreach (var dataItem in dataVN)
            {
                var model = new CreateStudentAndOrderByAdminCommandModel();
                var fullName = dataItem[0].ToString();
                var email = dataItem[1].ToString();
                var dob = dataItem[3].ToString();
                var month = dataItem[5].ToString();
                var expireDateStr = dataItem.Count == 7 ? dataItem[6].ToString() : null;

                if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(month) && string.IsNullOrEmpty(expireDateStr))
                {
                    methodResult.AddErrorBadRequest("Thiếu dữ liệu");
                    return methodResult;
                }

                if (!string.IsNullOrEmpty(month) && !string.IsNullOrEmpty(expireDateStr))
                {
                    methodResult.AddErrorBadRequest("Chỉ nhập Package hoặc ExpiredDate");
                    return methodResult;
                }

                var dateOfBirth = DateTime.UtcNow;
                var expireDate = DateTime.UtcNow;

                if (!string.IsNullOrEmpty(dob))
                {
                    if (!DateTime.TryParse(dob, out dateOfBirth))
                    {
                        methodResult.AddErrorBadRequest("DOB không đúng định dạng");
                        return methodResult;
                    }
                }

                if (!string.IsNullOrEmpty(expireDateStr))
                {
                    if (!DateTime.TryParse(expireDateStr, out expireDate))
                    {
                        methodResult.AddErrorBadRequest("ExpireDate không đúng định dạng");
                        return methodResult;
                    }
                }

                var isMonthNumber = int.TryParse(month, out int monthNumber);

                model.FullName = fullName;
                model.Email = email;
                model.PhoneNumber = dataItem[2].ToString();
                model.DateOfBirth = dateOfBirth;
                model.School = dataItem[4].ToString();
                model.MonthNumber = isMonthNumber ? monthNumber : null;
                model.ExpireDate = expireDate;
                model.IsRevenue = true;
                model.IsSendMail = true;

                models.Users.Add(model);
            }

            var result = await _mediator.Send(new CreateStudentsAndOrdersByAdminCommand
            {
                Users = models.Users,
            }, cancellationToken);

            if (!result.IsOK)
            {
                methodResult.AddError(result.ErrorMessages);
            }

            methodResult.Result = result.Result;
            return methodResult;
        }
    }
}
