// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Realtime.Application.Hubs
{
    using System.Collections.Concurrent;

    public class ConnectionTracker
    {
        private static readonly Lazy<ConnectionTracker> s_instance = new Lazy<ConnectionTracker>(() => new ConnectionTracker());

        public static ConnectionTracker Instance => s_instance.Value;

        private readonly ConcurrentDictionary<string, DateTime> _connectionTimes;
        private readonly ConcurrentDictionary<string, string> _connectionUsers;

        private ConnectionTracker()
        {
            _connectionTimes = new ConcurrentDictionary<string, DateTime>();
            _connectionUsers = new ConcurrentDictionary<string, string>();
        }

        public void RecordConnectionStart(string connectionId)
        {
            _connectionTimes.TryAdd(connectionId, DateTime.UtcNow);
        }

        public void RecordConnectionStartUser(string connectionId, string userId)
        {
            _connectionUsers.TryAdd(userId, connectionId);
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

        public string? RecordConnectionEndUser(string userId)
        {
            string? connectionId;
            if (_connectionUsers.TryRemove(userId, out connectionId))
            {
                return connectionId;
            }
            return null;
        }
    }
}
