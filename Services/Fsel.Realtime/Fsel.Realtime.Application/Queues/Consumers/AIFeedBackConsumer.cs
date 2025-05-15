using Fsel.Core.Base;
using Fsel.Core.Base.Interfaces;
using Fsel.Core.Extensions;
using Fsel.Realtime.Application.Hubs;
using Fsel.Shared.Constants;
using Fsel.Shared.Models.ShareModels;
using MassTransit;
using Microsoft.AspNetCore.SignalR;

namespace Fsel.Realtime.Application.Queues.Consumers
{
    public class AIFeedBackConsumer : BaseConsumer<SubmitAIResponseModel>
    {
        private readonly IHubContext<ClassForumAIFeedBackHub> _classForumFeedBackHubContext;
        private readonly IQueueProvider _queueProvider;

        public AIFeedBackConsumer(IHubContext<ClassForumAIFeedBackHub> classForumAIFeedBackHubContext, IQueueProvider queueProvider, AuthContext authContext, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _classForumFeedBackHubContext = classForumAIFeedBackHubContext;
            _queueProvider = queueProvider;
        }

        public override async Task ConsumeQueue(SubmitAIResponseModel? message)
        {
            if (message != null)
            {
                var classForumResultId = message.ClassForumResultId.ToString();
                await _classForumFeedBackHubContext.GetGroup(classForumResultId!).SendAsync(RealtimeSettings.ClassForumAIFeedBackHub.Methods.ClassForumResultFeedBack, message);
            }
        }
    }
}
