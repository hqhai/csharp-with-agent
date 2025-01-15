// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.GoogleSheetServices
{
    public interface IGoogleSheetService
    {
        IList<IList<object>> ReadDataFromSheet(string spreadsheetId, string sheetName);
    }
}
