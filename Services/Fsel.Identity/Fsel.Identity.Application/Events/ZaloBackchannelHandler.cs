using System.Text;

namespace Fsel.Identity.Application.Events
{
    public class ZaloBackchannelHandler : HttpClientHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (request.Content != null && request.RequestUri != null && request.RequestUri.AbsoluteUri.Contains("access_token", StringComparison.InvariantCulture))
            {
                var content = await request.Content.ReadAsStringAsync(cancellationToken);
                content = content.Replace("client_id", "app_id", StringComparison.InvariantCulture);
                request.Content = new StringContent(content, Encoding.UTF8, "application/x-www-form-urlencoded");
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
