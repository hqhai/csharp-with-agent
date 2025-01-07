// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Repositories
{
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Infrastructure.ValueSettings;
    using global::System.Net.Http.Headers;
    using global::System.Text;
    using Microsoft.Identity.Client;

    public class SharePointService : ISharePointService
    {
        private readonly string _tenantId;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly AppSetting _appSetting;

        public SharePointService(AppSetting appSetting)
        {
            _appSetting = appSetting;
            _tenantId = appSetting?.SharePointConfig?.TenantId ?? string.Empty;
            _clientId = appSetting?.SharePointConfig?.ClientId ?? string.Empty;
            _clientSecret = appSetting?.SharePointConfig?.ClientSecret ?? string.Empty;
        }

        public async Task<string> GetAccessTokenAsync()
        {
            IConfidentialClientApplication app = ConfidentialClientApplicationBuilder.Create(_clientId)
                .WithTenantId(_tenantId)
                .WithClientSecret(_clientSecret)
                .Build();

            var result = await app.AcquireTokenForClient(new[] { "https://graph.microsoft.com/.default" }).ExecuteAsync();
            return result.AccessToken;
        }

        public async Task<bool> AddDataToExcelFile(string siteId, string fileId, string sheetName, List<List<string>> data)
        {
            if (data == null || data.Count == 0)
            {
                return false;
            }

            var accessToken = await GetAccessTokenAsync();

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var getRangeUrl = $"https://graph.microsoft.com/v1.0/sites/{siteId}/drive/items/{fileId}/workbook/worksheets('{sheetName}')/usedRange";
                var getRangeResponse = await httpClient.GetAsync(getRangeUrl);

                if (getRangeResponse.IsSuccessStatusCode)
                {
                    var usedRangeJson = await getRangeResponse.Content.ReadAsStringAsync();
                    var usedRange = global::System.Text.Json.JsonDocument.Parse(usedRangeJson);
                    var rowCount = usedRange.RootElement.GetProperty("values").GetArrayLength();

                    int dataRowCount = data.Count;
                    int dataColCount = data.Select(p => p.Count).Max();

                    string startAddress = $"A{rowCount + 1}";
                    string endCol = GetExcelColumnName(dataColCount);
                    int endRow = rowCount + dataRowCount;
                    string endAddress = $"{endCol}{endRow}";

                    var url = $"https://graph.microsoft.com/v1.0/sites/{siteId}/drive/items/{fileId}/workbook/worksheets('{sheetName}')/range(address='{startAddress}:{endAddress}')";

                    data.ForEach(p => p.AddRange(Enumerable.Repeat(string.Empty, dataColCount - p.Count)));
                    var content = new
                    {
                        values = data.Select(row => row.ToArray()).ToArray()
                    };

                    using (var jsonContent = new StringContent(global::System.Text.Json.JsonSerializer.Serialize(content), Encoding.UTF8, "application/json"))
                    {
                        var response = await httpClient.PatchAsync(url, jsonContent);
                        if (response.IsSuccessStatusCode)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    return false;
                }
            }
        }

        private static string GetExcelColumnName(int columnNumber)
        {
            string columnName = "";
            while (columnNumber > 0)
            {
                int modulo = (columnNumber - 1) % 26;
                columnName = Convert.ToChar(65 + modulo) + columnName;
                columnNumber = (columnNumber - modulo) / 26;
            }
            return columnName;
        }
    }
}
