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

    public class AddDynamicInfoToGoogleSheetFileCommand : IRequest<MethodResult<bool>>
    {
        public IList<Dictionary<string, object>>? Model { get; set; }
        public string? OverrideSpreadSheetId { get; set; }
        public string? OverrideSheet { get; set; }
        public IList<string> ColumnOrder { get; set; }
    }

    public class AddDynamicInfoToGoogleSheetFileCommandHandler : IRequestHandler<AddDynamicInfoToGoogleSheetFileCommand, MethodResult<bool>>
    {
        private readonly AppSetting _appSetting;

        public AddDynamicInfoToGoogleSheetFileCommandHandler(AppSetting appSetting)
        {
            _appSetting = appSetting;
        }

        public async Task<MethodResult<bool>> Handle(AddDynamicInfoToGoogleSheetFileCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            if (request.ColumnOrder == null || !request.ColumnOrder.Any())
            {
                methodResult.AddErrorBadRequest("ColumnOrder is required.");
                return methodResult;
            }

            if (request.Model == null || !request.Model.Any())
            {
                methodResult.AddErrorBadRequest("Model data is required.");
                return methodResult;
            }

            var spreadSheetId = !string.IsNullOrEmpty(request.OverrideSpreadSheetId)
                ? request.OverrideSpreadSheetId
                : _appSetting.GoogleSheetConfig?.LandingPageSpreadSheetId;

            var sheet = !string.IsNullOrEmpty(request.OverrideSheet)
                ? request.OverrideSheet
                : _appSetting.GoogleSheetConfig?.LandingPageSheet;

            if (string.IsNullOrEmpty(spreadSheetId) || string.IsNullOrEmpty(sheet))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var credentialsPath = ResourceSettings.I18NCredentialsFilePath;

            var data = request.Model.Select(item =>
            {
                var row = new List<object>();
                foreach (var col in request.ColumnOrder!)
                {
                    if (string.Equals(col, "CreatedTime", StringComparison.OrdinalIgnoreCase))
                    {
                        row.Add(DateTimeHelper.ConvertTimeFromUtc(DateTime.UtcNow, EnumCountryKey.Vietnam)
                            .ToString("dd-MM-yyyy HH:mm", CultureInfo.CurrentCulture));
                        continue;
                    }

                    // Get value by key if exists
                    if (item.TryGetValue(col, out var value))
                    {
                        row.Add(value ?? string.Empty);
                    }
                    else
                    {
                        row.Add(string.Empty);
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

                var valueRange = new ValueRange
                {
                    Values = data.Select(d => (IList<object>)d).ToList()
                };

                var appendRequest = service.Spreadsheets.Values.Append(valueRange, spreadSheetId, $"{sheet}!A:A");
                appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
                var appendResponse = await appendRequest.ExecuteAsync(cancellationToken);
            }

            return methodResult;
        }
    }
}
