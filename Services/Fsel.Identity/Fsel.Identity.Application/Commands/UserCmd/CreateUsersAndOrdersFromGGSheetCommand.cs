// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.UserCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Identity.Application.Services.GoogleSheetServices;
    using Fsel.Identity.Domain.Models.CommandModels.Users;
    using Fsel.Identity.Infrastructure.ValueSettings;
    using Fsel.Shared.Constants;
    using MediatR;

    public class CreateUsersAndOrdersFromGGSheetCommand : IRequest<MethodResult<CreateUsersAndOrdersByAdminCommandResultModel>>
    {
        public string? Sheet { get; set; }
    }

    public class CreateUsersAndOrdersFromGGSheetCommandHandler : IRequestHandler<CreateUsersAndOrdersFromGGSheetCommand, MethodResult<CreateUsersAndOrdersByAdminCommandResultModel>>
    {
        private readonly IGoogleSheetService _googleSheetService;
        private readonly AppSetting _appSetting;
        private readonly IMediator _mediator;

        public CreateUsersAndOrdersFromGGSheetCommandHandler(AppSetting appSetting, IMediator mediator)
        {
            _googleSheetService = new GoogleSheetService(ResourceSettings.StudentCredentialsFilePath);
            _appSetting = appSetting;
            _mediator = mediator;
        }

        public async Task<MethodResult<CreateUsersAndOrdersByAdminCommandResultModel>> Handle(CreateUsersAndOrdersFromGGSheetCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<CreateUsersAndOrdersByAdminCommandResultModel>();

            var googleSheetId = _appSetting.GoogleSheetConfig?.CreateUsersAndOrders;

            if (string.IsNullOrEmpty(googleSheetId) || string.IsNullOrEmpty(request.Sheet))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            IList<IList<object>> dataVN = _googleSheetService.ReadDataFromSheet(googleSheetId, request.Sheet);
            dataVN.RemoveAt(0);

            var models = new CreateUsersAndOrdersByAdminCommandModel();
            models.Users = new List<CreateUserAndOrderByAdminCommandModel>();
            foreach (var dataItem in dataVN)
            {
                var model = new CreateUserAndOrderByAdminCommandModel();
                var fullName = dataItem[0].ToString();
                var email = dataItem[1].ToString();
                var dob = dataItem[3].ToString();
                var month = dataItem[5].ToString();
                var expireDateStr = dataItem.Count == 7 ? dataItem[6].ToString() : null;

                if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(email) || (string.IsNullOrEmpty(month) && string.IsNullOrEmpty(expireDateStr)))
                {
                    methodResult.AddErrorBadRequest("Thiếu dữ liệu");
                    return methodResult;
                }

                if (!string.IsNullOrEmpty(month) && !string.IsNullOrEmpty(expireDateStr))
                {
                    methodResult.AddErrorBadRequest("Chỉ nhập Package hoặc ExpiredDate");
                    return methodResult;
                }

                DateTime dateOfBirth = DateTime.UtcNow;
                DateTime expireDate = DateTime.UtcNow;

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

            var result = await _mediator.Send(new CreateUsersAndOrdersByAdminCommand
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
