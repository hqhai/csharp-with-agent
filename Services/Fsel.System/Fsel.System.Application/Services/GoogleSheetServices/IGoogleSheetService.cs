// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Services.GoogleSheetServices
{
    public interface IGoogleSheetService
    {
        IList<IList<object>> ReadDataFromSheet(string spreadsheetId, string sheetName);
        Task AppendRowsAsync(string spreadsheetId, string sheetName, IList<IList<object>> rows, CancellationToken cancellationToken);
    }
}
