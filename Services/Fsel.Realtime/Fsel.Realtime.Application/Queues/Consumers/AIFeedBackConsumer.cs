using Fsel.Core.Base.BaseModels;
using Fsel.Core.Base.Interfaces;
using Fsel.Core.Extensions;
using Fsel.Realtime.Application.Hubs;
using Fsel.Shared.Constants;
using Fsel.Shared.Models.ShareModels;
using MassTransit;
using Microsoft.AspNetCore.SignalR;

namespace Fsel.Realtime.Application.Queues.Consumers
{
    public class AIFeedBackConsumer : Core.Base.Interfaces.IBaseConsumer<SubmitAIResponseModel>
    {
        private readonly IHubContext<ClassForumAIFeedBackHub> _classForumFeedBackHubContext;
        private readonly IQueueProvider _queueProvider;

        public AIFeedBackConsumer(IHubContext<ClassForumAIFeedBackHub> classForumAIFeedBackHubContext, IQueueProvider queueProvider)
        {
            _classForumFeedBackHubContext = classForumAIFeedBackHubContext;
            _queueProvider = queueProvider;
        }

        public async Task Consume(ConsumeContext<BaseQueueDataModel<SubmitAIResponseModel>> context)
        {
            if (context != null)
            {
                var classForumResultId = context.Message.ClassForumDetailResultId.ToString();
                await _classForumFeedBackHubContext.GetGroup(classForumResultId!).SendAsync(RealtimeSettings.ClassForumAIFeedBackHub.Methods.ClassForumResultFeedBack, context.Message);

                try
                {
                    _queueProvider.Publish(RealtimeSettings.ClassForumAIFeedBackHub.Methods.ClassForumResultFeedBack, classForumResultId, context.Message);
                }
                catch { }
            }
        }
    }
}
