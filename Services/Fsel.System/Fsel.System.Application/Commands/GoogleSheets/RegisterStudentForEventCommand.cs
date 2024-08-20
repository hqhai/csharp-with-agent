// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.GoogleSheets
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels;
    using Fsel.System.Infrastructure.ValueSettings;
    using global::System.Globalization;
    using Google.Apis.Auth.OAuth2;
    using Google.Apis.Services;
    using Google.Apis.Sheets.v4;
    using Google.Apis.Sheets.v4.Data;
    using MediatR;

    public class RegisterStudentForEventCommand : RegisterStudentForEventCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class RegisterStudentForEventCommandHandler : IRequestHandler<RegisterStudentForEventCommand, MethodResult<bool>>
    {
        private readonly AppSetting _appSetting;

        public RegisterStudentForEventCommandHandler(AppSetting appSetting)
        {
            _appSetting = appSetting;
        }

        public async Task<MethodResult<bool>> Handle(RegisterStudentForEventCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var spreadSheetId = _appSetting.GoogleSheetConfig?.RegisterStudentForEventSpreadSheetId;
            var sheet = request.EventCode;

            if (string.IsNullOrEmpty(spreadSheetId) || string.IsNullOrEmpty(sheet))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var credentialsPath = ResourceSettings.I18NCredentialsFilePath;

            var birthday = request.BirthDay.ToString("dd-MM-yyyy", CultureInfo.CurrentCulture);
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
                        "First Name",
                        "Last Name",
                        "Email",
                        "Phone Number",
                        "Birthday",
                        "Province",
                        "District",
                        "School",
                        "Grade",
                        "Class",
                        "Student Code",
                        "Created Date",
                        "Status",
                    });
                }

                var requestBody = service.Spreadsheets.Values.Get(spreadSheetId, $"{sheet}!A:A");
                var response = await requestBody.ExecuteAsync(cancellationToken);

                var valueRange = new ValueRange();
                data.Add(new List<object>
            {
                request.FirstName ?? string.Empty,
                request.LastName?? string.Empty,
                request.Email ?? string.Empty,
                request.PhoneNumber ?? string.Empty,
                birthday,
                request.Province ?? string.Empty,
                request.District ?? string.Empty,
                request.School ?? string.Empty,
                request.SchoolGrade ?? string.Empty,
                request.SchoolClass ?? string.Empty,
                request.SchoolStudentCode ?? string.Empty,
                currentDate
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
