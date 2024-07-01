// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Application.Workers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Hangfire.Application.Queues.Publishers;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Hosting;

    public class WeeklyReportWorker : IWorker
    {
        private readonly WeeklyReportPublisher _weeklyReportPublisher;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public WeeklyReportWorker(WeeklyReportPublisher weeklyReportPublisher, IWebHostEnvironment webHostEnvironment)
        {
            _weeklyReportPublisher = weeklyReportPublisher;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task RunAsync()
        {
            if (_webHostEnvironment.IsProduction() || _webHostEnvironment.IsStaging())
            {
                await _weeklyReportPublisher.Publish(CancellationToken.None);
            }
        }
    }
}
