// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Application.Workers
{
    using Fsel.Core.Base;
    using Fsel.Hangfire.Application.Queues.Publishers;
    using Microsoft.AspNetCore.Http;

    public class AggregateDataWeeklyReportWorker : BaseWorker
    {
        private readonly AggregateDataWeeklyReportPublisher _aggregateDataWeeklyReportPublisher;

        public AggregateDataWeeklyReportWorker(AggregateDataWeeklyReportPublisher aggregateDataWeeklyReportPublisher, AuthContext authContext, IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _aggregateDataWeeklyReportPublisher = aggregateDataWeeklyReportPublisher;
        }

        public override async Task RunAsync()
        {
            await _aggregateDataWeeklyReportPublisher.Publish(CancellationToken.None);
        }
    }
}
