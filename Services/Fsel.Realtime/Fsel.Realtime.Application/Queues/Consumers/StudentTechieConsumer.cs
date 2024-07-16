using Fsel.Core.Base;
using Fsel.Core.Extensions;
using Fsel.Realtime.Application.Hubs;
using Fsel.Shared.Constants;
using Fsel.Shared.Models.ShareModels;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Fsel.Realtime.Application.Queues.Consumers
{
    public class StudentTechieConsumer : BaseConsumer<StudentTechieMessageModel>
    {
        private readonly IHubContext<TechieHub> _techieHub;
        private readonly AuthContext _authContext;
        private readonly ILogger<object> _logger;

        public StudentTechieConsumer(IHubContext<TechieHub> techieHub, AuthContext authContext, ILogger<object> logger, IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _techieHub = techieHub;
            _authContext = authContext;
            _logger = logger;
        }

        public override async Task ConsumeQueue(StudentTechieMessageModel? message)
        {
            _logger.LogInformation($"Consumer Receive Techie Message:{message.Message}");
            if (message != null)
            {
                await _techieHub.GetGroup(_authContext.CurrentUserId.ToString()).SendAsync(RealtimeSettings.TechieHub.Methods.Techie, message);
                _logger.LogInformation($"Send TechieHub Successfully !:{message.Message}");
            }
        }
    }
}
