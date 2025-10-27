// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Realtime.Application.Hubs
{
    using System;
    using System.Threading.Tasks;
    using Fsel.Core.Base;
    using Fsel.Core.Extensions;
    using Fsel.Core.Services.IpApiServices;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.SignalR;

    public class ExamPracticeWritingHub : BaseHub
    {
        public ExamPracticeWritingHub(AuthContext authContext, IIpApiService ipApiService, IHttpContextAccessor httpContextAccessor) : base(authContext, ipApiService, httpContextAccessor)
        {
        }

        public override async Task OnConnectedHubAsync()
        {
            string mockTestCriteria = Context.GetHttpContext()?.Request.Query["ExamPracticeResultId"].ToString()!;
            if (!string.IsNullOrEmpty(mockTestCriteria))
            {
                await Groups.AddGroupAsync(Context.ConnectionId, mockTestCriteria);
            }
        }

        public override async Task OnDisconnectedHubAsync(Exception? exception)
        {
            string mockTestCriteria = Context.GetHttpContext()?.Request.Query["ExamPracticeResultId"].ToString()!;
            if (!string.IsNullOrEmpty(mockTestCriteria))
            {
                await Groups.RemoveGroupAsync(Context.ConnectionId, mockTestCriteria);
            }
        }
    }
}
