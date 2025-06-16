// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.LandingPages
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Identity.Application.Services.SystemService;
    using Fsel.Identity.Domain.Models.CommandModels.GoogleSheets;
    using Fsel.Identity.Infrastructure.ValueSettings;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;

    public class FormRegisterStudentForSummerSelfStudyCommand : IRequest<MethodResult<bool>>
    {
        public required string FullName { get; set; }
        public required string Email { get; set; }
        public required string PhoneNumber { get; set; }
        public string? DiscountCode { get; set; }
        public string? CampaignId { get; set; }
        public string? CampaignSource { get; set; }
        public string? CampaignMedium { get; set; }
        public string? CampaignName { get; set; }
        public string? CampaignTerm { get; set; }
        public string? CampaignContent { get; set; }
    }

    public class FormRegisterStudentForSummerSelfStudyCommandHandler : IRequestHandler<FormRegisterStudentForSummerSelfStudyCommand, MethodResult<bool>>
    {
        private readonly AppSetting _appSetting;
        private readonly ISystemService _systemService;

        public FormRegisterStudentForSummerSelfStudyCommandHandler(
            AppSetting appSetting,
            ISystemService systemService)
        {
            _appSetting = appSetting;
            _systemService = systemService;
        }

        public async Task<MethodResult<bool>> Handle(FormRegisterStudentForSummerSelfStudyCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();
            if (!string.IsNullOrEmpty(request.Email) && !request.Email.IsValidEmail())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat));
                return methodResult;
            }
            if (!string.IsNullOrEmpty(request.PhoneNumber) && !request.PhoneNumber.IsValidPhoneNumber())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat));
                return methodResult;
            }
            if (string.IsNullOrEmpty(request.FullName))
            {
                methodResult.AddErrorBadRequest(nameof(EnumValidateInputDataErrorCode.DataIsRequired));
                return methodResult;
            }
            await AddContactInfoToGGSheet(request);
            return methodResult;
        }

        private async Task AddContactInfoToGGSheet(FormRegisterStudentForSummerSelfStudyCommand request)
        {
            var columnOrder = new List<string> { "FullName", "Email", "PhoneNumber","DiscountCode", "Time",
                "CampaignId", "CampaignSource", "CampaignMedium", "CampaignName", "CampaignTerm", "CampaignContent"
            };
            var dict = new Dictionary<string, object>();

            foreach (var column in columnOrder)
            {
                if (string.Equals(column, "Time", StringComparison.OrdinalIgnoreCase))
                {
                    dict["Time"] = DateTimeHelper.ConvertTimeFromUtc(DateTime.UtcNow, EnumCountryKey.Vietnam)
                        .ToString("dd-MM-yyyy HH:mm", CultureInfo.CurrentCulture);
                    continue;
                }

                var prop = request.GetType().GetProperty(column);
                var value = prop?.GetValue(request) ?? string.Empty;
                dict[column] = value;
            }

            var modelDictList = new List<Dictionary<string, object>> { dict };

            var sheetName = _appSetting?.GoogleSheetConfig?.SummerSelfLearningSheet;
            var spreadSheetId = _appSetting?.GoogleSheetConfig?.SummerSelfLearningSpreadSheetId;

            await _systemService.AddDynamicInfoToGoogleSheetFile(new CreateDynamicInfosToGoogleSheetFileCommandModel
            {
                Model = modelDictList,
                OverrideSheet = sheetName,
                OverrideSpreadSheetId = spreadSheetId,
                ColumnOrder = columnOrder
            });
        }
    }
}
