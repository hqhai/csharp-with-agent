// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Realtime.Application.Hubs
{
    using System.Collections.Concurrent;

    public class ConnectionTracker
    {
        private static readonly Lazy<ConnectionTracker> s_instance = new Lazy<ConnectionTracker>(() => new ConnectionTracker());

        public static ConnectionTracker Instance => s_instance.Value;

        private readonly ConcurrentDictionary<string, DateTime> _connectionTimes;

        private ConnectionTracker()
        {
            _connectionTimes = new ConcurrentDictionary<string, DateTime>();
        }

        public void RecordConnectionStart(string connectionId)
        {
            _connectionTimes.TryAdd(connectionId, DateTime.UtcNow);
        }

        public long? RecordConnectionEnd(string connectionId)
        {
            DateTime startTime;
            if (_connectionTimes.TryRemove(connectionId, out startTime))
            {
                TimeSpan duration = DateTime.UtcNow - startTime;
                return (long)duration.TotalSeconds;
            }

            return null;
        }

        public long? GetTimeValue(string connectionId)
        {
            DateTime startTime;
            if (_connectionTimes.TryGetValue(connectionId, out startTime))
            {
                TimeSpan duration = DateTime.UtcNow - startTime;
                return (long)duration.TotalSeconds;
            }

            return null;
        }
    }
}
