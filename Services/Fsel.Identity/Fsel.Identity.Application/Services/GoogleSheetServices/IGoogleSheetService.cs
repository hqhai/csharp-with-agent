// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Services.GoogleSheetServices
{
    public interface IGoogleSheetService
    {
        IList<IList<object>> ReadDataFromSheet(string spreadsheetId, string sheetName);

        bool CreateDataFromSheet(string spreadsheetId, string range, IList<IList<object>> values);
    }
}
