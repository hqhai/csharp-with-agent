// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Services.DictionaryServices
{
    using AutoMapper;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.ShareModels;
    using Fsel.System.Application.Services.AIServices;
    using Fsel.System.Application.Services.AIServices.Models;
    using Fsel.System.Application.Services.CourseServices;
    using Fsel.System.Application.Services.CourseServices.Models;
    using Fsel.System.Application.Services.CourseServices.QueryModels;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.Enums;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.CommandModels.ChatBot;
    using Fsel.System.Infrastructure.ValueSettings;
    using global::System.Text.Json;
    using StringHelper = Shared.Helpers.StringHelper;

    public class SemanticDictionaryService : ISemanticDictionaryService
    {
        private readonly IDictionaryAIRepository _dictionaryAIRepository;
        private readonly IOpenAIService _openAIService;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICourseService _courseService;
        private readonly SemanticDictionaryConfig _config;

        public SemanticDictionaryService(
            IDictionaryAIRepository dictionaryAIRepository,
            IOpenAIService openAIService,
            IMapper mapper,
            IUnitOfWork unitOfWork,
            ICourseService courseService,
            SemanticDictionaryConfig config)
        {
            _dictionaryAIRepository = dictionaryAIRepository;
            _openAIService = openAIService;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _courseService = courseService;
            _config = config ?? new SemanticDictionaryConfig();
        }

        public async Task<SemanticDictionaryResultModel?> SearchAsync(
            SemanticDictionaryRequestModel request,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.HighlightedItem))
            {
                throw new ArgumentException("Highlighted item is required");
            }

            if (request.HighlightedItem.Length > _config.MaxInputLength)
            {
                throw new ArgumentException($"Highlighted item exceeds {_config.MaxInputLength} characters");
            }

            var normalizedItem = NormalizeText(request.HighlightedItem);
            var searchText = !string.IsNullOrEmpty(request.SentenceContext)
                ? $"{normalizedItem} {request.SentenceContext}"
                : normalizedItem;

            var embedding = await GenerateEmbeddingAsync(searchText, cancellationToken);
            if (embedding == null || embedding.Length == 0)
            {
                return null;
            }

            var results = await _dictionaryAIRepository.VectorSearchAsync(
                embedding,
                request.SourceLanguage ?? "en",
                request.TargetLanguage ?? "vi",
                normalizedItem,
                _config.SimilarityThreshold,
                10,
                cancellationToken);

            var resultList = results.ToList();
            if (resultList.Count == 0)
            {
                return null;
            }

            var bestMatch = resultList.First();
            var result = _mapper.Map<SemanticDictionaryResultModel>(bestMatch);
            result.IsFromCache = true;

            return result;
        }

        public async Task<SemanticDictionaryResultModel?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            if (id == Guid.Empty)
            {
                return null;
            }

            var entity = await _dictionaryAIRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return null;
            }

            var result = _mapper.Map<SemanticDictionaryResultModel>(entity);
            result.IsFromCache = true;
            return result;
        }

        public async Task<SemanticDictionaryResultModel> GenerateAsync(
            SemanticDictionaryRequestModel request,
            CancellationToken cancellationToken = default)
        {
            var aiResponse = await CallAIAsync(request, cancellationToken);

            var normalizedItem = NormalizeText(request.HighlightedItem ?? "");

            var searchText = !string.IsNullOrEmpty(request.SentenceContext)
                ? $"{normalizedItem} {request.SentenceContext}"
                : normalizedItem;
            var embedding = await GenerateEmbeddingAsync(searchText, cancellationToken);

            var dictionaryId = Guid.NewGuid();
            var dictionary = new DictionaryAI
            {
                Id = dictionaryId,
                CreatedUserId = dictionaryId,
                CreatedFullName = "DictionaryAI",
                CreatedDate = DateTime.UtcNow,
                IsDeleted = false,
                HighlightedItemSource = aiResponse.HighlightedItemSource,
                HighlightedItemTarget = aiResponse.HighlightedItemTarget,
                DefinitionSource = aiResponse.DefinitionSource,
                DefinitionTarget = aiResponse.DefinitionTarget,
                NotesJson = JsonSerializer.Serialize(aiResponse.Notes),
                ExampleSentenceSource = aiResponse.ExampleSentenceSource,
                ExampleSentenceTarget = aiResponse.ExampleSentenceTarget,
                SourceLanguage = request.SourceLanguage,
                TargetLanguage = request.TargetLanguage,
                HasAudioFromLegacy = false,
                ModelName = aiResponse.ModelName ?? _config.DefaultCompletionModel,
                InputLength = (request.HighlightedItem ?? "").Length,
                Embedding = embedding
            };

            var existingEntity = await _dictionaryAIRepository.GetByIdAsync(dictionary.Id);
            if (existingEntity != null)
            {
                var existingResult = _mapper.Map<SemanticDictionaryResultModel>(existingEntity);
                existingResult.IsFromCache = true;
                return existingResult;
            }

            _dictionaryAIRepository.Add(dictionary);
            await _unitOfWork.SaveEntitiesAsync(cancellationToken);

            var result = _mapper.Map<SemanticDictionaryResultModel>(dictionary);
            result.IsFromCache = false;
            return result;
        }

        public async Task<float[]> GenerateEmbeddingAsync(
            string text,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _openAIService.GenerateEmbeddingAsync(new EmbeddingRequest
                {
                    Input = text,
                    Model = _config.EmbeddingModel
                });

                if (response?.Content?.Data == null || response.Content.Data.Count == 0)
                {
                    return Array.Empty<float>();
                }

                return response.Content.Data[0].Embedding.ToArray();
            }
            catch
            {
                return Array.Empty<float>();
            }
        }

        public async Task<Models.AIResponseModel> CallAIAsync(
            SemanticDictionaryRequestModel request,
            CancellationToken cancellationToken = default)
        {
            // Get AI config from LMS API
            var aiConfig = await GetAICriteriaConfigAsync(cancellationToken);

            var prompt = BuildPrompt(request, aiConfig);

            var response = await _openAIService.SubmitAICompletionsAsync(new RequestAIModel
            {
                Model = aiConfig.Model ?? _config.DefaultCompletionModel,
                Messages = new List<ChatBotMessageModel>
                {
                    new ChatBotMessageModel { Role = "system", Content = aiConfig.SystemRole },
                    new ChatBotMessageModel { Role = "user", Content = prompt }
                },
                Temperature = aiConfig.Temperature ?? _config.DefaultTemperature,
                MaxTokens = aiConfig.MaxTokens ?? _config.DefaultMaxTokens,
                TopP = aiConfig.TopP ?? _config.DefaultTopP,
                FrequencyPenalty = aiConfig.FrequencyPenalty ?? _config.DefaultFrequencyPenalty,
                PresencePenalty = aiConfig.PresencePenalty ?? _config.DefaultPresencePenalty,
            });

            if (response?.Content?.Choices == null || response.Content.Choices.Count == 0)
            {
                throw new InvalidOperationException("Empty AI response");
            }

            var content = response.Content.Choices.First().Message?.Content ?? "";
            content = StringHelper.RemoveMarkdownFromJson(content).Trim();

            var aiResponse = JsonSerializer.Deserialize<Models.AIResponseModel>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            });

            if (aiResponse == null)
            {
                throw new InvalidOperationException("Failed to parse AI response");
            }

            aiResponse.ModelName = aiConfig.Model ?? _config.DefaultCompletionModel;
            return aiResponse;
        }

        private async Task<AICriteriaConfig> GetAICriteriaConfigAsync(
            CancellationToken cancellationToken)
        {
            try
            {
                // Try to get config from LMS API using ObjectId, SubFeatureType, and FeatureMultiple
                var queryModel = new GetAICriteriaConfigsQueryModel
                {
                    ObjectIds = new List<Guid> { new Guid("E5689C3D-B0EA-4CAD-9A07-5D4B08EE08EE") },
                    SubFeatureType = EnumSubFeatureType.ClassForumDefault,
                    FeatureMultiple = EnumFeatureMultiple.Lesson
                };
                var response = await _courseService.GetConfigByIdsAsync(queryModel);

                if (response?.Content != null && response.Content.IsOK && response.Content.Result != null && response.Content.Result.Count > 0)
                {
                    var config = response.Content.Result.First();
                    var aiCriteria = config.AiCriteriaModels?.FirstOrDefault();
                    return new AICriteriaConfig
                    {
                        Model = config.AiModel,
                        Temperature = config.SettingTemperature,
                        MaxTokens = (int?)config.SettingWordMaxLength,
                        TopP = config.SettingTopP,
                        FrequencyPenalty = config.SettingFrequency,
                        PresencePenalty = config.SettingPresence,
                        SystemRole = aiCriteria?.SettingAiConfig,
                        UserRole = aiCriteria?.UserRole
                    };
                }
            }
            catch
            {
                // Fallback to default config if LMS API is unavailable
            }

            // Return default config
            return new AICriteriaConfig();
        }

        private static string NormalizeText(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            return text.Trim().ToLowerInvariant();
        }

        private static string BuildPrompt(SemanticDictionaryRequestModel request, AICriteriaConfig aiConfig)
        {
            var highlightedItem = request.HighlightedItem ?? "";
            var sentenceContext = request.SentenceContext ?? "";
            var sourceLang = request.SourceLanguage ?? "English";
            var targetLang = request.TargetLanguage ?? "VietNamese";
            var userRole = aiConfig.UserRole;

            return string.Format(aiConfig.UserRole, highlightedItem, sentenceContext, sourceLang, targetLang);
        }
    }
}
