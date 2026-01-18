// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Realtime.Application.Hubs
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Core.Base;
    using Fsel.Core.Extensions;
    using Fsel.Core.Services.IpApiServices;
    using Fsel.Realtime.Application.Queues.Publishers;
    using Fsel.Shared.Models.ShareModels;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.SignalR;
    public class TranslationHub : BaseHub
    {
        private readonly AuthContext _authContext;
        private readonly AITranslationRequestPublisher _translationRequestPublisher;


        public TranslationHub(AuthContext authContext,
                             IIpApiService ipApiService,
                             IHttpContextAccessor httpContextAccessor,
                             AITranslationRequestPublisher translationRequestPublisher) : base(authContext, ipApiService, httpContextAccessor)
        {
            _authContext = authContext;
            _translationRequestPublisher = translationRequestPublisher;
        }

        public override async Task OnConnectedHubAsync()
        {
            string classForumDetailResultId = Context.GetHttpContext()?.Request.Query["ClassForumDetailResultId"].ToString()!;
            if (!string.IsNullOrEmpty(classForumDetailResultId))
            {
                await Groups.AddGroupAsync(Context.ConnectionId, classForumDetailResultId);
            }
        }

        /// <summary>
        /// Client gọi hàm này để yêu cầu phiên dịch AI response sang tiếng Việt
        /// </summary>
        /// <param name="request">Thông tin yêu cầu phiên dịch</param>
        public async Task TranslateAIResponse(AITranslationRequestModel request)
        {
            if (IsInvalidRequest(request))
            {
                return;
            }

            await JoinTranslationGroupAsync(request.ClassForumDetailResultId.ToString());

            await PublishTranslationRequestAsync(request);
        }

        public override async Task OnDisconnectedHubAsync(Exception? exception)
        {
            string classForumDetailResultId = Context.GetHttpContext()?.Request.Query["ClassForumDetailResultId"].ToString()!;
            if (!string.IsNullOrEmpty(classForumDetailResultId))
            {
                await Groups.RemoveGroupAsync(Context.ConnectionId, classForumDetailResultId);
            }
        }

        #region Private Methods

        private static bool IsInvalidRequest(AITranslationRequestModel request)
        {
            return request == null;
        }

        private async Task JoinTranslationGroupAsync(string groupId)
        {
            await Groups.AddGroupAsync(Context.ConnectionId, groupId);
        }

        private async Task PublishTranslationRequestAsync(AITranslationRequestModel request)
        {
            await _translationRequestPublisher.Publish(request, CancellationToken.None);
        }

        #endregion
    }
}
