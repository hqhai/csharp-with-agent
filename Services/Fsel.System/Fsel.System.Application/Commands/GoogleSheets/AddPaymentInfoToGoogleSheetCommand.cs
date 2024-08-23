// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.GoogleSheets
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Shared.Constants;
    using Fsel.System.Domain.Models.CommandModels.GoogleSheets;
    using Fsel.System.Infrastructure.ValueSettings;
    using Google.Apis.Auth.OAuth2;
    using Google.Apis.Services;
    using Google.Apis.Sheets.v4;
    using Google.Apis.Sheets.v4.Data;
    using MediatR;

    public class AddPaymentInfoToGoogleSheetCommand : AddPaymentInfoToGoogleSheetCommandModel, IRequest<MethodResult<VoidMethodResult>>
    {
    }

    public class AddPaymentInfoToGoogleSheetCommandHandler : IRequestHandler<AddPaymentInfoToGoogleSheetCommand, MethodResult<VoidMethodResult>>
    {
        private readonly AppSetting _appSetting;

        public AddPaymentInfoToGoogleSheetCommandHandler(AppSetting appSetting)
        {
            _appSetting = appSetting;
        }

        public async Task<MethodResult<VoidMethodResult>> Handle(AddPaymentInfoToGoogleSheetCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<VoidMethodResult>();

            var spreadSheetId = _appSetting.GoogleSheetConfig?.OrderInfoSpreadSheetId;
            var sheet = _appSetting.GoogleSheetConfig?.OrderInfoSheet;

            if (string.IsNullOrEmpty(spreadSheetId))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var credentialsPath = ResourceSettings.I18NCredentialsFilePath;

            var data = new List<List<object>>() {
                new List<object>()
                {
                    request.Code ?? string.Empty,
                    request.CreatedDate ?? string.Empty,
                    request.Price ?? string.Empty,
                    request.FullName ?? string.Empty,
                    request.StudentEmail ?? string.Empty,
                    request.BillingEmail ?? string.Empty,
                    request.CompanyTaxCode ?? string.Empty,
                    request.CompanyName ?? string.Empty,
                    request.CompanyAddress ?? string.Empty,
                }
            };

            GoogleCredential credential;

            using (var stream = new FileStream(credentialsPath, FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream)
                    .CreateScoped(new[] { SheetsService.Scope.Spreadsheets });
            }

            using (var service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Google Sheets Integration",
            }))
            {
                var requestBody = service.Spreadsheets.Values.Get(spreadSheetId, $"{sheet}!A:A");
                var response = await requestBody.ExecuteAsync(cancellationToken);

                var valueRange = new ValueRange();
                valueRange.Values = data.ToArray();

                var appendRequest = service.Spreadsheets.Values.Append(valueRange, spreadSheetId, $"{sheet}!A:A");
                appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
                var appendResponse = await appendRequest.ExecuteAsync(cancellationToken);
            }

            return methodResult;
        }
    }
}
