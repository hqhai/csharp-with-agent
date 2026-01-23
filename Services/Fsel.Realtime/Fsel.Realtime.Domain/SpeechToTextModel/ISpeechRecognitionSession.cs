// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Realtime.Domain.SpeechToTextModel
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Interface for a speech recognition session
    /// Handles audio processing and event management
    /// </summary>
    public interface ISpeechRecognitionSession : IDisposable
    {
        /// <summary>
        /// Unique session identifier
        /// </summary>
        string SessionId { get; }

        /// <summary>
        /// Connection ID associated with this session
        /// </summary>
        string ConnectionId { get; }

        /// <summary>
        /// Session start time
        /// </summary>
        DateTime StartTime { get; }

        /// <summary>
        /// Whether the session is currently active
        /// </summary>
        bool IsActive { get; }

        /// <summary>
        /// Start the recognition session
        /// </summary>
        Task StartAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Stop the recognition session
        /// </summary>
        Task StopAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Write audio data to the session
        /// </summary>
        Task WriteAudioAsync(byte[] audioData, CancellationToken cancellationToken = default);
    }
}
