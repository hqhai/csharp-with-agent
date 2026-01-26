// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Realtime.Application.Hubs
{
    using System;
    using System.Threading.Tasks;
    using Fsel.Core.Base;
    using Fsel.Core.Extensions;
    using Fsel.Core.Services.IpApiServices;
    using Fsel.Realtime.Application.Trackers;
    using Fsel.Realtime.Domain.SpeechToTextModel;
    using Fsel.Shared.Constants;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.SignalR;

    /// <summary>
    /// SignalR Hub for Realtime Speech-to-Text
    /// Refactored to follow SOLID principles:
    /// - SRP: Only handles SignalR communication, delegates speech logic to ISpeechRecognitionService
    /// - OCP: Open for extension (can add new providers) without modification
    /// - LSP: Works with any ISpeechRecognitionService implementation
    /// - ISP: Depends only on ISpeechRecognitionService interface
    /// - DIP: Depends on abstraction (ISpeechRecognitionService), not concrete implementation
    /// </summary>
    public class SpeechToTextHub : BaseHub
    {
        private readonly AuthContext _authContext;
        private readonly ISpeechRecognitionService _speechRecognitionService;

        public SpeechToTextHub(
            AuthContext authContext,
            IIpApiService ipApiService,
            IHttpContextAccessor httpContextAccessor,
            ISpeechRecognitionService speechRecognitionService)
            : base(authContext, ipApiService, httpContextAccessor)
        {
            _authContext = authContext;
            _speechRecognitionService = speechRecognitionService;
        }

        public override async Task OnConnectedHubAsync()
        {
            await Groups.AddGroupAsync(Context.ConnectionId, _authContext.CurrentUserId.ToString());
            ConnectionTracker.Instance.RecordConnectionStart(Context.ConnectionId);
        }

        public override async Task OnDisconnectedHubAsync(Exception? exception)
        {
            await Groups.RemoveGroupAsync(Context.ConnectionId, _authContext.CurrentUserId.ToString());

            // Cleanup recognizer session if exists
            var connectionId = Context.ConnectionId;
            await _speechRecognitionService.RemoveSessionAsync(connectionId);
        }

        /// <summary>
        /// Client calls this to start a new recognition session
        /// </summary>
        public async Task StartRecognition(string? language = null)
        {
            var connectionId = Context.ConnectionId;

            try
            {
                var session = await _speechRecognitionService.CreateSessionAsync(connectionId, language);

                await Clients.Caller.SendAsync(
                    RealtimeSettings.SpeechToTextHub.Methods.OnStarted,
                    new { sessionId = session.SessionId, message = "Recognition started." });
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync(
                    RealtimeSettings.SpeechToTextHub.Methods.OnError,
                    new { errorCode = "START_ERROR", errorDetails = ex.Message });
            }
        }

        /// <summary>
        /// Client calls this to send audio chunks (base64 encoded)
        /// </summary>
        public async Task SendAudioChunk(string audioDataBase64)
        {
            var connectionId = Context.ConnectionId;

            try
            {
                var session = _speechRecognitionService.GetSession(connectionId);
                if (session == null)
                {
                    return;
                }

                byte[] audioBytes = Convert.FromBase64String(audioDataBase64);
                await session.WriteAudioAsync(audioBytes);
            }
            catch
            {
                // Ignore - errors will be handled via event handler
            }
        }

        /// <summary>
        /// Client calls this to stop recognition
        /// </summary>
        public async Task StopRecognition()
        {
            var connectionId = Context.ConnectionId;

            try
            {
                var session = _speechRecognitionService.GetSession(connectionId);
                if (session == null)
                {
                    return;
                }

                await _speechRecognitionService.RemoveSessionAsync(connectionId);

                var duration = (DateTime.UtcNow - session.StartTime).TotalSeconds;

                await Clients.Caller.SendAsync(
                    RealtimeSettings.SpeechToTextHub.Methods.OnStopped,
                    new { sessionId = session.SessionId, duration = duration });
            }
            catch
            {
                // Ignore - errors will be handled via event handler
            }
        }
    }
}
