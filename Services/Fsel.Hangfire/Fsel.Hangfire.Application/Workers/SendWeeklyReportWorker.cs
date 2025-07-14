// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Application.Workers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.AspNetCore.Http;

    public class SendWeeklyReportWorker : BaseWorker
    {
        //private readonly SendWeeklyReportPublisher _sendWeeklyReportPublisher;
        private readonly IWebHostEnvironment _webHostEnvironment;

        private readonly ILogger<SendWeeklyReportWorker> _logger;

        public SendWeeklyReportWorker(/*SendWeeklyReportPublisher sendWeeklyReportPublisher, */IWebHostEnvironment webHostEnvironment, ILogger<SendWeeklyReportWorker> logger, AuthContext authContext, IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            //_sendWeeklyReportPublisher = sendWeeklyReportPublisher;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        public override async Task RunAsync()
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
