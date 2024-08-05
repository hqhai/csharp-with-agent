// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Realtime.Application.Trackers
{
    using System;
    using System.Collections.Concurrent;
    using Fsel.Shared.Models.ShareModels;

    public class ConnectionTracker
    {
        private static readonly Lazy<ConnectionTracker> s_instance = new Lazy<ConnectionTracker>(() => new ConnectionTracker());

        public static ConnectionTracker Instance => s_instance.Value;

        private readonly ConcurrentDictionary<string, ConnectionInfo<TrackingTimeModel>> _connectionTimes;
        private readonly ConcurrentDictionary<string, string> _connectionUsers;

        private ConnectionTracker()
        {
            _connectionTimes = new ConcurrentDictionary<string, ConnectionInfo<TrackingTimeModel>>();
            _connectionUsers = new ConcurrentDictionary<string, string>();
        }

        public void RecordConnectionStart(string connectionId, TrackingTimeModel? model = default)
        {
            var connectionInfo = new ConnectionInfo<TrackingTimeModel>
            {
                ConnectionTime = DateTime.UtcNow,
                Model = model,
            };


            if (!_connectionTimes.TryAdd(connectionId, connectionInfo))
            {
                // Nếu không thể thêm, cập nhật giá trị hiện tại với giá trị mới
                _connectionTimes.AddOrUpdate(connectionId, connectionInfo, (key, existingValue) =>
                {
                    existingValue.Model = model;
                    return existingValue;
                });
            }
        }

        public void RecordConnectionStartUser(string connectionId, string userId)
        {
            _connectionUsers.TryAdd(userId, connectionId);
        }

        public long? RecordConnectionEnd(string connectionId)
        {
            ConnectionInfo<TrackingTimeModel>? connectionInfo;
            if (_connectionTimes!.TryRemove(connectionId, out connectionInfo))
            {
                TimeSpan duration = DateTime.UtcNow - connectionInfo.ConnectionTime;
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

        public long? GetTimeValue(string connectionId)
        {
            ConnectionInfo<TrackingTimeModel> connectionInfo;
            if (_connectionTimes!.TryGetValue(connectionId, out connectionInfo!))
            {
                TimeSpan duration = DateTime.UtcNow - connectionInfo.ConnectionTime;
                return (long)duration.TotalSeconds;
            }
            return null;
        }

        public TrackingTimeModel GetModel(string connectionId)
        {
            ConnectionInfo<TrackingTimeModel> connectionInfo;
            if (_connectionTimes!.TryGetValue(connectionId, out connectionInfo!))
            {
                return connectionInfo.Model!;
            }
            return null;
        }
    }

    public class ConnectionInfo<T>
    {
        public DateTime ConnectionTime { get; set; }
        public T? Model { get; set; }
    }
}
