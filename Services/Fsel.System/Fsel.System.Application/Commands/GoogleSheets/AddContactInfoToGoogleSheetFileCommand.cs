// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.GoogleSheets
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Shared.Constants;
    using Fsel.System.Domain.Models.CommandModels.GoogleSheets;
    using Fsel.System.Infrastructure.ValueSettings;
    using global::System;
    using global::System.Globalization;
    using global::System.Threading.Tasks;
    using Google.Apis.Auth.OAuth2;
    using Google.Apis.Services;
    using Google.Apis.Sheets.v4;
    using Google.Apis.Sheets.v4.Data;
    using MediatR;

    public class AddContactInfoToGoogleSheetFileCommand : IRequest<MethodResult<bool>>
    {
        public IList<AddContactInfoToGoogleSheetFileCommandModel>? Model { get; set; }
        public string? OverrideSpreadSheetId { get; set; }
        public string? OverrideSheet { get; set; }
        public IList<string>? ColumnOrder { get; set; }
    }

    public class AddContactInfoToGoogleSheetFileCommandHandler : IRequestHandler<AddContactInfoToGoogleSheetFileCommand, MethodResult<bool>>
    {
        private readonly AppSetting _appSetting;

        public AddContactInfoToGoogleSheetFileCommandHandler(AppSetting appSetting)
        {
            _appSetting = appSetting;
        }

        public async Task<MethodResult<bool>> Handle(AddContactInfoToGoogleSheetFileCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var spreadSheetId = !string.IsNullOrEmpty(request.OverrideSpreadSheetId)
                ? request.OverrideSpreadSheetId
                : _appSetting.GoogleSheetConfig?.LandingPageSpreadSheetId;
            var sheet = !string.IsNullOrEmpty(request.OverrideSheet)
                ? request.OverrideSheet
                : _appSetting.GoogleSheetConfig?.LandingPageSheet;

            if (string.IsNullOrEmpty(spreadSheetId))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var credentialsPath = ResourceSettings.I18NCredentialsFilePath;

            var defaultOrder = new List<string> { "Email", "PhoneNumber", "FullName", "Time" };
            var columnOrder = request.ColumnOrder?.Any() == true ? request.ColumnOrder : defaultOrder;

            var data = request.Model?.Select(item =>
            {
                var row = new List<object>();
                foreach (var col in columnOrder)
                {
                    switch (col)
                    {
                        case "Email":
                            row.Add(item?.Email ?? string.Empty);
                            break;

                        case "PhoneNumber":
                            row.Add(item?.PhoneNumber ?? string.Empty);
                            break;

                        case "FullName":
                            row.Add(item?.FullName ?? string.Empty);
                            break;

                        case "Time":
                            row.Add(DateTimeHelper.ConvertTimeFromUtc(DateTime.UtcNow, EnumCountryKey.Vietnam)
                                .ToString("dd-MM-yyyy HH:mm", CultureInfo.CurrentCulture));
                            break;

                        default:
                            row.Add(string.Empty);
                            break;
                    }
                }
                return row;
            }).ToList();

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
                valueRange.Values = data?.ToArray();

                var appendRequest = service.Spreadsheets.Values.Append(valueRange, spreadSheetId, $"{sheet}!A:A");
                appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
                var appendResponse = await appendRequest.ExecuteAsync(cancellationToken);
            }

            return methodResult;
        }
    }
}
