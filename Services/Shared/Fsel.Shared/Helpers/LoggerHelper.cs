// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Helpers
{
    using Fsel.Common.Helpers;
    using Microsoft.Extensions.Logging;

    public static class LoggerHelper
    {
        public static void LoggerRequest(this ILogger _logger, object request)
        {
            var requestInfo = new
            {
                Timestamp = DateTimeOffset.UtcNow.ToString("o"),
                Request = request
            };
            _logger.LogInformation(ConvertHelper.Serialize(requestInfo));
        }
    }
}
