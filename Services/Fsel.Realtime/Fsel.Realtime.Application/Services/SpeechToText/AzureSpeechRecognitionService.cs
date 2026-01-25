// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Realtime.Application.Services.SpeechToText
{
    using System;
    using System.Collections.Concurrent;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Realtime.Application.ValueSettings;
    using Fsel.Realtime.Domain.SpeechToTextModel;
    using Fsel.Realtime.Infrastructure.Services;
    using Fsel.Shared.Enums;
    using Microsoft.AspNetCore.SignalR;
    using Microsoft.CognitiveServices.Speech;
    using Microsoft.CognitiveServices.Speech.Audio;
    using Refit;

    /// <summary>
    /// Azure Speech Service implementation of ISpeechRecognitionService
    /// Handles Azure-specific speech recognition logic
    /// </summary>
    public class AzureSpeechRecognitionService : ISpeechRecognitionService
    {
        private readonly ConcurrentDictionary<string, ISpeechRecognitionSession> _sessions;
        private readonly ISpeechRecognitionEventHandler _eventHandler;
        private readonly IHubContext<Hubs.SpeechToTextHub> _hubContext;
        private readonly AppSetting _appSetting;
        private readonly AudioStreamConfig _audioConfig;
        private readonly IStorageService _storageService;

        public AzureSpeechRecognitionService(
            ISpeechRecognitionEventHandler eventHandler,
            IHubContext<Hubs.SpeechToTextHub> hubContext,
            AppSetting appSetting,
            IStorageService storageApi,
            AudioStreamConfig? audioConfig = null)
        {
            _sessions = new ConcurrentDictionary<string, ISpeechRecognitionSession>();
            _eventHandler = eventHandler;
            _hubContext = hubContext;
            _appSetting = appSetting;
            _audioConfig = audioConfig ?? AudioStreamConfig.DefaultPcm;
            _storageService = storageApi;
        }

        public async Task<ISpeechRecognitionSession> CreateSessionAsync(
            string connectionId,
            string? language = null,
            CancellationToken cancellationToken = default)
        {
            var sessionId = Guid.NewGuid().ToString();

            // Get Azure Speech configuration
            var speechKey = _appSetting.AzureAiConfig?.SecondApiKey
                ?? _appSetting.AzureAiConfig?.FirstApiKey
                ?? throw new InvalidOperationException("Azure Speech API Key is not configured in appsettings.");
            var region = _appSetting.AzureAiConfig?.Location
                ?? throw new InvalidOperationException("Azure Speech Location is not configured in appsettings.");

            // Create push audio stream
            var pushStream = AudioInputStream.CreatePushStream(
                AudioStreamFormat.GetWaveFormatPCM((uint)_audioConfig.SampleRate, (byte)_audioConfig.BitsPerSample, (byte)_audioConfig.Channels));

            // Create audio config and speech config
            var audioConfig = AudioConfig.FromStreamInput(pushStream);
            var speechConfig = SpeechConfig.FromSubscription(speechKey, region);

            speechConfig.SpeechRecognitionLanguage = string.IsNullOrEmpty(language)
                ? (_appSetting.AzureAiConfig?.SpeechRecognitionLanguage ?? "ja-JP")
                : language;

            // Create recognizer
            var recognizer = new SpeechRecognizer(speechConfig, audioConfig);

            // Create session
            var session = new AzureSpeechRecognitionSession(
                connectionId,
                sessionId,
                recognizer,
                audioConfig,
                pushStream,
                _eventHandler,
                _hubContext);

            // Add to sessions dictionary
            _sessions[connectionId] = session;

            // Start recognition
            await session.StartAsync(cancellationToken);

            return session;
        }

        public ISpeechRecognitionSession? GetSession(string connectionId)
        {
            return _sessions.TryGetValue(connectionId, out var session) ? session : null;
        }

        public async Task RemoveSessionAsync(string connectionId, CancellationToken cancellationToken = default)
        {
            if (_sessions.TryRemove(connectionId, out var session))
            {
                await session.StopAsync(cancellationToken);

                // Save audio to storage and notify client
                if (session is AzureSpeechRecognitionSession azureSession)
                {
                    var audioData = azureSession.GetAudioData();
                    if (audioData.Length > 0)
                    {
                        try
                        {
                            // Create WAV file
                            var wavData = AzureSpeechRecognitionSession.CreateWavFile(
                                audioData,
                                _audioConfig.SampleRate,
                                _audioConfig.Channels,
                                _audioConfig.BitsPerSample);

                            // Generate file name
                            var fileName = $"speech_{azureSession.SessionId}_{DateTime.UtcNow:yyyyMMddHHmmss}.wav";

                            // Upload to S3 via Refit
                            using var stream = new MemoryStream(wavData);
                            var streamPart = new StreamPart(stream, fileName, "audio/wav");
                            var response = await _storageService.UploadFile(
                                EnumFolderType.Files,
                                EnumBucketType.FselPublic,
                                streamPart,
                                isResize: false,
                                isValidEmpty: false,
                                isAddSuffix: true);

                            var audioUrl = response?.Content?.Result;
                            if (!string.IsNullOrEmpty(audioUrl))
                            {
                                // Notify client with audio URL
                                await _eventHandler.OnAudioSavedAsync(connectionId, azureSession.SessionId, audioUrl);
                            }
                        }
                        catch (ApiException ex)
                        {
                            // Log error but don't fail the session cleanup
                            await _eventHandler.OnErrorAsync(connectionId, "AUDIO_UPLOAD_ERROR", $"Failed to upload audio: {ex.Message}");
                        }
                        catch (Exception ex)
                        {
                            await _eventHandler.OnErrorAsync(connectionId, "AUDIO_UPLOAD_ERROR", $"Failed to upload audio: {ex.Message}");
                        }
                    }
                }

                session.Dispose();
            }
        }
    }
}
