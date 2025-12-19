// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.AIService.SpeakingAIService
{
    using System.Globalization;
    using System.IO;
    using System.Linq.Dynamic.Core;
    using System.Text;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Lms.Application.Commands.AiCmd;
    using Fsel.Course.Lms.Application.Commands.MockTestCmd.V1i1;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Course.Lms.Application.Services.AiService.SpeakingAIService;
    using Fsel.Course.Lms.Application.Services.AIService.SpeakingAIService.Models;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class SpeakingAIService : ISpeakingAIService
    {
        private readonly IMediator _mediator;
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly IMockTestScoreRepository _mockTestScoreRepository;
        private readonly IProsodyScoreRepository _prosodyScoreRepository;
        private readonly SubmitAiSpeakingAnswerPublisher _submitAiSpeakingAnswerPublisher;
        private readonly ISectionGroupRepository _sectionGroupRepository;
        private readonly ISectionGroupResultRepository _sectionGroupResultRepository;
        private readonly IMockTestAnswerRepository _mockTestAnswerRepository;

        public SpeakingAIService(IMediator mediator,
            IMockTestResultRepository mockTestResultRepository,
            IMockTestScoreRepository mockTestScoreRepository,
            IProsodyScoreRepository prosodyScoreRepository,
            SubmitAiSpeakingAnswerPublisher submitAiSpeakingAnswerPublisher,
            ISectionGroupRepository sectionGroupRepository,
            ISectionGroupResultRepository sectionGroupResultRepository,
            IMockTestAnswerRepository mockTestAnswerRepository)
        {
            _mediator = mediator;
            _mockTestResultRepository = mockTestResultRepository;
            _mockTestScoreRepository = mockTestScoreRepository;
            _prosodyScoreRepository = prosodyScoreRepository;
            _submitAiSpeakingAnswerPublisher = submitAiSpeakingAnswerPublisher;
            _sectionGroupRepository = sectionGroupRepository;
            _sectionGroupResultRepository = sectionGroupResultRepository;
            _mockTestAnswerRepository = mockTestAnswerRepository;
        }

        #region Handle

        /// <summary>
        /// Chấm điểm speaking bằng AI
        /// </summary>
        /// <param name="mockTestResultId"></param>
        /// <param name="sectionGroupId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<bool> EvaluationSpeakingAI(Guid mockTestResultId, Guid sectionGroupId, CancellationToken cancellationToken)
        {
            MethodResult<bool> methodResult = new MethodResult<bool>();
            var mockTestResult = _mockTestResultRepository.Queryable
                                            .Include(x => x.SectionGroupResults)
                                            .FirstOrDefault(x => x.Id == mockTestResultId);

            if (mockTestResult == null)
            {
                return false;
            }
            var sectionGroup = await _sectionGroupRepository.Queryable.Where(x => x.Id == sectionGroupId).FirstOrDefaultAsync(cancellationToken);

            var sectionGroupResult = await _sectionGroupResultRepository.Queryable.Where(x => x.SectionGroupId == sectionGroupId && x.MockTestResultId == mockTestResultId && x.CreatedDate >= mockTestResult.CreatedDate)
                                                                       .FirstOrDefaultAsync(cancellationToken);

            if (sectionGroupResult == null)
            {
                methodResult.Result = false;
                return methodResult.Result;
            }

            var mockTestAnswers = await _mockTestAnswerRepository.Queryable.Include(x => x.SectionTimeCode).Where(x => x.MockTestResultId == mockTestResultId && x.SectionGroupResultId == sectionGroupResult.Id)
                                                                           .AsNoTracking().ToListAsync(cancellationToken);

            var (questionArray, answerArray, averagePronScore, count) = ExtractQuestionAnswerAndPronunciationScores(mockTestAnswers);
            var scoreRanges = _prosodyScoreRepository.Queryable.ToList();
            (long bandScore, string? feedBack) = GetBandScore(averagePronScore, scoreRanges);

            List<MockTestScore> mockTestScores = new List<MockTestScore>
                                                        {
                                                            CreateMockTestScore(EnumMockTestScoreCriteria.Pronunciation, bandScore, feedBack ?? string.Empty, sectionGroupId, mockTestResultId)
                                                        };

            var criteria = new List<EnumMockTestScoreCriteria>
            {
                EnumMockTestScoreCriteria.GrammaticalRangeAndAccuracy,
                EnumMockTestScoreCriteria.LexicalResource,
                EnumMockTestScoreCriteria.FluencyAndCoherence
            };

            foreach (var item in criteria)
            {
                var aIResponse = await GetAIResponse(item, questionArray, answerArray, cancellationToken);
                var responseModel = ConvertHelper.Deserialize<AIEvaluationOutputModel>(aIResponse);

                if (long.TryParse(responseModel!.BandScore, out long bandScoreValue))
                {
                    mockTestScores.Add(CreateMockTestScore(item, bandScoreValue, responseModel.BandDescriptorText ?? string.Empty, sectionGroupId, mockTestResultId));
                }
            }

            var score = mockTestScores.Sum(x => x.Score);
            if (sectionGroupResult.SkillScores == null)
            {
                methodResult.Result = false;
                return methodResult.Result;
            }

            var skillScores = new List<SkillScores>();
            foreach (var skillScore in sectionGroupResult.SkillScores)
            {
                if (skillScore.Skill == sectionGroup?.CourseSkill)
                {
                    skillScore.CorrectCount = score;
                    skillScore.TotalCount = 36;
                    skillScore.Skill = EnumCourseSkill.Speaking;
                    skillScore.Scores = NumberHelper.RoundReduceNumber((double)score / 4); // sửa sau
                }
                skillScores.Add(skillScore);
            }
            sectionGroupResult.SkillScores = skillScores;
            sectionGroupResult.CorrectCount += (int)score;

            if (mockTestResult.SkillScores != null && mockTestResult.SkillScores.Count > 1)
            {
                if (skillScores.Any())
                {
                    // Bước 1: Clone danh sách mà không chứa phần tử có Skill là Speaking
                    var clonedSkillScores = mockTestResult.SkillScores
                        .Where(x => x.Skill != EnumCourseSkill.Speaking)
                        .ToList(); // Chuyển đổi thành danh sách

                    // Bước 2: Thêm phần tử từ skillScores.Single() vào danh sách đã clone
                    var skillScoreToAdd = skillScores.Single();
                    clonedSkillScores.Add(skillScoreToAdd);

                    // Bước 3: Gán ngược giá trị đã chỉnh sửa vào mockTestResult.SkillScores
                    mockTestResult.SkillScores = clonedSkillScores;
                }
            }
            else
            {
                mockTestResult.SkillScores = skillScores;
            }

            await SendToWebSocket(mockTestScores, cancellationToken);

            await SaveMockTestScoresToDatabase(mockTestScores, cancellationToken);

            //save skillscores
            await SaveSectionGroupResultToDatabase(sectionGroupResult, cancellationToken);

            //save MocKTestResult
            await SaveMockTestResultAsync(mockTestResult, cancellationToken);

            return methodResult.Result;
        }

        /// <summary>
        /// Tạo question, answer và averageScore tương ứng
        /// </summary>
        /// <param name="mockTestResult"></param>
        /// <returns></returns>
        private static (IList<string> questionArray, IList<string> answerArray, double averagePronScore, int count) ExtractQuestionAnswerAndPronunciationScores(IList<MockTestAnswer>? mockTestAnswers)
        {
            IList<string> questionArray = new List<string>();
            IList<string> answerArray = new List<string>();
            double pronScore = 0;
            int count = 0;

            mockTestAnswers ??= new List<MockTestAnswer>();
            foreach (var item in mockTestAnswers)
            {
                questionArray.Add(item?.SectionTimeCode?.Name ?? string.Empty);
                answerArray.Add(item?.SpeechTextAnswer ?? string.Empty);
                pronScore += item != null && item.PronunciationScore.HasValue ? item.PronunciationScore.Value : 0;

                if (item != null && item.PronunciationScore.HasValue && item.PronunciationScore.Value != 0)
                {
                    count++;
                }
            }

            double averagePronScore = Math.Round(count > 0 ? (double)pronScore / count : 0);
            return (questionArray, answerArray, averagePronScore, count);
        }

        /// <summary>
        /// Tạo model mocktestscore tương ứng
        /// </summary>
        /// <param name="criteria"></param>
        /// <param name="score"></param>
        /// <param name="feedback"></param>
        /// <param name="sectionGroupId"></param>
        /// <param name="mockTestResultId"></param>
        /// <returns></returns>
        private static MockTestScore CreateMockTestScore(EnumMockTestScoreCriteria criteria, long score, string feedback, Guid sectionGroupId, Guid mockTestResultId)
        {
            return new MockTestScore
            {
                Criteria = criteria,
                Score = score,
                FeedBack = feedback,
                SectionGroupId = sectionGroupId,
                MockTestResultId = mockTestResultId
            };
        }

        /// <summary>
        /// Lấy dữ liệu AI
        /// </summary>
        /// <param name="item"></param>
        /// <param name="questionArray"></param>
        /// <param name="answerArray"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<string> GetAIResponse(EnumMockTestScoreCriteria item, IList<string> questionArray, IList<string> answerArray, CancellationToken cancellationToken)
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

            return Shared.Helpers.StringHelper.RemoveMarkdownFromJson(aIResponse ?? string.Empty);
        }

        /// <summary>
        /// Lưu xuống db
        /// </summary>
        /// <param name="mockTestScores"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task SaveMockTestScoresToDatabase(List<MockTestScore> mockTestScores, CancellationToken cancellationToken)
        {
            await _mockTestScoreRepository.ExecuteTransactionAsync(async () =>
            {
                await _mockTestScoreRepository.AddList(mockTestScores);
                await _mockTestScoreRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                return new MethodResult<bool>();
            });
        }

        private async Task SaveSectionGroupResultToDatabase(SectionGroupResult sectionGroupResult, CancellationToken cancellationToken)
        {
            await _sectionGroupResultRepository.BulkUpdateList(new List<SectionGroupResult> { sectionGroupResult }, bulk =>
            {
                bulk.IgnoreOnUpdateExpression = c => new { c.SectionGroupId, c.StudentId, c.PlacementTestResultId, c.FinalTestResultId, c.MockTestResultId, c.WorkingTime };
            });

            //await _mockTestScoreRepository.ExecuteTransactionAsync(async () =>
            //{
            //return new MethodResult<bool>();

            //});
        }

        private async Task SaveMockTestResultAsync(MockTestResult mockTestResult, CancellationToken cancellationToken)
        {
            try
            {
                await _mockTestResultRepository.BulkUpdateList(new List<MockTestResult> { mockTestResult }, bulk =>
                {
                    bulk.IgnoreOnUpdateExpression = c => new { c.CourseId, c.StudentId, c.UnitId, c.MockTestId };
                });
                await _mockTestResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                await _mediator.Send(new SendTokenHistoryCommand { MockTestResultId = mockTestResult.Id }, cancellationToken);
            }
            catch { }
        }

        #endregion Handle

        #region Func

        /// <summary>
        /// Gửi kết quả đển websocket
        /// </summary>
        /// <param name="scores"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task SendToWebSocket(List<MockTestScore> scores, CancellationToken cancellationToken)
        {
            foreach (var score in scores)
            {
                SubmitAiSpeakingResponseModel model = new SubmitAiSpeakingResponseModel()
                {
                    CriteriaName = score.Criteria.ToString(),
                    BandScore = score.Score,
                    BandDescriptionText = score.FeedBack,
                    MockTestResultId = score.MockTestResultId,
                };
                await _submitAiSpeakingAnswerPublisher.Publish(model, cancellationToken);
            }
        }

        /// <summary>
        /// Lấy giá trị BandScore theo khoảng.
        /// </summary>
        /// <param name="averagePronScore"></param>
        /// <param name="scoreRanges"></param>
        /// <returns></returns>
        public static (long bandScore, string? comment) GetBandScore(double averagePronScore, List<ProsodyScore>? scoreRanges)
        {
            if (scoreRanges == null || scoreRanges.Count == 0)
            {
                return (0, string.Empty);
            }

            foreach (var range in scoreRanges)
            {
                if (averagePronScore >= range.MinScore && averagePronScore <= range.MaxScore)
                {
                    return ((long)range.BandScore, range.BandComment);
                }
            }
            return (0, string.Empty); // Hoặc giá trị mặc định nếu không tìm thấy khoảng phù hợp
        }

        /// <summary>
        /// config answer , question. để tạo thành prompt gửi cho chatgpt
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

        /// <summary>
        /// Lấy config của AI Speaking theo tiêu chí
        /// </summary>
        /// <param name="criteria"></param>
        /// <param name="isUserConfig"></param>
        /// <returns></returns>
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

        #endregion Func
    }
}