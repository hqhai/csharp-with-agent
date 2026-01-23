// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Realtime.Application.Services.SpeechToText
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Realtime.Domain.SpeechToTextModel;
    using Microsoft.AspNetCore.SignalR;
    using Microsoft.CognitiveServices.Speech;
    using Microsoft.CognitiveServices.Speech.Audio;

    /// <summary>
    /// Azure-specific implementation of ISpeechRecognitionSession
    /// Encapsulates Azure Speech SDK details
    /// </summary>
    public class AzureSpeechRecognitionSession : ISpeechRecognitionSession
    {
        private readonly SpeechRecognizer _recognizer;
        private readonly AudioConfig _audioConfig;
        private readonly PushAudioInputStream _pushStream;
        private readonly ISpeechRecognitionEventHandler _eventHandler;
        private readonly IHubContext<Hubs.SpeechToTextHub> _hubContext;
        private readonly List<string> _recognitionResults;
        private bool _isActive;

        public string SessionId { get; }
        public string ConnectionId { get; }
        public DateTime StartTime { get; }
        public bool IsActive => _isActive;

        public AzureSpeechRecognitionSession(
            string connectionId,
            string sessionId,
            SpeechRecognizer recognizer,
            AudioConfig audioConfig,
            PushAudioInputStream pushStream,
            ISpeechRecognitionEventHandler eventHandler,
            IHubContext<Hubs.SpeechToTextHub> hubContext)
        {
            ConnectionId = connectionId;
            SessionId = sessionId;
            _recognizer = recognizer;
            _audioConfig = audioConfig;
            _pushStream = pushStream;
            _eventHandler = eventHandler;
            _hubContext = hubContext;
            _recognitionResults = new List<string>();
            StartTime = DateTime.UtcNow;
            _isActive = false;

            SetupEventHandlers();
        }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            await _recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);
            _isActive = true;
        }

        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            await _recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);
            _isActive = false;
        }

        public Task WriteAudioAsync(byte[] audioData, CancellationToken cancellationToken = default)
        {
            _pushStream.Write(audioData, audioData.Length);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            if (_isActive)
            {
                try
                {
                    _recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }

            _pushStream?.Close();
            _recognizer?.Dispose();
            (_audioConfig as IDisposable)?.Dispose();
        }

        private void SetupEventHandlers()
        {
            _recognizer.Recognizing += (s, e) =>
            {
                if (e.Result.Reason == ResultReason.RecognizingSpeech)
                {
                    _ = _eventHandler.OnRecognizingAsync(ConnectionId, e.Result.Text);
                }
            };

            _recognizer.Recognized += (s, e) =>
            {
                if (e.Result.Reason == ResultReason.RecognizedSpeech)
                {
                    _recognitionResults.Add(e.Result.Text);
                    _ = _eventHandler.OnRecognizedAsync(ConnectionId, e.Result.Text);
                }
            };

            _recognizer.Canceled += (s, e) =>
            {
                if (e.Reason == CancellationReason.Error)
                {
                    _ = _eventHandler.OnErrorAsync(ConnectionId, e.ErrorCode.ToString(), e.ErrorDetails);
                }
            };

            _recognizer.SessionStarted += (s, e) =>
            {
                _ = _eventHandler.OnSessionStartedAsync(ConnectionId, SessionId);
            };

            _recognizer.SessionStopped += (s, e) =>
            {
                var fullText = string.Join(" ", _recognitionResults);
                _ = _eventHandler.OnSessionStoppedAsync(ConnectionId, SessionId, fullText, _recognitionResults.Count);
            };
        }
    }
}
