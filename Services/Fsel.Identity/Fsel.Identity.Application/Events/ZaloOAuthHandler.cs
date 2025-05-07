using System.Net.Http.Headers;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fsel.Identity.Application.Events
{
    public class ZaloOAuthHandler : OAuthHandler<OAuthOptions>
    {
        public ZaloOAuthHandler(
            IOptionsMonitor<OAuthOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        protected override async Task<OAuthTokenResponse> ExchangeCodeAsync(OAuthCodeExchangeContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            var parameters = new Dictionary<string, string>
            {
                { "app_id", Options.ClientId }, // Zalo yêu cầu 'app_id' chứ không phải 'client_id'
                { "grant_type", "authorization_code" },
                { "code", context.Code },
            };

            if (context.Properties.Items.TryGetValue("code_verifier", out var codeVerifier) && !string.IsNullOrEmpty(codeVerifier))
            {
                parameters.Add("code_verifier", codeVerifier);
            }

            var request = new HttpRequestMessage(HttpMethod.Post, Options.TokenEndpoint);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Add("secret_key", Options.ClientSecret); // Gửi secret_key vào header

            request.Content = new FormUrlEncodedContent(parameters);

            var response = await Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, Context.RequestAborted);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(Context.RequestAborted);
                return OAuthTokenResponse.Failed(new Exception($"OAuth token endpoint failure: {response.StatusCode}; {error}"));
            }

            var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync(Context.RequestAborted));
            return OAuthTokenResponse.Success(payload);
        }
    }
}
