// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.IRepositories
{
    using global::System.Collections.Generic;
    using global::System.Threading.Tasks;

    public interface ISharePointService
    {
        Task<bool> AddDataToExcelFile(string siteId, string fileId, string sheetName, List<List<string>> data);
    }
}
