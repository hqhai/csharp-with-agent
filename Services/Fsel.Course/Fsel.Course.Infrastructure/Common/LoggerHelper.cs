// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Common
{
    using Fsel.Common.Helpers;
    using Microsoft.Extensions.Logging;

    public class LoggerHelper
    {
        private readonly ILogger<object> _logger;

        public LoggerHelper(ILogger<object> logger)
        {
            _logger = logger;
        }

        public void LoggerRequest(object request)
        {
            var requestInfo = new
            {
                Timestamp = DateTimeOffset.UtcNow.ToString("o"),
                Request = request
            };
            _logger.LogError(ConvertHelper.Serialize(requestInfo));
        }
    }
}
