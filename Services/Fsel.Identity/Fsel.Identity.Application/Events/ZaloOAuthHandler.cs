using System.Net.Http.Headers;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using static IdentityServer4.IdentityServerConstants;

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
                { OAuthFields.AppId, Options.ClientId },
                { OAuthFields.GrantType, PersistedGrantTypes.AuthorizationCode },
                { OAuthFields.Code, context.Code }
            };

            if (context.Properties.Items.TryGetValue(OAuthFields.CodeVerifier, out var codeVerifier) && !string.IsNullOrEmpty(codeVerifier))
            {
                parameters.Add(OAuthFields.CodeVerifier, codeVerifier);
            }

            var content = new FormUrlEncodedContent(parameters);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

            var request = new HttpRequestMessage(HttpMethod.Post, Options.TokenEndpoint);
            request.Content = content;
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Add(OAuthFields.ClientSecret, Options.ClientSecret); // Gửi secret_key trong header

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
