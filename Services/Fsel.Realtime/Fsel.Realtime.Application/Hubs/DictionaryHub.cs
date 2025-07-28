// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Realtime.Application.Hubs
{
    using Fsel.Core.Base;
    using Fsel.Core.Extensions;
    using Fsel.Core.Services.IpApiServices;
    using Fsel.Realtime.Application.Queues.Publishers;
    using Microsoft.AspNetCore.Http;

    public class DictionaryHub : BaseHub
    {
        private readonly DictionaryPublisher _dictionaryPublisher;
        private readonly AuthContext _authContext;

        public DictionaryHub(AuthContext authContext, IIpApiService ipApiService, IHttpContextAccessor httpContextAccessor, DictionaryPublisher dictionaryPublisher) : base(authContext, ipApiService, httpContextAccessor)
        {
            _dictionaryPublisher = dictionaryPublisher;
            _authContext = authContext;
        }

        public override async Task OnConnectedHubAsync()
        {
            await Groups.AddGroupAsync(Context.ConnectionId, _authContext.CurrentUserId.ToString());
        }

        public override async Task OnDisconnectedHubAsync(Exception? exception)
        {
            await Groups.RemoveGroupAsync(Context.ConnectionId, _authContext.CurrentUserId.ToString());
        }

        public async Task GetDictionary(string? word)
        {
            if (string.IsNullOrEmpty(word))
            {
                return;
            }

            await _dictionaryPublisher.Publish(word, CancellationToken.None);
        }
    }
}
