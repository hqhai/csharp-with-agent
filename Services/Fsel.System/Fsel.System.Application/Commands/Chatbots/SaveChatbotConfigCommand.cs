// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.Chatbots
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Shared.Enums;
    using Fsel.System.Domain.Entities.Chatbots;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.CommandModels.ChatbotConfigs;
    using Fsel.System.Domain.Models.EntityModels;
    using global::System.Globalization;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

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
        private const string GrammarTopicList = "Grammar Topic List";
        private const string VocabularyLists = "Vocabulary Lists";

        public SaveChatbotConfigCommandHandler(IChatbotConfigRepository chatbotConfigRepository, IMapper mapper)
        {
            _chatbotConfigRepository = chatbotConfigRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<ChatbotConfigModel>> Handle(SaveChatbotConfigCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<ChatbotConfigModel>();


            //Config chung
            string programConfig = BuildTemplatePrompt(ProgramName, nameof(request.ProgramName), request.ProgramName?.ToString() ?? string.Empty);
            string courseConfig = BuildTemplatePrompt(CourseName, nameof(request.CourseName), request.CourseName?.ToString() ?? string.Empty);
            string cefrConfig = BuildTemplatePrompt(CEFRLevel, nameof(request.CEFRLevel), request.CEFRLevel?.ToString() ?? string.Empty);
            string unitTopicConfig = BuildTemplatePrompt(UnitTopic, nameof(request.UnitTopic), request.UnitTopic?.ToString() ?? string.Empty);
            string unitNumberConfig = BuildTemplatePrompt(UnitNumber, nameof(request.UnitNumber), request.UnitNumber?.ToString() ?? string.Empty);
            List<string> aiConfig = new List<string> { programConfig, courseConfig, cefrConfig, unitNumberConfig };


            var vocabConfigs = request.ChatbotSkillConfigs?.FirstOrDefault(x => x.Skill == EnumCourseSkill.Vocabulary)?.Configs ?? new List<SkillConfigModel>();
            string vocabConfigsPart = BuildTemplateConfigPrompt(VocabularyLists, vocabConfigs, false, true);
            string vocabConfigsFull = BuildTemplateConfigPrompt(VocabularyLists, vocabConfigs, true, true);

            var grammarConfigs = request.ChatbotSkillConfigs?.FirstOrDefault(x => x.Skill == EnumCourseSkill.Grammar)?.Configs ?? new List<SkillConfigModel>();
            string grammarPart = BuildTemplateConfigPrompt(GrammarTopicList, vocabConfigs, false, true);
            string grammarFull = BuildTemplateConfigPrompt(GrammarTopicList, vocabConfigs, true, true);

            //Config động
            foreach (var item in request.ChatbotSkillConfigs!)
            {
                if (item.Skill == EnumCourseSkill.Vocabulary)
                {
                    aiConfig.Add(grammarFull);
                    item.AiConfig = string.Join("\n", aiConfig);

                }
                else if (item.Skill == EnumCourseSkill.Grammar)
                {
                    aiConfig.Add(BuildTemplateConfigPrompt(GrammarTopicList, item.Configs!, true, true));
                    item.AiConfig = string.Join("\n", aiConfig);
                }
                else
                {
                    aiConfig.Add(UnitTopic);
                    aiConfig.Add(grammarPart);
                    aiConfig.Add(vocabConfigsPart);
                    item.AiConfig = string.Join("\n", aiConfig);
                }
            }

            bool isUpdate = false;
            var existChatBot = await _chatbotConfigRepository.Queryable.FirstOrDefaultAsync(x => x.UnitId == request.UnitId, cancellationToken);
            ChatbotConfig chatbotConfig = new ChatbotConfig();
            chatbotConfig = _mapper.Map<ChatbotConfig>(request);

            if (existChatBot != null)
            {
                isUpdate = true;
                existChatBot = _mapper.Map(request, existChatBot);
            }

            await _chatbotConfigRepository.ExecuteTransactionAsync(async () =>
            {
                if (!isUpdate)
                {
                    _chatbotConfigRepository.Add(chatbotConfig);
                }
                else
                {
                    existChatBot = _chatbotConfigRepository.Update(existChatBot!);

                }

                await _chatbotConfigRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
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

        /// <summary>
        /// Build Template Promt của các thông tin bên lề ProgramName, CourseName, etc...
        /// </summary>
        /// <param name="skillType"></param>
        /// <param name="customProp"></param>
        /// <param name="customValue"></param>
        /// <returns></returns>
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
        /// tạo template cho các kĩ năng
        /// </summary>
        /// <param name="skillType" desciption="tiêu đề"></param> 
        /// <param name="configs" desciption="giá trị config"></param> 
        /// <param name="getTopic" desciption="chỉ hiển thị TopicName"></param>
        /// <param name="getItem" desciption="chỉ hiển thị Item"></param>
        /// <returns></returns>
        private static string BuildTemplateConfigPrompt(string skillType, IList<SkillConfigModel> configs, bool getTopic, bool getItem)
        {
            string topicName = string.Empty;
            string itemName = string.Empty;
            string fullPrompt = string.Empty;

            int configCount = configs?.Count ?? 0;

            for (int i = 0; i < configCount; i++)
            {
                var listContent = configs?[i]?.ItemSkillContent ?? default;
                int countList = listContent?.Count ?? 0;

                for (int j = 0; j < countList; j++)
                {
                    if (!getItem)
                    {
                        break;

                    }

                    itemName += "\n" + listContent?[j]?.Content ?? string.Empty;

                    if (j < countList - 1)
                    {
                        itemName += ",\n";
                    }
                    else
                    {
                        itemName += "\n";
                    }
                }

                if (getTopic && !getItem)
                {
                    topicName += $"{configs?[i]?.Name ?? string.Empty}";
                    if (i < countList - 1)
                    {
                        topicName += ",\n";
                    }
                    else
                    {
                        topicName += "\n";
                    }
                }
                else if (!getTopic && getItem)
                {
                    fullPrompt += "\n[\n{" + $"{itemName}" + "}\n]\n";
                }
                else if (getTopic && getItem)
                {
                    fullPrompt += "\n[\n{\n" + $"{configs?[i]?.Name ?? string.Empty}" + "\n}\n]\n" + "\n[\n{" + $"{itemName}" + "}\n]\n";
                }

                itemName = string.Empty;
            }

            if (getTopic && !getItem)
            {
                fullPrompt += "\n[\n{\n" + topicName + "}\n]\n";
            }
            string result = skillType + ":" + fullPrompt;
            return result;
        }

        /// <summary>
        /// chuyển text thành dạng camelcase VD: Program Name => programName
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static string ConvertToCamelCase(string input)
        {
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            string titleCaseInput = textInfo.ToTitleCase(input?.ToLower() ?? string.Empty);

            string[] words = titleCaseInput.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            string camelCase = string.Join("", words);

            camelCase = char.ToLower(camelCase[0]) + camelCase.Substring(1);

            return camelCase;
        }



    }
}
