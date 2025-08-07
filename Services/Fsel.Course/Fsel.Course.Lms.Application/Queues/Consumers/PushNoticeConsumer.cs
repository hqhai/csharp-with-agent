namespace Fsel.Course.Lms.Application.Queues.Consumers
{
    using Fsel.Core.Base;
    using Fsel.Course.Lms.Application.Commands.OtherCmd;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.Extensions.Logging;

    public class PushNoticeConsumer : BaseConsumer<PushNoticeQueueModel>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<PushNoticeConsumer> _logger;

        public PushNoticeConsumer(IMediator mediator, AuthContext authContext, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor, ILogger<PushNoticeConsumer> logger) : base(authContext, httpContextAccessor)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public override async Task ConsumeQueue(PushNoticeQueueModel? message)
        {
            if (message == null)
            {
                _logger.LogError($"Message_PushNoticeConsumer_Null");
                return;
            }
            _logger.LogError($"Start_PushNoticeCommand_{message.TimeNotifyType}");
            await _mediator.Send(new PushNoticeCommand { TimeNotifyType = message.TimeNotifyType }).ConfigureAwait(false);
        }
    }
}
