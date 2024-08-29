// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.GoogleSheets
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Shared.Constants;
    using Fsel.System.Domain.Models.CommandModels.GoogleSheets;
    using Fsel.System.Infrastructure.ValueSettings;
    using global::System.Globalization;
    using Google.Apis.Auth.OAuth2;
    using Google.Apis.Services;
    using Google.Apis.Sheets.v4.Data;
    using Google.Apis.Sheets.v4;
    using MediatR;

    public class AddVouchersForMAIntoGoogleSheetCommand : AddVouchersForMAIntoGoogleSheetCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class AddVouchersForMAIntoGoogleSheetCommandHandler : IRequestHandler<AddVouchersForMAIntoGoogleSheetCommand, MethodResult<bool>>
    {
        private readonly AppSetting _appSetting;

        public AddVouchersForMAIntoGoogleSheetCommandHandler(AppSetting appSetting)
        {
            _appSetting = appSetting;
        }

        public async Task<MethodResult<bool>> Handle(AddVouchersForMAIntoGoogleSheetCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var spreadSheetId = _appSetting.GoogleSheetConfig?.VoucherForMASpreadSheetId;
            var sheet = request.MACode;

            if (string.IsNullOrEmpty(spreadSheetId) || string.IsNullOrEmpty(sheet))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var credentialsPath = ResourceSettings.I18NCredentialsFilePath;

            var currentDate = DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam).ToString("dd-MM-yyyy HH:mm", CultureInfo.CurrentCulture);

            var data = new List<IList<object>>();

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
                // Lấy thông tin về các sheet trong spreadsheet
                var spreadsheet = await service.Spreadsheets.Get(spreadSheetId).ExecuteAsync(cancellationToken);
                var sheetExists = spreadsheet.Sheets.Any(s => s.Properties.Title == sheet);

                if (!sheetExists)
                {
                    // Thêm mới sheet nếu không tồn tại
                    var addSheetRequest = new AddSheetRequest
                    {
                        Properties = new SheetProperties
                        {
                            Title = sheet
                        }
                    };

                    var batchUpdateRequest = new BatchUpdateSpreadsheetRequest
                    {
                        Requests = new List<Request> { new Request { AddSheet = addSheetRequest } }
                    };

                    await service.Spreadsheets.BatchUpdate(batchUpdateRequest, spreadSheetId).ExecuteAsync(cancellationToken);
                    data.Add(new List<object>
                    {
                        "Voucher",
                        "Package",
                        "Expired Date",
                        "Created Date"
                    });
                }

                var requestBody = service.Spreadsheets.Values.Get(spreadSheetId, $"{sheet}!A:A");
                var response = await requestBody.ExecuteAsync(cancellationToken);

                var valueRange = new ValueRange();

                request.Vouchers.ForEach(p =>
                {
                    data.Add(new List<object>
                    {
                        p ?? string.Empty,
                        request.Package ?? string.Empty,
                        request.ExpiredDate ?? string.Empty,
                        currentDate
                    });
                });
                valueRange.Values = data.ToArray();

                var appendRequest = service.Spreadsheets.Values.Append(valueRange, spreadSheetId, $"{sheet}!A:A");
                appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
                var appendResponse = await appendRequest.ExecuteAsync(cancellationToken);
            }

            return methodResult;
        }
    }
}
