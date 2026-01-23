// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Realtime.Application.Services.SpeechToText
{
    using System;
    using System.Threading.Tasks;
    using Fsel.Realtime.Domain.SpeechToTextModel;
    using Fsel.Shared.Constants;
    using Microsoft.AspNetCore.SignalR;

    /// <summary>
    /// SignalR-based implementation of ISpeechRecognitionEventHandler
    /// Notifies clients via SignalR
    /// </summary>
    public class SpeechRecognitionEventNotifier : ISpeechRecognitionEventHandler
    {
        private readonly IHubContext<Hubs.SpeechToTextHub> _hubContext;

        public SpeechRecognitionEventNotifier(IHubContext<Hubs.SpeechToTextHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public Task OnRecognizingAsync(string connectionId, string text)
        {
            return _hubContext.Clients.Client(connectionId).SendAsync(
                RealtimeSettings.SpeechToTextHub.Methods.OnRecognizing,
                new { text = text, isFinal = false });
        }

        public Task OnRecognizedAsync(string connectionId, string text)
        {
            return _hubContext.Clients.Client(connectionId).SendAsync(
                RealtimeSettings.SpeechToTextHub.Methods.OnRecognized,
                new { text = text, isFinal = true });
        }

        public Task OnSessionStartedAsync(string connectionId, string sessionId)
        {
            return _hubContext.Clients.Client(connectionId).SendAsync(
                RealtimeSettings.SpeechToTextHub.Methods.OnSessionStarted,
                new { sessionId = sessionId });
        }

        public Task OnSessionStoppedAsync(string connectionId, string sessionId, string fullText, int segmentCount)
        {
            return _hubContext.Clients.Client(connectionId).SendAsync(
                RealtimeSettings.SpeechToTextHub.Methods.OnSessionStopped,
                new { sessionId = sessionId, fullText = fullText, segmentCount = segmentCount });
        }

        public Task OnErrorAsync(string connectionId, string errorCode, string errorDetails)
        {
            return _hubContext.Clients.Client(connectionId).SendAsync(
                RealtimeSettings.SpeechToTextHub.Methods.OnError,
                new { errorCode = errorCode, errorDetails = errorDetails });
        }
    }
}
