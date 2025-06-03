namespace Fsel.Sender.Application.Services.ZaloServices
{
    using System.Text;
    using System.Text.Json;
    using Fsel.Sender.Application.Services.ZaloServices.Models;
    using Microsoft.Extensions.Logging;
    using Polly;

    public class ZaloService : IZaloService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ZaloService> _logger;
        private const string ErrorCode = "000";
        private const string Url = "https://api-ott.brandsms.vn/api/ott/send";

        public ZaloService(HttpClient httpClient, ILogger<ZaloService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<SendSMSByZaloResponseModel?> SendSMSAsync(SendSMSByZaloRequestModel request, string token)
        {
            var json = JsonSerializer.Serialize(request);

            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            return await Send(Url, content, token);
        }

        private async Task<SendSMSByZaloResponseModel?> Send(string url, StringContent content, string token)
        {
            var retryPolicyDC = Policy
                                .HandleResult<SendSMSByZaloResponseModel?>(tokenResult => tokenResult == null || tokenResult.ErrorCode != ErrorCode)
                                .Or<Exception>()
                                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(retryAttempt), async (result, timeSpan, retryCount, context) =>
                                {
                                    _logger.LogError($"Retry {retryCount} cho API Zalo thất bại: {result?.Result}");
                                });

            try
            {
                var tokenResult = await retryPolicyDC.ExecuteAsync(async () =>
                {
                    var httpRequest = new HttpRequestMessage(HttpMethod.Post, url)
                    {
                        Content = content
                    };

                    httpRequest.Headers.Add("token", token);

                    var response = await _httpClient.SendAsync(httpRequest);

                    response.EnsureSuccessStatusCode();

                    var responseString = await response.Content.ReadAsStringAsync();

                    var result = JsonSerializer.Deserialize<SendSMSByZaloResponseModel>(responseString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    return result;
                });

                return tokenResult;
            }
            catch (Exception ex)
            {
                _logger.LogError($"API Zalo thất bại sau retry: {ex.Message}");
            }

            return null;
        }
    }
}
