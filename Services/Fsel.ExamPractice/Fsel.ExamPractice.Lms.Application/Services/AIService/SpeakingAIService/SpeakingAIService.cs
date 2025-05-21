// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Lms.Application.Services.AIService.SpeakingAIService
{
    using System.Globalization;
    using System.IO;
    using System.Text;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.ExamPractice.Domain.Entities;
    using Fsel.ExamPractice.Domain.Entities.SkillScoreConfigs;
    using Fsel.ExamPractice.Domain.IRepositories;
    using Fsel.ExamPractice.Lms.Application.Commands.AiCmd;
    using Fsel.ExamPractice.Lms.Application.Queues.Publishers;
    using Fsel.ExamPractice.Lms.Application.Services.AiService.SpeakingAIService;
    using Fsel.ExamPractice.Lms.Application.Services.AIService.SpeakingAIService.Models;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class SpeakingAIService : ISpeakingAIService
    {
        private readonly IMediator _mediator;
        private readonly IExamPracticeResultRepository _examPracticeResultRepository;
        private readonly IExamPracticeScoreRepository _examPracticeScoreRepository;
        private readonly IProsodyScoreRepository _prosodyScoreRepository;
        private readonly SubmitAiSpeakingAnswerPublisher _submitAiSpeakingAnswerPublisher;
        private readonly IExamPracticeSectionRepository _examPracticeSectionRepository;
        private readonly IExamPracticeSectionResultRepository _examPracticeSectionResultRepository;

        public SpeakingAIService(IMediator mediator, IExamPracticeResultRepository examPracticeResultRepository,
            IExamPracticeScoreRepository examPracticeScoreRepository,
            IProsodyScoreRepository prosodyScoreRepository,
            SubmitAiSpeakingAnswerPublisher submitAiSpeakingAnswerPublisher,
            IExamPracticeSectionRepository examPracticeSectionRepository,
            IExamPracticeSectionResultRepository examPracticeSectionResultRepository)
        {
            _mediator = mediator;
            _examPracticeResultRepository = examPracticeResultRepository;
            _examPracticeScoreRepository = examPracticeScoreRepository;
            _prosodyScoreRepository = prosodyScoreRepository;
            _submitAiSpeakingAnswerPublisher = submitAiSpeakingAnswerPublisher;
            _examPracticeSectionRepository = examPracticeSectionRepository;
            _examPracticeSectionResultRepository = examPracticeSectionResultRepository;
        }

        #region Handle

        /// <summary>
        /// Chấm điểm speaking bằng AI
        /// </summary>
        /// <param name="mockTestResultId"></param>
        /// <param name="sectionGroupId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<bool> EvaluationSpeakingAI(Guid examPracticeResultId, Guid examPracticeSectionId, CancellationToken cancellationToken)
        {
            MethodResult<bool> methodResult = new MethodResult<bool>();
            var examPracticeResult = await _examPracticeResultRepository.Queryable
                                                .Include(x => x.ExamPracticeAnswers)
                                                .ThenInclude(x => x.ExamPracticeSection)
                                                .Include(x => x.ExamPracticeSectionResults)
                                                .FirstOrDefaultAsync(x => x.Id == examPracticeResultId, cancellationToken);

            if (examPracticeResult == null)
            {
                return false;
            }

            var (questionArray, answerArray, averagePronScore, count) = ExtractQuestionAnswerAndPronunciationScores(examPracticeResult);
            var scoreRanges = await _prosodyScoreRepository.Queryable.ToListAsync(cancellationToken);
            (long bandScore, string? feedBack) = GetBandScore(averagePronScore, scoreRanges);

            List<ExamPracticeScore> examPracticeScores = new List<ExamPracticeScore>
            {
                CreateExamPracticeScore(EnumMockTestScoreCriteria.Pronunciation, bandScore, feedBack ?? string.Empty, examPracticeSectionId, examPracticeResultId)
            };

            var criteria = new List<EnumMockTestScoreCriteria>
            {
                EnumMockTestScoreCriteria.GrammaticalRangeAndAccuracy,
                EnumMockTestScoreCriteria.LexicalResource,
                EnumMockTestScoreCriteria.FluencyAndCoherence
            };

            var examPracticeSection = await _examPracticeSectionRepository.GetByIdAsync(examPracticeSectionId);
            var examPracticeSectionResult = await _examPracticeSectionResultRepository.Queryable
                                           .Where(x => x.ExamPracticeSectionId == examPracticeSectionId && x.ExamPracticeResultId == examPracticeSectionId)
                                           .FirstOrDefaultAsync(cancellationToken);

            if (examPracticeSectionResult == null)
            {
                methodResult.Result = false;
                return methodResult.Result;
            }

            foreach (var item in criteria)
            {
                var aIResponse = await GetAIResponse(item, questionArray, answerArray, cancellationToken);
                var responseModel = ConvertHelper.Deserialize<AIEvaluationOutputModel>(aIResponse);

                if (long.TryParse(responseModel!.BandScore, out long bandScoreValue))
                {
                    examPracticeScores.Add(CreateExamPracticeScore(item, bandScoreValue, responseModel.BandDescriptorText ?? string.Empty, examPracticeSectionId, examPracticeResultId));
                }
            }

            var score = examPracticeScores.Sum(x => x.Score);
            if (examPracticeSectionResult.SkillScores == null)
            {
                methodResult.Result = false;
                return methodResult.Result;
            }

            var skillScores = new List<SkillScores>();
            foreach (var skillScore in examPracticeSectionResult.SkillScores)
            {
                if (skillScore.Skill == examPracticeSection?.CourseSkill)
                {
                    skillScore.CorrectCount = score;
                    skillScore.TotalCount = 36;
                    skillScore.Skill = EnumCourseSkill.Speaking;
                    skillScore.Scores = NumberHelper.RoundReduceNumber((double)score / 4); // sửa sau
                }
                skillScores.Add(skillScore);
            }
            examPracticeSectionResult.SkillScores = skillScores;
            examPracticeSectionResult.CorrectCount += (int)score;

            if (examPracticeResult.SkillScores != null && examPracticeResult.SkillScores.Count > 1)
            {
                if (skillScores.Any())
                {
                    // Bước 1: Clone danh sách mà không chứa phần tử có Skill là Speaking
                    var clonedSkillScores = examPracticeResult.SkillScores
                        .Where(x => x.Skill != EnumCourseSkill.Speaking)
                        .ToList(); // Chuyển đổi thành danh sách

                    // Bước 2: Thêm phần tử từ skillScores.Single() vào danh sách đã clone
                    var skillScoreToAdd = skillScores.Single();
                    clonedSkillScores.Add(skillScoreToAdd);

                    // Bước 3: Gán ngược giá trị đã chỉnh sửa vào examPracticeResult.SkillScores
                    examPracticeResult.SkillScores = clonedSkillScores;
                }
            }
            else
            {
                examPracticeResult.SkillScores = skillScores;
            }

            await SendToWebSocket(examPracticeScores, cancellationToken);

            await SaveExamPracticeScoresToDatabase(examPracticeScores, cancellationToken);

            //save skillscores
            await SaveExamPracticeSectionResultToDatabase(examPracticeSectionResult, cancellationToken);

            //save ExamPracticeResult
            await SaveExamPracticeResultAsync(examPracticeResult, cancellationToken);

            return methodResult.Result;
        }

        /// <summary>
        /// Tạo question, answer và averageScore tương ứng
        /// </summary>
        /// <param name="examPracticeResult"></param>
        /// <returns></returns>
        private static (IList<string> questionArray, IList<string> answerArray, double averagePronScore, int count) ExtractQuestionAnswerAndPronunciationScores(ExamPracticeResult examPracticeResult)
        {
            IList<string> questionArray = new List<string>();
            IList<string> answerArray = new List<string>();
            double pronScore = 0;
            int count = 0;

            examPracticeResult.ExamPracticeAnswers = examPracticeResult.ExamPracticeAnswers.Where(x => x.ExamPracticeSectionResult?.SkillScores?.FirstOrDefault()?.Skill == EnumCourseSkill.Speaking).ToList();

            foreach (var item in examPracticeResult.ExamPracticeAnswers)
            {
                questionArray.Add(item?.ExamPracticeSection?.Name ?? string.Empty);
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
        /// Tạo model ExamPracticeScore tương ứng
        /// </summary>
        /// <param name="criteria"></param>
        /// <param name="score"></param>
        /// <param name="feedback"></param>
        /// <param name="examPracticeSectionId"></param>
        /// <param name="examPracticeResultId"></param>
        /// <returns></returns>
        private static ExamPracticeScore CreateExamPracticeScore(EnumMockTestScoreCriteria criteria, long score, string feedback, Guid examPracticeSectionId, Guid examPracticeResultId)
        {
            return new ExamPracticeScore
            {
                Criteria = criteria,
                Score = score,
                FeedBack = feedback,
                ExamPracticeSectionId = examPracticeSectionId,
                ExamPracticeResultId = examPracticeResultId
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
        private async Task SaveExamPracticeScoresToDatabase(List<ExamPracticeScore> examPracticeScores, CancellationToken cancellationToken)
        {
            await _examPracticeScoreRepository.ExecuteTransactionAsync(async () =>
            {
                await _examPracticeScoreRepository.AddList(examPracticeScores);
                await _examPracticeScoreRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                return new MethodResult<bool>();
            });
        }

        private async Task SaveExamPracticeSectionResultToDatabase(ExamPracticeSectionResult examPracticeSectionResult, CancellationToken cancellationToken)
        {
            _examPracticeSectionResultRepository.Update(examPracticeSectionResult, false, x => x.WorkingTime);
            await _examPracticeSectionResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        private async Task SaveExamPracticeResultAsync(ExamPracticeResult examPracticeResult, CancellationToken cancellationToken)
        {
            try
            {
                _examPracticeResultRepository.Update(examPracticeResult);
                await _examPracticeResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
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
        private async Task SendToWebSocket(List<ExamPracticeScore> scores, CancellationToken cancellationToken)
        {
            foreach (var score in scores)
            {
                SubmitExamPracticeAiSpeakingResponseModel model = new SubmitExamPracticeAiSpeakingResponseModel()
                {
                    CriteriaName = score.Criteria.ToString(),
                    BandScore = score.Score,
                    BandDescriptionText = score.FeedBack,
                    ExamPracticeResultId = score.ExamPracticeSectionId,
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
