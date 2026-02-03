// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Realtime.Application.Hubs
{
    using Fsel.Core.Base;
    using Fsel.Core.Extensions;
    using Fsel.Core.Services.IpApiServices;
    using Fsel.Realtime.Application.Queues.Publishers;
    using Fsel.Shared.Models.ShareModels;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// Hub for Semantic Dictionary AI responses
    /// </summary>
    public class SemanticDictionaryHub : BaseHub
    {
        private readonly SemanticDictionaryPublisher _publisher;
        private readonly AuthContext _authContext;

        public SemanticDictionaryHub(
            AuthContext authContext,
            IIpApiService ipApiService,
            IHttpContextAccessor httpContextAccessor,
            SemanticDictionaryPublisher publisher)
            : base(authContext, ipApiService, httpContextAccessor)
        {
            _publisher = publisher;
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

        /// <summary>
        /// Client invokes this method to request semantic dictionary lookup
        /// </summary>
        public async Task SearchSemanticDictionary(SemanticDictionaryRequestModel request)
        {
            if (request == null || string.IsNullOrEmpty(request.HighlightedItem))
            {
                return;
            }

            await _publisher.Publish(_authContext.CurrentUserId.ToString(), request, CancellationToken.None);
        }
    }
}
