// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Realtime.Application.Queues.Publishers
{
    using Fsel.Core.Base.Interfaces;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels;

    public class SemanticDictionaryPublisher
    {
        private readonly IQueueProvider _queueProvider;

        public SemanticDictionaryPublisher(IQueueProvider queueProvider)
        {
            _queueProvider = queueProvider;
        }

        public async Task Publish(string userId, SemanticDictionaryRequestModel request, CancellationToken cancellationToken)
        {
            var queueModel = new SemanticDictionaryQueueModel
            {
                UserId = userId,
                Result = GenerateHardcodedResult(request)
            };

            await _queueProvider.Publish(
                QueueSettings.RealtimeQueue.NameQueue.SemanticDictionary,
                queueModel,
                cancellationToken);
        }

        /// <summary>
        /// Generate hardcoded sample result (for testing without AI service)
        /// </summary>
        private static SemanticDictionaryResultModel GenerateHardcodedResult(SemanticDictionaryRequestModel request)
        {
            var word = request.HighlightedItem ?? "";
            var targetLang = request.TargetLanguage ?? "vi";
            var translatedWord = targetLang;

            return new SemanticDictionaryResultModel
            {
                Id = Guid.NewGuid().ToString(),
                HighlightedItemSource = word,
                HighlightedItemTarget = translatedWord,
                DefinitionSource = "A form of energy that makes things visible and is emitted by sources such as lamps, the sun, or fires. It can also refer to a device that produces illumination.",
                DefinitionTarget = "Năng lượng giúp mọi thứ trở nên rõ ràng và phát ra từ các nguồn như đèn, mặt trời hoặc lửa. Cũng có thể chỉ đến một thiết bị tạo sáng.",
                SourceLanguage = request.SourceLanguage ?? "en",
                TargetLanguage = targetLang,
                ExampleSentenceSource = request.SentenceContext,
                ExampleSentenceTarget = request.SentenceContext,
                SimilarityScore = 0.95,
                IsFromCache = false,
                HasAudioFromLegacy = false,
                JsonPayload = null,
                Notes = new List<SemanticNoteModel>
                {
                    new()
                    {
                        SemanticChunk = request.SentenceContext ?? "",
                        TypeSource = "verb",
                        TypeTarget = "động từ",
                        ExplanationSource = "In this context, 'light' as a verb means to make something start to burn or to switch on a source of illumination.",
                        ExplanationTarget = "Trong ngữ cảnh này, 'light' nghĩa là bật sáng hoặc làm cho thứ gì đó bắt đầu cháy."
                    },
                    new()
                    {
                        SemanticChunk = "the light room",
                        TypeSource = "noun phrase",
                        TypeTarget = "danh từ",
                        ExplanationSource = "Here, 'light' refers to a room that is well-lit or associated with light, possibly a room with good illumination or a room named 'Light'.",
                        ExplanationTarget = "'phòng sáng' hoặc 'phòng có ánh sáng' trong ngữ cảnh này, đề cập đến phòng có ánh sáng tốt hoặc tên gọi của phòng."
                    },
                    new()
                    {
                        SemanticChunk = "with a light",
                        TypeSource = "noun phrase",
                        TypeTarget = "danh từ",
                        ExplanationSource = "In this context, 'a light' refers to a source of illumination, such as a lamp or bulb, used to light the room.",
                        ExplanationTarget = "'một nguồn sáng' đề cập đến một thiết bị chiếu sáng như đèn hoặc bóng đèn, dùng để thắp sáng phòng."
                    }
                }
            };
        }

        private static string GetTranslatedWord(string word, string targetLanguage)
        {
            // Simple hardcoded translations for demo
            var translations = new Dictionary<string, Dictionary<string, string>>
            {
                ["hello"] = new() { ["vi"] = "xin chào", ["ko"] = "안녕하세요", ["ja"] = "こんにちは", ["zh"] = "你好" },
                ["world"] = new() { ["vi"] = "thế giới", ["ko"] = "세계", ["ja"] = "世界", ["zh"] = "世界" },
                ["love"] = new() { ["vi"] = "yêu", ["ko"] = "사랑", ["ja"] = "愛", ["zh"] = "爱" },
                ["computer"] = new() { ["vi"] = "máy tính", ["ko"] = "컴퓨터", ["ja"] = "コンピュータ", ["zh"] = "电脑" }
            };

            var lowerWord = word.ToLowerInvariant();
            if (translations.TryGetValue(lowerWord, out var langDict))
            {
                return langDict.TryGetValue(targetLanguage.ToLowerInvariant(), out var trans) ? trans : word;
            }
            return $"[{targetLanguage}]" + word;
        }
    }
}
