// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Application.Workers
{
    using Fsel.Core.Base.Interfaces;
    using Fsel.Hangfire.Application.Queues.Publishers;

    public class AggregateDataWeeklyReportWorker : IWorker
    {
        private readonly AggregateDataWeeklyReportPublisher _aggregateDataWeeklyReportPublisher;

        public AggregateDataWeeklyReportWorker(AggregateDataWeeklyReportPublisher aggregateDataWeeklyReportPublisher)
        {
            _aggregateDataWeeklyReportPublisher = aggregateDataWeeklyReportPublisher;
        }

        public async Task RunAsync()
        {
            await _aggregateDataWeeklyReportPublisher.Publish(CancellationToken.None);
        }
    }
}
