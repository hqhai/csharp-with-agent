// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.GoogleSheetServices
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
    }
}
