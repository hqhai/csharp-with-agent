// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Application.Workers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Hangfire.Application.Queues.Publishers;

    public class WeeklyReportWorker : IWorker
    {
        private readonly WeeklyReportPublisher _weeklyReportPublisher;

        public WeeklyReportWorker(WeeklyReportPublisher weeklyReportPublisher)
        {
            _weeklyReportPublisher = weeklyReportPublisher;
        }

        public async Task RunAsync()
        {
            await _weeklyReportPublisher.Publish(CancellationToken.None);
        }
    }
}
