using Fsel.Core.Extensions;
using Fsel.Realtime.Application.Hubs;
using Fsel.Shared.Constants;
using Fsel.Shared.Models.ShareModels;
using MassTransit;
using Microsoft.AspNetCore.SignalR;

namespace Fsel.Realtime.Application.Queues.Consumers
{
    public class AIFeedBackConsumer : IConsumer<SubmitAIResponseModel>
    {
        private readonly IHubContext<ClassForumAIFeedBackHub> _classForumFeedBackHubContext;

        public AIFeedBackConsumer(IHubContext<ClassForumAIFeedBackHub> classForumAIFeedBackHubContext)
        {
            _classForumFeedBackHubContext = classForumAIFeedBackHubContext;
        }

        public async Task Consume(ConsumeContext<SubmitAIResponseModel> context)
        {
            if (context != null)
            {
                var classForumResultId = context.Message.ClassForumResultId.ToString();
                await _classForumFeedBackHubContext.GetGroup(classForumResultId!).SendAsync(RealtimeSettings.ClassForumAIFeedBackHub.Methods.ClassForumResultFeedBack, context.Message);
            }
        }
    }
}
