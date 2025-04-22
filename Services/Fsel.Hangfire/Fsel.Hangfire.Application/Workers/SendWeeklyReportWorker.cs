// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Application.Workers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Hangfire.Application.Queues.Publishers;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    public class SendWeeklyReportWorker : IWorker
    {
        //private readonly SendWeeklyReportPublisher _sendWeeklyReportPublisher;
        private readonly IWebHostEnvironment _webHostEnvironment;

        private readonly ILogger<SendWeeklyReportWorker> _logger;

        public SendWeeklyReportWorker(/*SendWeeklyReportPublisher sendWeeklyReportPublisher, */IWebHostEnvironment webHostEnvironment, ILogger<SendWeeklyReportWorker> logger)
        {
            //_sendWeeklyReportPublisher = sendWeeklyReportPublisher;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        public async Task RunAsync()
        {
            if (_webHostEnvironment.IsProduction() || _webHostEnvironment.IsStaging())
            {
                _logger.LogWarning($"Call WeeklyReportWorker - {DateTime.UtcNow}");
                _logger.LogWarning($"Call WeeklyReportWorker, Disable WeeklyReportPublisher - {DateTime.UtcNow}");
                //await _sendWeeklyReportPublisher.Publish(CancellationToken.None);
            }
        }
    }
}
