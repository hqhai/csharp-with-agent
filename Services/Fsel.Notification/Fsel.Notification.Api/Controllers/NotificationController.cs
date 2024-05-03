// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Notification.Api.Controllers
{
    using Fsel.Common.ActionResults;
    using System.Net;
    using Fsel.Common.Constants;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using Fsel.Notification.Domain.Model.EntityModels;
    using Fsel.Notification.Application.Queries;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Notification.Application.Commands;
    using Asp.Versioning;
    using Fsel.Shared.Constants;
    using System.Text;

    [ApiVersion(ApiSettings.APIVersion1)][ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/notifications")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly HttpClient _httpClient;

        public NotificationController(IMediator mediator, HttpClient httpClient)
        {
            _mediator = mediator;
            _httpClient = httpClient;
        }

        /// <summary>
        /// Get Notification
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<PagingItemsModel<NotificationMessageModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetList([FromQuery] GetListNotificationQuery query)
        {
            MethodResult<PagingItemsModel<NotificationMessageModel>> commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Push Notification
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<NotificationMessageModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreateNotification([FromBody] CreateNotificationCommand cmd)
        {
            MethodResult<NotificationMessageModel> commandResult = await _mediator.Send(cmd).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Turn Off/On Notification
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        [HttpPost("remind-status")]
        [ProducesResponseType(typeof(MethodResult<NotificationRemindModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreateNotificationRemind([FromBody] CreateNotificationRemindCommand cmd)
        {
            MethodResult<NotificationRemindModel> commandResult = await _mediator.Send(cmd).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Turn Off/On Notification
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        [HttpPost("remind-by-status")]
        [ProducesResponseType(typeof(MethodResult<IList<NotificationRemindModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetListNotificationRemind([FromBody] GetListNotificationRemindQuery query)
        {
            MethodResult<IList<NotificationRemindModel>> commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Mark Read Notification
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        [HttpPut("status")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateStatusNotificationMessage([FromBody] UpdateStatusNotificationCommand cmd)
        {
            MethodResult<bool> commandResult = await _mediator.Send(cmd).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Get Transcription
        /// </summary>
        [HttpPost("text-to-speech")]
        public async Task<IActionResult> TextToSpeech([FromBody] TextRequestModel request)
        {
            try
            {
                // Define your JSON object
                string json = "{\"text\": \"" + request.Text + "\"}";

                // URL to which you want to send the request
                string url = "https://api.deepgram.com/v1/speak"; // Replace with your actual endpoint URL

                // API Key
                string apiKey = "YOUR_DEEPGRAM_API_KEY"; // Replace with your actual API key

                // Prepare the HTTP request content
                HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");

                // Add Authorization header
                _httpClient.DefaultRequestHeaders.Add("Authorization", "token " + "ce119c53119855d5cb0d45b90eadecd79b5b8bff");

                // Send the POST request
                HttpResponseMessage response = await _httpClient.PostAsync(url, content);

                // Check if the request was successful
                if (response.IsSuccessStatusCode)
                {
                    // Read and save the response as binary data
                    using (Stream audioStream = await response.Content.ReadAsStreamAsync())
                    {
                        // Specify where you want to save the audio file
                        string filePath = Path.Combine(Directory.GetCurrentDirectory(), "output_audio.mp3");
                        using (FileStream fileStream = System.IO.File.Create(filePath))
                        {
                            using (BinaryWriter writer = new BinaryWriter(fileStream))
                            {
                                // Copy the binary data from the response stream to the file stream
                                byte[] buffer = new byte[8192];
                                int bytesRead;
                                while ((bytesRead = await audioStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                                {
                                    writer.Write(buffer, 0, bytesRead);
                                }
                            }
                        }
                        return PhysicalFile(filePath, "audio/mpeg", "output_audio.mp3");
                    }
                }
                else
                {
                    return StatusCode((int)response.StatusCode, "Request failed with status code: " + response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, "Error: " + ex.Message);
            }
        }



    }

    public class TextRequestModel
    {
        public string? Text { get; set; }
    }
}
