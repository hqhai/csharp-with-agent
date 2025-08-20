// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Api.Controllers
{
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Attributes;
    using Fsel.Common.Constants;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Constants;
    using Fsel.System.Application.Commands.Chatbots;
    using Fsel.System.Application.Queries.ChabotQuery;
    using Fsel.System.Application.Queries.ChatbotConfigQuery;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using global::System.Net;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/chat-bot-config")]
    [ApiController]
    public class ChatbotConfigsController : BaseController
    {
        private readonly IMediator _mediator;
        private readonly ISchoolRepository _schoolRepository;

        public ChatbotConfigsController(IMediator mediator, ISchoolRepository schoolRepository)
        {
            _mediator = mediator;
            _schoolRepository = schoolRepository;
        }

        /// <summary>
        /// Execute-list-query
        /// </summary>
        [HttpPost("execute-list-query")]
        [ProducesResponseType(typeof(MethodResult<IList<SchoolModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        [Permission]
        public async Task<IActionResult> ExecuteList([FromBody] BaseQueryModel cmd)
        {
            SetQuery(cmd);
            var result = await _schoolRepository.GetListResultAsync<SchoolModel>(cmd);
            return result.GetActionResult();
        }

        /// <summary>
        /// Lưu ChatbotConfig
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<ChatbotConfigModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SaveChatBotConfig([FromBody] SaveChatbotConfigCommand cmd)
        {
            MethodResult<ChatbotConfigModel> commandResult = await _mediator.Send(cmd).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        [HttpPost("message")]
        [ProducesResponseType(typeof(MethodResult<ChatBotModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SaveChatBotMessage([FromBody] SaveChatBotMessageCommand cmd)
        {
            MethodResult<ChatBotModel> commandResult = await _mediator.Send(cmd).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Lưu ChatbotConfig
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        [HttpPost("init-chat-bot")]
        [ProducesResponseType(typeof(MethodResult<ChatBotModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> InitChatBotByUnitAndSkill([FromBody] InitChatBotRoomCommand cmd)
        {
            MethodResult<ChatBotModel> commandResult = await _mediator.Send(cmd).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Lấy chatbotconfig theo unitid
        /// </summary>
        /// <param name="unitId"></param>
        /// <returns></returns>
        [HttpGet("{unitId}")]
        [ProducesResponseType(typeof(MethodResult<ChatbotConfigModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetChatBotConfig([FromRoute] Guid unitId)
        {
            MethodResult<ChatbotConfigModel> commandResult = await _mediator.Send(new GetChatBotConfigQuery { UnitId = unitId }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// Lấy chatbotconfig theo unitid
        /// </summary>
        /// <param name="unitId"></param>
        /// <returns></returns>
        [HttpPost("unit-chatbot-configs")]
        [ProducesResponseType(typeof(MethodResult<IList<ChatbotConfigModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetChatbotConfigsByUserIds([FromBody] GetUnitChatBotConfigsQuery query)
        {
            MethodResult<IList<ChatbotConfigModel>> commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// lấy chatbot theo chatbotid
        /// </summary>
        /// <param name="unitId"></param>
        /// <returns></returns>
        [HttpGet("list-chat-bot")]
        [ProducesResponseType(typeof(MethodResult<IList<ChatBotModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetChatBotModel([FromQuery] GetChatBotQuery query)
        {
            MethodResult<IList<ChatBotModel>> commandResult = await _mediator.Send(query).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        /// <summary>
        /// lấy chatbot theo chatbotid
        /// </summary>
        /// <param name="unitId"></param>
        /// <returns></returns>
        [HttpGet("chat-bot-message/{chatbotId}")]
        [ProducesResponseType(typeof(MethodResult<ChatBotModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetChatBotById([FromRoute] Guid chatbotId)
        {
            MethodResult<ChatBotModel> commandResult = await _mediator.Send(new GetChatBotByIdQuery { ChatbotId = chatbotId }).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }

        [HttpPost("convert-file")]
        [ProducesResponseType(typeof(MethodResult<string>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ConvertFile([FromQuery] ConvertFileWavCommand command)
        {
            MethodResult<string> commandResult = await _mediator.Send(command).ConfigureAwait(false);
            return commandResult.GetActionResult();
        }
    }
}
