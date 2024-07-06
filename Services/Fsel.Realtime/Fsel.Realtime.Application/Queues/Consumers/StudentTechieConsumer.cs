using Fsel.Core.Base;
using Fsel.Core.Extensions;
using Fsel.Realtime.Application.Hubs;
using Fsel.Shared.Constants;
using Fsel.Shared.Models.ShareModels;
using MassTransit;
using Microsoft.AspNetCore.SignalR;

namespace Fsel.Realtime.Application.Queues.Consumers
{
    public class StudentTechieConsumer : BaseConsumer<StudentTechieMessageModel>
    {
        private readonly IHubContext<TechieHub> _techieHub;

        public StudentTechieConsumer(IHubContext<TechieHub> techieHub, AuthContext authContext) : base(authContext)
        {
            _techieHub = techieHub;
        }

        public override async Task ConsumeQueue(StudentTechieMessageModel? message)
        {
            if (message != null)
            {
                var mockTestResultId = message.StudentId.ToString();
                await _techieHub.GetGroup(mockTestResultId!).SendAsync(RealtimeSettings.TechieHub.Methods.Techie, message);
            }
        }
    }
}
