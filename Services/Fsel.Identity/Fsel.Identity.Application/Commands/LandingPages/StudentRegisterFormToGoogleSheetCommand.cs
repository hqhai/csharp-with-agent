// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.LandingPages
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Identity.Application.Services.SystemService;
    using Fsel.Identity.Domain.Models.CommandModels.GoogleSheets;
    using Fsel.Identity.Domain.Models.CommandModels.LandingPages;
    using Fsel.Identity.Infrastructure.ValueSettings;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;

    public class StudentRegisterFormToGoogleSheetCommand : StudentRegisterFormToGoogleSheetCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class StudentRegisterFormToGoogleSheetCommandHandler : IRequestHandler<StudentRegisterFormToGoogleSheetCommand, MethodResult<bool>>
    {
        private readonly AppSetting _appSetting;
        private readonly ISystemService _systemService;

        public StudentRegisterFormToGoogleSheetCommandHandler(AppSetting appSetting,
                                                              ISystemService systemService)
        {
            _appSetting = appSetting;
            _systemService = systemService;
        }

        public async Task<MethodResult<bool>> Handle(StudentRegisterFormToGoogleSheetCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            if ((!string.IsNullOrEmpty(request.Email) && !request.Email.IsValidEmail()) || (!string.IsNullOrEmpty(request.ParentEmail) && !request.ParentEmail.IsValidEmail()))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat));
                return methodResult;
            }

            if ((!string.IsNullOrEmpty(request.PhoneNumber) && !request.PhoneNumber.IsValidPhoneNumber()) || (!string.IsNullOrEmpty(request.ParentPhoneNumber) && !request.ParentPhoneNumber.IsValidPhoneNumber()))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat));
                return methodResult;
            }

            if (string.IsNullOrEmpty(request.FirstName) || string.IsNullOrEmpty(request.LastName))
            {
                methodResult.AddErrorBadRequest(nameof(EnumValidateInputDataErrorCode.DataIsRequired));
                return methodResult;
            }
            await AddContactInfoToGGSheet(request);
            return methodResult;
        }

        private async Task AddContactInfoToGGSheet(StudentRegisterFormToGoogleSheetCommandModel request)
        {
            var columnOrder = new List<string> { "FirstName", "LastName", "Email", "PhoneNumber", "ParentEmail", "ParentPhoneNumber", "School", "District", "SchoolGrade", "SchoolClass" };

            var dict = new Dictionary<string, object>();

            foreach (var column in columnOrder)
            {
                var prop = request.GetType().GetProperty(column);
                var value = prop?.GetValue(request) ?? string.Empty;
                dict[column] = value;
            }

            var dicts = new List<Dictionary<string, object>> { dict };

            var sheetName = _appSetting?.GoogleSheetConfig?.StudentRegisterFormSheet;
            var spreadSheetId = _appSetting?.GoogleSheetConfig?.StudentRegisterFormSheetId;

            await _systemService.AddDynamicInfoToGoogleSheetFile(new CreateDynamicInfosToGoogleSheetFileCommandModel
            {
                Model = dicts,
                OverrideSheet = sheetName,
                OverrideSpreadSheetId = spreadSheetId,
                ColumnOrder = columnOrder
            });
        }
    }
}
