// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.Chatbots
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.System.Domain.Entities.Chatbots;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.CommandModels.ChatbotConfigs;
    using Fsel.System.Domain.Models.EntityModels;
    using global::System.Globalization;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class SaveChatbotConfigCommand : SaveChatbotConfigCommandModel, IRequest<MethodResult<ChatbotConfigModel>>
    {
    }

    public class SaveChatbotConfigCommandHandler : IRequestHandler<SaveChatbotConfigCommand, MethodResult<ChatbotConfigModel>>
    {
        private readonly IChatbotConfigRepository _chatbotConfigRepository;
        private readonly IMapper _mapper;

        private const string ProgramName = "Program Name";
        private const string CourseName = "Course Name";
        private const string CEFRLevel = "CEFR Level";
        private const string UnitTopic = "Unit Topic";
        private const string UnitNumber = "Unit Number";

        public SaveChatbotConfigCommandHandler(IChatbotConfigRepository chatbotConfigRepository, IMapper mapper)
        {
            _chatbotConfigRepository = chatbotConfigRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<ChatbotConfigModel>> Handle(SaveChatbotConfigCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<ChatbotConfigModel>();

            foreach (var item in request.ChatbotSkillConfigs!)
            {
                if(item.Configs.type)

                item.AiConfig += ConvertHelper.Serialize(item.Configs);
            }

            ChatbotConfig chatbotConfig = new ChatbotConfig();
            chatbotConfig = _mapper.Map<ChatbotConfig>(request);

            await _chatbotConfigRepository.ExecuteTransactionAsync(async () =>
            {
                _chatbotConfigRepository.Add(chatbotConfig);
                await _chatbotConfigRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<ChatbotConfigModel>(chatbotConfig);
                return methodResult;
            });
            return methodResult;
        }


        /// <summary>
        /// Convert về dạng chung
        /// </summary>
        /// <param name="skillConfigs"></param>
        /// <returns></returns>
        private static string ConvertToAiPrompt(string property, string value)
        {
            string result = "{\n" + ConvertToCamelCase(property) + ":" + value + "\n}";

            return result;
        }

        private static string BuildTemplatePrompt2(string skillType, SaveChatbotConfigCommand request)
        {

            string itemContentProgram = ConvertToAiPrompt(nameof(request.ProgramName), request.ProgramName!.ToString());
            string itemContentCourse = ConvertToAiPrompt(nameof(request.CourseName), request.CourseName!.ToString());
            string itemContentCEFRLevel = ConvertToAiPrompt(nameof(request.CEFRLevel), request.CEFRLevel!.ToString());
            string itemUnitTopic = ConvertToAiPrompt(nameof(request.UnitTopic), request.UnitTopic!.ToString());
            string itemUnitNumber = ConvertToAiPrompt(nameof(request.UnitNumber), request.UnitNumber!.ToString());


            string resultProgram = "Program Name:\n" + "[" + itemContentProgram + "]\n";
            string resultCourse = "Course Name:\n" + "[" + itemContentCourse + "]\n";
            string resultCEFRLevel = "Course CEFR Level:\n" + "[" + itemContentCEFRLevel + "]\n";
            string resultUnitTopic = "Unit Topic:\n" + "[" + itemUnitTopic + "]\n";
            string resultUnitNumber = "Unit Number:\n" + "[" + itemUnitNumber + "]\n";

            return resultProgram;
        }


        private static string BuildTemplatePrompt(string skillType, string customProp, string customValue)
        {
            if (skillType == CEFRLevel)
            {
                skillType = "Course " + skillType;
            }

            string item = ConvertToAiPrompt(customProp, customValue);
            return $"{skillType}:\n" + $"[{item}]\n";
        }




        /// <summary>
        /// chuyển text thành dạng camelcase
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static string ConvertToCamelCase(string input)
        {
            // Chuyển đổi chuỗi thành dạng TitleCase
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            string titleCaseInput = textInfo.ToTitleCase(input.ToLower());

            // Loại bỏ khoảng trắng và ghép lại
            string[] words = titleCaseInput.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            string camelCase = string.Join("", words);

            // Chuyển đổi ký tự đầu tiên thành lowercase
            camelCase = char.ToLower(camelCase[0]) + camelCase.Substring(1);

            return camelCase;
        }
    }
}
