// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Realtime.Domain.SpeechToTextModel
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Interface for speech recognition service
    /// Abstraction for any speech recognition provider (Azure, Google, etc.)
    /// </summary>
    public interface ISpeechRecognitionService
    {
        /// <summary>
        /// Create a new speech recognition session
        /// </summary>
        Task<ISpeechRecognitionSession> CreateSessionAsync(
            string connectionId,
            string? language = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get an existing session by connection ID
        /// </summary>
        ISpeechRecognitionSession? GetSession(string connectionId);

        /// <summary>
        /// Remove/terminate a session
        /// </summary>
        Task RemoveSessionAsync(string connectionId, CancellationToken cancellationToken = default);
    }
}
