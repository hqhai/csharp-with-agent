// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.AIService.SpeakingAIService
{
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Commands.AiCmd;
    using Fsel.Course.Lms.Application.Services.AiService.SpeakingAIService;
    using Fsel.Course.Lms.Application.Services.AIService.SpeakingAIService.Models;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class SpeakingAIService : ISpeakingAIService
    {
        private readonly IMediator _mediator;
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly IMockTestScoreRepository _mockTestScoreRepository;
        private readonly IMapper _mapper;

        public SpeakingAIService(IMediator mediator, IMockTestResultRepository mockTestResultRepository, IMockTestScoreRepository mockTestScoreRepository, IMapper mapper)
        {
            _mediator = mediator;
            _mockTestResultRepository = mockTestResultRepository;
            _mockTestScoreRepository = mockTestScoreRepository;
            _mapper = mapper;
        }


        public async Task<bool> EvaluationSpeakingAI(Guid mockTestResultId, Guid sectionGroupId, CancellationToken cancellationToken)
        {
            List<MockTestScore> mockTestScores = new List<MockTestScore>();

            MethodResult<bool> methodResult = new MethodResult<bool>();
            var mockTestResult = _mockTestResultRepository.Queryable.Include(x => x.MockTestAnswers).ThenInclude(x => x.SectionTimeCode).Where(x => x.Id == mockTestResultId).FirstOrDefault();

            List<EnumMockTestScoreCriteria> criteria = new List<EnumMockTestScoreCriteria> { EnumMockTestScoreCriteria.GrammaticalRangeAndAccuracy, EnumMockTestScoreCriteria.LexicalResource, EnumMockTestScoreCriteria.FluencyAndCoherence };

            //Validate
            if (mockTestResult == null)
            {
                return false;
            }

            // Build Dynamic Config 
            IList<string> questionArray = new List<string>();
            IList<string> answerArray = new List<string>();
            foreach (var item in mockTestResult.MockTestAnswers)
            {
                questionArray.Add(item?.SectionTimeCode?.Name ?? string.Empty);
                answerArray.Add(item?.SpeechTextAnswer ?? string.Empty);
            }


            //Handler Data
            foreach (var item in criteria)
            {
                string userAiConfig = CustomAnswerConfigToSendGPT(questionArray, answerArray, item);

                var aIResponse = await _mediator.Send(new SubmitAICommand
                {
                    SystemRoleAlConfig = GetConfigByType(item, true),
                    UserAIConfig = userAiConfig,
                    SettingModel = "gpt-4o",
                    SettingTemperature = 1,
                    SettingFrequecy = 0,
                    SettingWordMaxLength = 1000,
                    SettingPresence = 0,
                    SettingTopP = 1
                }, cancellationToken).ConfigureAwait(false);
                aIResponse = RemoveMarkdownFromJson(aIResponse ?? string.Empty);
                var responseModel = ConvertHelper.Deserialize<AIEvaluationOutputModel>(aIResponse);

                if (long.TryParse(responseModel!.BandScore, out long bandScoreValue))
                {
                    MockTestScore mockTestScoreModel = new MockTestScore()
                    {
                        Criteria = item,
                        Score = bandScoreValue,
                        FeedBack = responseModel!.BandDescriptorText,
                        SectionGroupId = sectionGroupId,
                        MockTestResultId = mockTestResultId
                    };

                    mockTestScores.Add(mockTestScoreModel);
                }
            }


            await _mockTestScoreRepository.ExecuteTransactionAsync(async () =>
            {
                await _mockTestScoreRepository.AddList(mockTestScores);
                await _mockTestResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                return methodResult;


            });

            return methodResult.Result;
        }

        /// <summary>
        /// Hàm loại bỏ MarkDown của chatgpt trả về
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static string RemoveMarkdownFromJson(string json)
        {
            // Loại bỏ dấu ```json từ đầu và cuối chuỗi JSON
            string cleanedJson = Regex.Replace(json, @"^```json\s*|\s*```$", "");

            // Trả về chuỗi JSON đã được loại bỏ dấu ```json
            return cleanedJson;
        }

        /// <summary>
        /// config answer , question.
        /// </summary>
        /// <param name="answer"></param>
        /// <returns></returns>
        private static string CustomAnswerConfigToSendGPT(IList<string> questions, IList<string> answers, EnumMockTestScoreCriteria criteria)
        {
            StringBuilder sb = new StringBuilder();
            string defaultConfigByCriteria = GetConfigByType(criteria, false);

            // Generate questions
            sb.AppendLine("Speaking Test Questions:");
            for (int i = 0; i < questions.Count; i++)
            {
                sb.AppendFormat(CultureInfo.InvariantCulture, "\"question{0}\": \"{1}\"\n", NumberToWords(i + 1), questions[i]);
            }

            sb.AppendLine();

            // Generate answers
            sb.AppendLine("Student Submission:");
            for (int i = 0; i < answers.Count; i++)
            {
                sb.AppendFormat(CultureInfo.InvariantCulture, "\"answer{0}\": \"{1}\"\n", NumberToWords(i + 1), answers[i]);
            }
            sb.AppendLine();

            string result = sb.ToString();

            result = string.Concat(result, " ", defaultConfigByCriteria);


            return result;
        }

        private static string GetConfigByType(EnumMockTestScoreCriteria criteria, bool isUserConfig)
        {
            string result = string.Empty;
            switch (criteria)
            {
                case EnumMockTestScoreCriteria.GrammaticalRangeAndAccuracy:
                    result = isUserConfig ? File.ReadAllText(ResourceSettings.SpeakingGrammarRole) : File.ReadAllText(ResourceSettings.SpeakingGrammar);
                    break;
                case EnumMockTestScoreCriteria.LexicalResource:
                    result = isUserConfig ? File.ReadAllText(ResourceSettings.SpeakingLexicalRole) : File.ReadAllText(ResourceSettings.SpeakingLexical);
                    break;
                case EnumMockTestScoreCriteria.FluencyAndCoherence:
                    result = isUserConfig ? File.ReadAllText(ResourceSettings.SpeakingFluencyRole) : File.ReadAllText(ResourceSettings.SpeakingFluency);

                    break;
            }

            return result;
        }



        /// <summary>
        /// Chuyển đổi số thành chữ
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        private static string NumberToWords(int number)
        {
            if (number == 0)
            {
                return "Zero";
            }
            if (number < 0)
            {
                return "Minus" + NumberToWords(Math.Abs(number));
            }

            string words = "";

            if ((number / 1000000) > 0)
            {
                words += NumberToWords(number / 1000000) + " Million ";
                number %= 1000000;
            }
            if ((number / 1000) > 0)
            {
                words += NumberToWords(number / 1000) + " Thousand ";
                number %= 1000;
            }
            if ((number / 100) > 0)
            {
                words += NumberToWords(number / 100) + " Hundred ";
                number %= 100;
            }
            if (number > 0)
            {
                if (string.IsNullOrEmpty(words))
                {
                    words += "";
                }
                var unitsMap = new[] { "Zero", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen" };
                var tensMap = new[] { "Zero", "Ten", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };

                if (number < 20)
                {
                    words += unitsMap[number];
                }
                else
                {
                    words += tensMap[number / 10];
                    if ((number % 10) > 0)
                    {
                        words += "-" + unitsMap[number % 10];
                    }
                }
            }

            return words;
        }
    }
}
