// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Services.GoogleSheetServices
{
    using Google.Apis.Auth.OAuth2;
    using Google.Apis.Services;
    using Google.Apis.Sheets.v4;
    using Google.Apis.Sheets.v4.Data;

    public class GoogleSheetService : IGoogleSheetService, IDisposable
    {
        private readonly SheetsService _sheetsService;

        public GoogleSheetService(string credentialsFilePath)
        {
            GoogleCredential credential;

            using (var stream = new FileStream(credentialsFilePath, FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream)
                    .CreateScoped(SheetsService.Scope.Spreadsheets);
            }

            _sheetsService = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "FSEL"
            });
        }

        public IList<IList<object>> ReadDataFromSheet(string spreadsheetId, string sheetName)
        {
            SpreadsheetsResource.ValuesResource.GetRequest request =
                _sheetsService.Spreadsheets.Values.Get(spreadsheetId, sheetName);

            ValueRange response = request.Execute();
            IList<IList<object>> values = response.Values;

            return values;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _sheetsService.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public bool CreateDataFromSheet(string spreadsheetId, string range, IList<IList<object>> values)
        {
            var valueRange = new ValueRange
            {
                Values = values
            };

            var appendRequest = _sheetsService.Spreadsheets.Values.Append(valueRange, spreadsheetId, range);

            // sử dụng USERENTERED sẽ xử lý giống như cách người dùng nhập vào Google Sheets (ví dụ dùng hàm SUM sẽ trả ra kết quả sau khi được SUM)
            // còn RAW khi dùng mã ko muốn Google Sheets xử lý (nhập hàm SUM sẽ dữ nguyên hàm)
            appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;

            var response = appendRequest.Execute();

            if (response.Updates.UpdatedRows <= 0)
            {
                return false;
            }

            return true;
        }
    }
}
