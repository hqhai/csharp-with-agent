// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Application.Workers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Hangfire.Application.Queues.Publishers;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;

    public class EnergyOfPlanetWorker : IWorker
    {
        private readonly WeeklyNoticePublisher _weeklyNoticePublisher;

        public EnergyOfPlanetWorker(WeeklyNoticePublisher weeklyNoticePublisher)
        {
            _weeklyNoticePublisher = weeklyNoticePublisher;
        }

        public async Task RunAsync()
        {
            await _weeklyNoticePublisher.Publish(new WeeklyNoticeQueueModel { WeeklyNoticeType = EnumWeeklyNoticeType.EnergyOfPlanet }, CancellationToken.None);
        }
    }
}
