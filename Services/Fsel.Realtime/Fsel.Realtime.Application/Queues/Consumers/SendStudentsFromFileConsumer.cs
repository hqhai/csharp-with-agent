// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Realtime.Application.Queues.Consumers
{
    using Fsel.Core.Base;
    using Fsel.Core.Extensions;
    using Fsel.Realtime.Application.Hubs;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.SignalR;
    using Microsoft.Extensions.Logging;

    public class SendStudentsFromFileConsumer : BaseConsumer<CreateStudentsToEventFromFileModel>
    {
        private readonly IHubContext<SendStudentsFromFileHub> _sendStudentsFromFileHub;
        private readonly AuthContext _authContext;
        private readonly ILogger<object> _logger;

        public SendStudentsFromFileConsumer(IHubContext<SendStudentsFromFileHub> sendStudentsFromFileHub, AuthContext authContext, ILogger<object> logger, IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _sendStudentsFromFileHub = sendStudentsFromFileHub;
            _authContext = authContext;
            _logger = logger;
        }

        public override async Task ConsumeQueue(CreateStudentsToEventFromFileModel? message)
        {
            _logger.LogInformation($"Consumer SendStudentsFromFile Message: {message.Message}");
            if (message != null && !string.IsNullOrEmpty(message.Key))
            {
                await _sendStudentsFromFileHub.GetGroup(message.Key).SendAsync(RealtimeSettings.SendStudentsFromFileHub.Methods.SendStudentsFromFile, message);
                _logger.LogInformation($"Send SendStudentsFromFile Successfully !: {message.Message}");
            }
        }
    }
}
