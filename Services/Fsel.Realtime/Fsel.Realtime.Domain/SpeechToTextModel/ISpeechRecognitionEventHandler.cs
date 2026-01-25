// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Realtime.Domain.SpeechToTextModel
{
    using System.Threading.Tasks;

    /// <summary>
    /// Interface for handling speech recognition events
    /// Decouples event handling from the speech service
    /// </summary>
    public interface ISpeechRecognitionEventHandler
    {
        /// <summary>
        /// Handle speech recognizing event (intermediate result)
        /// </summary>
        Task OnRecognizingAsync(string connectionId, string text);

        /// <summary>
        /// Handle speech recognized event (final result)
        /// </summary>
        Task OnRecognizedAsync(string connectionId, string text);

        /// <summary>
        /// Handle session started event
        /// </summary>
        Task OnSessionStartedAsync(string connectionId, string sessionId);

        /// <summary>
        /// Handle session stopped event
        /// </summary>
        Task OnSessionStoppedAsync(string connectionId, string sessionId, string fullText, int segmentCount);

        /// <summary>
        /// Handle error event
        /// </summary>
        Task OnErrorAsync(string connectionId, string errorCode, string errorDetails);

        /// <summary>
        /// Handle audio saved event
        /// </summary>
        Task OnAudioSavedAsync(string connectionId, string sessionId, string audioUrl);
    }
}
