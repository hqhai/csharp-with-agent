// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Realtime.Application.Services.SpeechToText
{
    using System;
    using System.Collections.Generic;
    using System.IO;
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
        private readonly List<byte[]> _audioChunks;
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
            _audioChunks = new List<byte[]>();
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
            // Write to Azure push stream for recognition
            _pushStream.Write(audioData, audioData.Length);

            // Collect audio data for WAV file creation
            _audioChunks.Add(audioData);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Gets all collected audio data as a combined byte array
        /// </summary>
        public byte[] GetAudioData()
        {
            if (_audioChunks.Count == 0)
                return Array.Empty<byte>();

            var totalLength = 0;
            foreach (var chunk in _audioChunks)
            {
                totalLength += chunk.Length;
            }

            var result = new byte[totalLength];
            var offset = 0;

            foreach (var chunk in _audioChunks)
            {
                Buffer.BlockCopy(chunk, 0, result, offset, chunk.Length);
                offset += chunk.Length;
            }

            return result;
        }

        /// <summary>
        /// Creates a WAV file from collected audio data
        /// </summary>
        /// <param name="pcmData">Raw PCM audio data</param>
        /// <param name="sampleRate">Sample rate (default: 16000)</param>
        /// <param name="channels">Number of channels (default: 1)</param>
        /// <param name="bitsPerSample">Bits per sample (default: 16)</param>
        /// <returns>WAV file as byte array</returns>
        public static byte[] CreateWavFile(byte[] pcmData, int sampleRate = 16000, int channels = 1, int bitsPerSample = 16)
        {
            var byteRate = sampleRate * channels * bitsPerSample / 8;
            var blockAlign = channels * bitsPerSample / 8;
            var dataSize = pcmData.Length;
            var fileSize = 36 + dataSize;

            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);

            // RIFF header
            writer.Write(new char[] { 'R', 'I', 'F', 'F' });
            writer.Write(fileSize);
            writer.Write(new char[] { 'W', 'A', 'V', 'E' });

            // fmt sub-chunk
            writer.Write(new char[] { 'f', 'm', 't', ' ' });
            writer.Write(16); // Subchunk1Size (16 for PCM)
            writer.Write((short)1); // AudioFormat (1 for PCM)
            writer.Write((short)channels);
            writer.Write(sampleRate);
            writer.Write(byteRate);
            writer.Write((short)blockAlign);
            writer.Write((short)bitsPerSample);

            // data sub-chunk
            writer.Write(new char[] { 'd', 'a', 't', 'a' });
            writer.Write(dataSize);
            writer.Write(pcmData);

            return ms.ToArray();
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
