// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Services.GoogleSheetServices
{
    using Google.Apis.Auth.OAuth2;
    using Google.Apis.Services;
    using Google.Apis.Sheets.v4;
    using Google.Apis.Sheets.v4.Data;

    public class GoogleSheetService : IGoogleSheetService, IDisposable
    {
        private readonly SheetsService _sheetsService;
        private static SheetsService? _sharedService;

        public GoogleSheetService(string credentialsFilePath)
        {
            if (_sharedService == null)
            {
                using var stream = new FileStream(credentialsFilePath, FileMode.Open, FileAccess.Read);
                var credential = GoogleCredential.FromStream(stream)
                    .CreateScoped(SheetsService.Scope.Spreadsheets);

                _sharedService = new SheetsService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "FSEL"
                });
            }

            _sheetsService = _sharedService!;
        }

        public IList<IList<object>> ReadDataFromSheet(string spreadsheetId, string sheetName)
        {
            SpreadsheetsResource.ValuesResource.GetRequest request =
                _sheetsService.Spreadsheets.Values.Get(spreadsheetId, sheetName);

            ValueRange response = request.Execute();
            IList<IList<object>> values = response.Values;

            return values;
        }

        public async Task AppendRowsAsync(string spreadsheetId, string sheetName, IList<IList<object>> rows, CancellationToken cancellationToken)
        {
            var valueRange = new ValueRange { Values = rows };

            var appendRequest = _sheetsService.Spreadsheets.Values.Append(valueRange, spreadsheetId, sheetName);
            appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;

            await appendRequest.ExecuteAsync(cancellationToken);
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
    }
}
