// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.Chatbots
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Shared.Constants;
    using Fsel.System.Domain.Entities.Chatbots;
    using Fsel.System.Domain.Enums;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.CommandModels.ChatbotConfigs;
    using Fsel.System.Domain.Models.EntityModels;
    using global::System.Globalization;
    using global::System.Text;
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

        private readonly string Program_Name = ValueSettings.PromptNameTemplate.ProgramName;
        private readonly string Course_Name = ValueSettings.PromptNameTemplate.CourseName;
        private readonly string CEFR_Level = ValueSettings.PromptNameTemplate.CEFRLevel;
        private readonly string Unit_Topic = ValueSettings.PromptNameTemplate.UnitTopic;
        private readonly string Unit_Number = ValueSettings.PromptNameTemplate.UnitNumber;
        private readonly string Grammar_TopicList = ValueSettings.PromptNameTemplate.GrammarTopicList;
        private readonly string Vocabulary_Lists = ValueSettings.PromptNameTemplate.VocabularyLists;

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
            List<string> baseConfigs = BuildBaseAiConfigs(request);

            var vocabConfigs = GetSkillConfigs(request, EnumChatbotLayout.Vocabulary);
            var grammarConfigs = GetSkillConfigs(request, EnumChatbotLayout.Grammar);

            var vocabPart = BuildTemplateConfigPrompt(Vocabulary_Lists, vocabConfigs, false, true);
            var vocabFull = BuildTemplateConfigPrompt(Vocabulary_Lists, vocabConfigs, true, true);

            var grammarPart = BuildTemplateConfigPrompt(Grammar_TopicList, grammarConfigs, false, true);
            var grammarFull = BuildTemplateConfigPrompt(Grammar_TopicList, grammarConfigs, true, true);

            //Config động
            foreach (var skill in request.ChatbotSkillConfigs ?? Enumerable.Empty<ChatbotSkillConfigsCommandModel>())
            {
                skill.AiConfig = BuildAiConfigByLayout(
                    skill.ChatbotLayout,
                    baseConfigs,
                    vocabPart,
                    vocabFull,
                    grammarPart,
                    grammarFull,
                    skill.Configs);
            }

            var chatbotConfig = await _chatbotConfigRepository.Queryable.FirstOrDefaultAsync(x => x.UnitId == request.UnitId, cancellationToken);
            if (chatbotConfig == null)
            {
                chatbotConfig = _mapper.Map<ChatbotConfig>(request);
            }
            else
            {
                _mapper.Map(request, chatbotConfig);
            }

            await _chatbotConfigRepository.ExecuteTransactionAsync(async () =>
            {
                if (chatbotConfig.Id == Guid.Empty)
                {
                    _chatbotConfigRepository.Add(chatbotConfig);
                }
                else
                {
                    chatbotConfig = _chatbotConfigRepository.Update(chatbotConfig!);
                }

                await _chatbotConfigRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<ChatbotConfigModel>(chatbotConfig);
                return methodResult;
            });
            return methodResult;
        }

        private static IList<SkillConfigCommandModel> GetSkillConfigs(
        SaveChatbotConfigCommand request,
        EnumChatbotLayout layout)
        {
            return request.ChatbotSkillConfigs?
                .FirstOrDefault(x => x.ChatbotLayout == layout)?
                .Configs
                ?? new List<SkillConfigCommandModel>();
        }

        private string BuildAiConfigByLayout(
        EnumChatbotLayout layout,
        IEnumerable<string> baseConfigs,
        string vocabPart,
        string vocabFull,
        string grammarPart,
        string grammarFull,
        IList<SkillConfigCommandModel>? currentConfigs)
        {
            var configs = new List<string>(baseConfigs);

            switch (layout)
            {
                case EnumChatbotLayout.Vocabulary:
                    configs.Add(grammarFull);
                    break;

                case EnumChatbotLayout.Grammar:
                    configs.Add(BuildTemplateConfigPrompt(
                        Grammar_TopicList,
                        currentConfigs ?? new List<SkillConfigCommandModel>(),
                        true,
                        true));
                    break;

                default:
                    configs.Add(Unit_Topic);
                    configs.Add(grammarPart);
                    configs.Add(vocabPart);
                    break;
            }

            return string.Join("\n", configs);
        }

        private List<string> BuildBaseAiConfigs(SaveChatbotConfigCommand request)
        {
            return new List<string>
            {
                BuildTemplatePrompt(Program_Name, nameof(request.ProgramName), request.ProgramName?.ToString() ?? string.Empty),
                BuildTemplatePrompt(Course_Name, nameof(request.CourseName), request.CourseName?.ToString() ?? string.Empty),
                BuildTemplatePrompt(CEFR_Level, nameof(request.CEFRLevel), request.CEFRLevel?.ToString() ?? string.Empty),
                BuildTemplatePrompt(Unit_Topic, nameof(request.UnitTopic), request.UnitTopic?.ToString() ?? string.Empty),
                BuildTemplatePrompt(Unit_Number, nameof(request.UnitNumber), request.UnitNumber?.ToString() ?? string.Empty),
            };
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
        private static string BuildTemplateConfigPrompt(
        string skillType,
        IList<SkillConfigCommandModel> configs,
        bool getTopic,
        bool getItem)
        {
            if (configs == null || configs.Count == 0)
            {
                return $"{skillType}:";
            }
            var fullPrompt = new StringBuilder();
            var topicBuilder = new StringBuilder();

            foreach (var config in configs)
            {
                if (config == null)
                {
                    continue;
                }
                // ===== Build Item =====
                if (getItem && config.ItemSkillContent?.Any() == true)
                {
                    var itemBuilder = new StringBuilder();

                    foreach (var item in config.ItemSkillContent)
                    {
                        if (!string.IsNullOrWhiteSpace(item?.Content))
                        {
                            itemBuilder.AppendLine(item.Content + ",");
                        }
                    }

                    if (itemBuilder.Length > 0)
                    {
                        // Remove last comma
                        itemBuilder.Length -= 3;

                        fullPrompt.AppendLine()
                                  .AppendLine("[")
                                  .AppendLine("{")
                                  .AppendLine(itemBuilder.ToString())
                                  .AppendLine("}")
                                  .AppendLine("]");
                    }
                }

                // ===== Build Topic =====
                if (getTopic)
                {
                    if (!string.IsNullOrWhiteSpace(config.Name))
                    {
                        topicBuilder.AppendLine(config.Name + ",");
                    }
                }

                // ===== Topic + Item =====
                if (getTopic && getItem && !string.IsNullOrWhiteSpace(config.Name))
                {
                    fullPrompt.Insert(0, $"\n[\n{{\n{config.Name}\n}}\n]\n");
                }
            }

            // ===== Final Topic Only =====
            if (getTopic && !getItem && topicBuilder.Length > 0)
            {
                topicBuilder.Length -= 3;

                fullPrompt.AppendLine()
                          .AppendLine("[")
                          .AppendLine("{")
                          .AppendLine(topicBuilder.ToString())
                          .AppendLine("}")
                          .AppendLine("]");
            }

            return $"{skillType}:{fullPrompt}";
        }

        /// <summary>
        /// chuyển text thành dạng camelcase VD: Program Name => programName
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static string ConvertToCamelCase(string input)
        {
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            string titleCaseInput = textInfo.ToTitleCase(input?.ToLower(CultureInfo.CurrentCulture) ?? string.Empty);

            string[] words = titleCaseInput.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            string camelCase = string.Join("", words);

            camelCase = char.ToLower(camelCase[0], CultureInfo.CurrentCulture) + camelCase.Substring(1);
            return camelCase;
        }
    }
}
