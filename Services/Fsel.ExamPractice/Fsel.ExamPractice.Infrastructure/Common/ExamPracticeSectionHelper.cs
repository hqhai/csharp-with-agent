// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Infrastructure.Common
{
    using System;
    using Fsel.Common.Helpers;
    using Fsel.ExamPractice.Domain.Entities;
    using Fsel.ExamPractice.Domain.Entities.QuestionTypeConfigs.Answers.V1i1;
    using Fsel.ExamPractice.Domain.Entities.SkillScoreConfigs;
    using Fsel.ExamPractice.Domain.Enums;
    using Fsel.ExamPractice.Domain.IRepositories;
    using Fsel.ExamPractice.Domain.Models.EntityModels.Configs;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Microsoft.EntityFrameworkCore;

    public class ExamPracticeSectionHelper
    {
        private readonly IQuestionRepository _questionRepository;
        private readonly IExamPracticeAnswerRepository _examPracticeAnswerRepository;
        private readonly IExamPracticeSectionRepository _examPracticeSectionRepository;
        private readonly IExamPracticeSectionResultRepository _examPracticeSectionResultRepository;

        public ExamPracticeSectionHelper(IQuestionRepository questionRepository,
            IExamPracticeAnswerRepository examPracticeAnswerRepository,
            IExamPracticeSectionRepository examPracticeSectionRepository,
            IExamPracticeSectionResultRepository examPracticeSectionResultRepository)
        {
            _questionRepository = questionRepository;
            _examPracticeAnswerRepository = examPracticeAnswerRepository;
            _examPracticeSectionRepository = examPracticeSectionRepository;
            _examPracticeSectionResultRepository = examPracticeSectionResultRepository;
        }

        public async Task UpdateExamPracticeToIsSubmit(ExamPractice examPractice, ExamPracticeSection examPracticeSection, ExamPracticeSectionResult examPracticeSectionResult, bool isSubmit)
        {
            if (isSubmit)
            {
                await UpdateUnansweredQuestions(examPracticeSection, examPracticeSectionResult);
                await UpdateAnswerProcessByTest(examPracticeSectionResult).ConfigureAwait(false);
                await CreateUnansweredExamPracticeSectionResultsAsync(examPracticeSectionResult);
                if (examPracticeSection.CourseSkill == EnumCourseSkill.Reading || examPracticeSection.CourseSkill == EnumCourseSkill.Listening)
                {
                    await UpdateExamPracticeSectionResultChildrenAsync(examPracticeSection, examPracticeSectionResult);
                }
                await UpdateExamPracticeSectionResultAsync(examPractice, examPracticeSection, examPracticeSectionResult);
            }
        }

        public async Task UpdateExamPracticeToIsSubmit(ExamPracticeResult examPracticeResult)
        {
            await CreateUnansweredExamPracticeSectionResultsAsync(examPracticeResult);
            await UpdateUnansweredQuestions(examPracticeResult);
            await UpdateAnswerProcessByTest(examPracticeResult).ConfigureAwait(false);
            await UpdateExamPracticeSectionResultsAsync(examPracticeResult);
        }

        private async Task UpdateExamPracticeSectionResultChildrenAsync(ExamPracticeSection examPracticeSection, ExamPracticeSectionResult examPracticeSectionResult)
        {
            var examPracticeSectionResultChildrens = await _examPracticeSectionResultRepository.Queryable.Where(x => x.ParentExamPracticeSectionResultId == examPracticeSectionResult.Id)
                                                                                               .ToListAsync();
            var examPracticeSectionIds = examPracticeSectionResultChildrens.Select(x => x.ExamPracticeSectionId).ToList();

            var examPracticeAnswers = await _examPracticeAnswerRepository.Queryable.Where(x => x.ExamPracticeSectionResultId == examPracticeSectionResult.Id).AsNoTracking().ToListAsync();
            var questions = await _questionRepository.Queryable.WhereBulkContains(examPracticeSectionIds, x => x.ExamPracticeSectionId).AsNoTracking().ToListAsync();

            foreach (var item in examPracticeSectionResultChildrens)
            {
                var listQuestion = questions.Where(x => x.ExamPracticeSectionId == item.ExamPracticeSectionId).ToList();
                var answers = examPracticeAnswers.Where(x => x.QuestionId.HasValue && listQuestion.Select(x => x.Id).Contains(x.QuestionId.Value)).ToList();
                var countQuestion = 0;
                foreach (var answer in answers)
                {
                    var question = listQuestion.FirstOrDefault(x => x.Id == answer.QuestionId);
                    countQuestion += AnswerTypeHelper.GetTotalCorrectByAnswerType(question, answer?.Answer);
                }

                item.CorrectCount = answers.Sum(x => x.CorrectCount);
                item.CorrectTotal = listQuestion.Sum(x => x.CorrectTotal);
                item.Status = EnumResultStatus.Done;
                item.SkillScores = new List<SkillScores> { new SkillScores
                {
                    CorrectCount = answers.Sum(x => x.CorrectCount),
                    TotalCount = listQuestion.Sum(x => x.CorrectTotal),
                    Skill = examPracticeSection.CourseSkill ?? default,
                    CountQuestion = countQuestion,
                    TotalQuestion = listQuestion.Sum(x => x.CorrectTotal),
                }};
            }
            _examPracticeSectionResultRepository.UpdateList(examPracticeSectionResultChildrens);
            await _examPracticeSectionResultRepository.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task CreateUnansweredExamPracticeSectionResultsAsync(ExamPracticeResult examPracticeResult)
        {
            ArgumentNullException.ThrowIfNull(examPracticeResult);
            var resultExamPracticeSectionIds = await _examPracticeSectionResultRepository.Queryable
                                                    .Where(x => x.ExamPracticeResultId == examPracticeResult.Id).AsNoTracking()
                                                    .Select(x => x.ExamPracticeSectionId)
                                                    .ToListAsync();
            var examPracticeSectionIds = await _examPracticeSectionRepository.Queryable
                                                .Where(x => x.ExamPracticeId == examPracticeResult.ExamPracticeId)
                                                .AsNoTracking()
                                                .Select(x => x.Id)
                                                .ToListAsync();
            var examPracticeUnansweredSectionIds = examPracticeSectionIds.Except(resultExamPracticeSectionIds);
            var createExamPracticeSectionResults = new List<ExamPracticeSectionResult>();

            foreach (var item in examPracticeUnansweredSectionIds)
            {
                createExamPracticeSectionResults.Add(new ExamPracticeSectionResult
                {
                    ExamPracticeSectionId = item,
                    StudentId = examPracticeResult.StudentId,
                    ExamPracticeResultId = examPracticeResult.Id,
                    Status = EnumResultStatus.Process
                });
            }
            await _examPracticeSectionResultRepository.BulkMergeAsync(createExamPracticeSectionResults, bulk =>
             {
                 bulk.ColumnPrimaryKeyExpression = entity => new { entity.StudentId, entity.ExamPracticeSectionId, entity.ExamPracticeResultId };
             });
        }

        public async Task CreateUnansweredExamPracticeSectionResultsAsync(ExamPracticeSectionResult examPracticeSectionResult)
        {
            ArgumentNullException.ThrowIfNull(examPracticeSectionResult);
            var resultExamPracticeSectionIds = await _examPracticeSectionResultRepository.Queryable.Where(x => x.ParentExamPracticeSectionResultId == examPracticeSectionResult.Id)
                                                                        .AsNoTracking()
                                                                        .Select(x => x.ExamPracticeSectionId)
                                                                        .ToListAsync();
            var examPracticeSectionIds = await _examPracticeSectionRepository.Queryable.Where(x => x.ParentExamPracticeSectionId == examPracticeSectionResult.ExamPracticeSectionId)
                                                                        .AsNoTracking()
                                                                        .Select(x => x.Id).ToListAsync();

            var examPracticeUnansweredSectionIds = examPracticeSectionIds.Except(resultExamPracticeSectionIds);
            var createExamPracticeSectionResults = new List<ExamPracticeSectionResult>();

            foreach (var item in examPracticeUnansweredSectionIds)
            {
                createExamPracticeSectionResults.Add(new ExamPracticeSectionResult
                {
                    ExamPracticeSectionId = item,
                    StudentId = examPracticeSectionResult.StudentId,
                    ExamPracticeResultId = examPracticeSectionResult.ExamPracticeResultId,
                    ParentExamPracticeSectionResultId = examPracticeSectionResult.Id,
                    Status = EnumResultStatus.Process
                });
            }
            await _examPracticeSectionResultRepository.BulkMergeAsync(createExamPracticeSectionResults, bulk =>
            {
                bulk.ColumnPrimaryKeyExpression = entity => new { entity.StudentId, entity.ExamPracticeSectionId, entity.ExamPracticeResultId };
            });
        }

        public async Task UpdateExamPracticeSectionResultsAsync(ExamPracticeResult examPracticeResult)
        {
            // Get all section results for the exam
            var examPracticeSectionResults = await _examPracticeSectionResultRepository.Queryable
                .Where(x => x.ExamPracticeResultId == examPracticeResult.Id)
                .ToListAsync();

            if (!examPracticeSectionResults.Any())
            {
                return;
            }
            // Get all answers for the exam, grouped by QuestionId for fast lookup
            var examPracticeAnswers = await _examPracticeAnswerRepository.Queryable
                .Where(x => x.ExamPracticeResultId == examPracticeResult.Id && x.QuestionId.HasValue)
                .AsNoTracking()
                .ToListAsync();

            var answersByQuestionId = examPracticeAnswers
                .GroupBy(x => x.QuestionId!.Value)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Get all questions for the sections, grouped by SectionId for fast lookup
            var examPracticeSectionIds = examPracticeSectionResults.Select(x => x.ExamPracticeSectionId).Distinct().ToList();

            var questions = await _questionRepository.Queryable
                .WhereBulkContains(examPracticeSectionIds, x => x.ExamPracticeSectionId)
                .AsNoTracking()
                .ToListAsync();

            var questionsBySectionId = questions
                .GroupBy(q => q.ExamPracticeSectionId)
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var sectionResult in examPracticeSectionResults)
            {
                var listQuestion = questionsBySectionId.TryGetValue(sectionResult.ExamPracticeSectionId, out var qs) ? qs : new List<Question>();
                var questionIds = listQuestion.Select(q => q.Id).ToHashSet();

                // Get answers for questions in this section
                var answers = questionIds.SelectMany(qid => answersByQuestionId.TryGetValue(qid, out var ans) ? ans : Enumerable.Empty<ExamPracticeAnswer>()).ToList();

                int countQuestion = 0;
                foreach (var answer in answers)
                {
                    var question = listQuestion.FirstOrDefault(x => x.Id == answer.QuestionId);
                    countQuestion += AnswerTypeHelper.GetTotalCorrectByAnswerType(question, answer?.Answer);
                }

                sectionResult.CorrectCount = answers.Sum(x => x.CorrectCount);
                sectionResult.CorrectTotal = listQuestion.Sum(x => x.CorrectTotal);
                sectionResult.Status = EnumResultStatus.Done;
            }

            _examPracticeSectionResultRepository.UpdateList(examPracticeSectionResults);
            await _examPracticeSectionResultRepository.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task UpdateAnswerProcessByTest(ExamPracticeSectionResult examPracticeSectionResult)
        {
            ArgumentNullException.ThrowIfNull(examPracticeSectionResult);
            var examPracticeAnswers = await _examPracticeAnswerRepository.Queryable.Where(x => x.ExamPracticeSectionResultId == examPracticeSectionResult.Id && x.Status == EnumAnswerStatus.Process)
                                                                              .Where(x => x.CreatedDate >= examPracticeSectionResult.CreatedDate)
                                                                              .ToListAsync();
            await _examPracticeAnswerRepository.BulkUpdateList(examPracticeAnswers.Select(x =>
            {
                x.Status = EnumAnswerStatus.Done;
                return x;
            }).ToList(), bulk =>
            {
                bulk.ColumnInputExpression = entity => new { entity.Status };
            });
        }

        private async Task<SkillScores> GetSkillScores(ExamPractice examPractice, ExamPracticeSection examPracticeSection, ExamPracticeSectionResult examPracticeSectionResult)
        {
            ArgumentNullException.ThrowIfNull(examPracticeSection);
            var maxTotalCorrect = 36;
            if (examPractice.Type == EnumExamPracticeType.Vstep)
            {
                maxTotalCorrect = examPracticeSection.CourseSkill == EnumCourseSkill.Writing ? 40 : 50;
            }
            int totalQuestion = default;
            var examPracticeAnswers = await _examPracticeAnswerRepository.Queryable.Include(x => x.Question)
                                                                         .Where(x => x.CreatedDate >= examPracticeSectionResult.CreatedDate)
                                                                         .Where(x => x.ExamPracticeSectionResultId == examPracticeSectionResult.Id).ToListAsync();

            var skillScore = GetSkillScore(examPractice, examPracticeSection, new List<BaseAnswer>(examPracticeAnswers));
            var countQuestion = 0;
            foreach (var item in examPracticeAnswers)
            {
                countQuestion += AnswerTypeHelper.GetTotalCorrectByAnswerType(item?.Question, item?.Answer);
            }
            skillScore.CountQuestion = countQuestion;

            if (examPracticeSection.CourseSkill == EnumCourseSkill.Listening || examPracticeSection.CourseSkill == EnumCourseSkill.Reading)
            {
                var questionIds = examPracticeAnswers.Where(x => x.QuestionId.HasValue).Select(x => x.QuestionId.GetValueOrDefault()).ToList();
                maxTotalCorrect = await _questionRepository.Queryable.WhereBulkContains(questionIds, x => x.Id).SumAsync(x => x.CorrectTotal);
                totalQuestion = maxTotalCorrect;
            }
            else if (examPracticeSection.CourseSkill == EnumCourseSkill.Writing || examPracticeSection.CourseSkill == EnumCourseSkill.Speaking)
            {
                totalQuestion = examPracticeAnswers.Select(x => x.ExamPracticeSectionId).Count();
            }
            skillScore.TotalCount = maxTotalCorrect;
            skillScore.TotalQuestion = totalQuestion;
            return skillScore;
        }

        #region BuildSkillScores

        public SkillScores GetSkillScore(
            ExamPractice examPractice,
            ExamPracticeSection examPracticeSection,
            IList<BaseAnswer>? baseAnswers)
        {
            ArgumentNullException.ThrowIfNull(examPracticeSection);
            ArgumentNullException.ThrowIfNull(examPractice);

            var skill = GetSkill(examPracticeSection);
            var correctCount = GetCorrectCount(baseAnswers);
            var score = CalculateScore(examPractice, skill, correctCount);

            return BuildSkillScores(skill, correctCount, baseAnswers, score);
        }

        private static EnumCourseSkill GetSkill(ExamPracticeSection examPracticeSection)
        {
            return examPracticeSection.CourseSkill ?? default;
        }

        private static int GetCorrectCount(IList<BaseAnswer>? baseAnswers)
        {
            return baseAnswers?.Sum(x => x.CorrectCount) ?? default;
        }

        private static double CalculateScore(ExamPractice examPractice, EnumCourseSkill skill, int correctCount)
        {
            if (examPractice.Type == EnumExamPracticeType.Vstep)
            {
                return GetBandScore(examPractice.Type, skill, correctCount);
            }
            else if (examPractice.Type == EnumExamPracticeType.IELTS)
            {
                return correctCount.GetIeltsScore(skill);
            }
            return default;
        }

        private static SkillScores BuildSkillScores(
            EnumCourseSkill skill,
            int correctCount,
            IList<BaseAnswer>? baseAnswers,
            double score)
        {
            return new SkillScores
            {
                CorrectCount = correctCount,
                CountQuestion = baseAnswers?.Count ?? default,
                Skill = skill,
                Scores = score
            };
        }

        #endregion BuildSkillScores

        private static double GetBandScore(EnumExamPracticeType type, EnumCourseSkill courseSkill, double correctCount)
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ResourceSettings.ExamScoringSkill);
            var examBandScores = ConvertHelper.DeserializeFromFilePath<IList<ExamBandScoreSkill>>(path);

            return examBandScores?.FirstOrDefault(x => x.Type == type && x.CourseSkill == courseSkill)?.BandScores
                                  .OrderByDescending(x => x.Score)
                                  .FirstOrDefault(x => correctCount >= x.Score)?.Band ?? 0;
        }

        public async Task<int> GetHighestStreak(ExamPracticeSection examPracticeSection, ExamPracticeSectionResult examPracticeSectionResult)
        {
            ArgumentNullException.ThrowIfNull(examPracticeSection);
            ArgumentNullException.ThrowIfNull(examPracticeSectionResult);
            var isHighestStreaks = new List<bool>();
            if (examPracticeSection.CourseSkill != EnumCourseSkill.Writing && examPracticeSection.CourseSkill != EnumCourseSkill.Speaking)
            {
                var mockTestAnswers = await _examPracticeAnswerRepository.Queryable.Where(x => x.CreatedDate >= examPracticeSectionResult.CreatedDate)
                                                                     .Where(x => x.ExamPracticeSectionResultId == examPracticeSectionResult.Id)
                                                                     .OrderBy(x => x.CreatedDate)
                                                                     .ToListAsync();

                isHighestStreaks = mockTestAnswers.Where(x => x.Answer != null).Select(x => x.Answer.Deserialize<MultipleChoiceAnswerV1>())
                                                     .Where(x => x != null && x.Answers != null)
                                                     .SelectMany(x => x!.Answers)
                                                     .Select(x => x.IsExact.HasValue && x.IsExact == true)
                                                     .ToList();
            }

            return LinQHelper.GetHighestStreak(isHighestStreaks);
        }

        public async Task<int> GetHighestStreak(ExamPracticeResult examPracticeResult, ExamPractice examPractice)
        {
            ArgumentNullException.ThrowIfNull(examPracticeResult);
            var isHighestStreaks = new List<bool>();
            var examPracticeAnswers = await _examPracticeAnswerRepository.Queryable.Where(x => x.CreatedDate >= examPracticeResult.CreatedDate)
                                                                     .Where(x => x.ExamPracticeResultId == examPracticeResult.Id)
                                                                     .OrderBy(x => x.CreatedDate)
                                                                     .ToListAsync();
            if (examPractice.Type == EnumExamPracticeType.IELTS)
            {
                isHighestStreaks = examPracticeAnswers.Where(x => x.Answer != null).Select(x => x.Answer.Deserialize<MultipleChoiceAnswerV1>())
                                              .Where(x => x != null && x.Answers != null)
                                              .SelectMany(x => x!.Answers)
                                              .Select(x => x.IsExact.HasValue && x.IsExact == true)
                                              .ToList();
            }
            else
            {
                isHighestStreaks = examPracticeAnswers.Select(x => x.IsCorrect.HasValue && x.IsCorrect == true)
                                              .ToList();
            }

            return LinQHelper.GetHighestStreak(isHighestStreaks);
        }

        public async Task UpdateExamPracticeSectionResultAsync(ExamPractice examPractice, ExamPracticeSection examPracticeSection, ExamPracticeSectionResult examPracticeSectionResult)
        {
            ArgumentNullException.ThrowIfNull(examPractice);
            ArgumentNullException.ThrowIfNull(examPracticeSection);
            ArgumentNullException.ThrowIfNull(examPracticeSectionResult);
            var skillScore = await GetSkillScores(examPractice, examPracticeSection, examPracticeSectionResult);
            examPracticeSectionResult.CorrectCount = (int)skillScore.CorrectCount;
            examPracticeSectionResult.CorrectTotal = (int)skillScore.TotalCount;
            examPracticeSectionResult.Status = EnumResultStatus.Done;
            examPracticeSectionResult.HighestStreak = await GetHighestStreak(examPracticeSection, examPracticeSectionResult);
            if (examPracticeSection.CourseSkill != EnumCourseSkill.Writing)
            {
                if (examPracticeSectionResult.SkillScores != null && examPracticeSectionResult.SkillScores.Any())
                {
                    examPracticeSectionResult.SkillScores.Add(skillScore);
                }
                else
                {
                    examPracticeSectionResult.SkillScores = new List<SkillScores> { skillScore };
                }
            }

            await _examPracticeSectionResultRepository.BulkUpdateList(new List<ExamPracticeSectionResult> { examPracticeSectionResult }, bulk =>
            {
                bulk.IgnoreOnUpdateExpression = entity => new
                {
                    entity.WorkingTime,
                    entity.ExamPracticeResultId,
                    entity.ExamPracticeSectionId
                };
            });
        }

        public async Task<IList<ExamPracticeSection>> GetExamPracticeSectionsAsync(ExamPracticeSection examPracticeSection, ExamPracticeSectionResult examPracticeSectionResult)
        {
            ArgumentNullException.ThrowIfNull(examPracticeSection);
            ArgumentNullException.ThrowIfNull(examPracticeSectionResult);
            var query = _examPracticeSectionRepository.Queryable;
            if (examPracticeSection.CourseSkill == EnumCourseSkill.Reading || examPracticeSection.CourseSkill == EnumCourseSkill.Listening)
            {
                query = query.Include(x => x.Questions);
            }
            else if (examPracticeSection.CourseSkill == EnumCourseSkill.Speaking)
            {
                query = query.Include(x => x.ExamPracticeSections);
            }
            return await query.Where(x => x.ParentExamPracticeSectionId == examPracticeSection.Id).OrderBy(x => x.DisplayOrder).ToListAsync();
        }

        public async Task<IList<Guid>?> GetUnansweredQuestionIds(ExamPracticeSection examPracticeSection, ExamPracticeSectionResult examPracticeSectionResult)
        {
            ArgumentNullException.ThrowIfNull(examPracticeSection);
            ArgumentNullException.ThrowIfNull(examPracticeSectionResult);
            var examPracticeSections = await GetExamPracticeSectionsAsync(examPracticeSection, examPracticeSectionResult);
            var examPracticeAnswers = await _examPracticeAnswerRepository.Queryable.Where(x => x.ExamPracticeSectionResultId == examPracticeSectionResult.Id).ToListAsync();
            if (examPracticeSection.CourseSkill == EnumCourseSkill.Reading || examPracticeSection.CourseSkill == EnumCourseSkill.Listening)
            {
                var questionIds = examPracticeSections.SelectMany(x => x.Questions).Select(x => x.Id);
                var questionCompleteIds = examPracticeAnswers.Where(x => x.QuestionId.HasValue).Select(x => x.QuestionId.GetValueOrDefault()).ToList();
                return questionIds.Except(questionCompleteIds).ToList();
            }
            else if (examPracticeSection.CourseSkill == EnumCourseSkill.Writing)
            {
                var examPracticeSectionIds = examPracticeAnswers.Where(x => x.ExamPracticeSectionId.HasValue).Select(x => x.ExamPracticeSectionId.GetValueOrDefault());
                return examPracticeSections.Select(x => x.Id).Except(examPracticeSectionIds).ToList();
            }
            else
            {
                var examPracticeSectionTimeCodes = examPracticeSections.SelectMany(x => x.ExamPracticeSections);
                var sectionTimeCodeCompleteIds = examPracticeAnswers.Where(x => x.ExamPracticeSectionId.HasValue).Select(x => x.ExamPracticeSectionId.GetValueOrDefault());
                return examPracticeSectionTimeCodes.Select(x => x.Id).Except(sectionTimeCodeCompleteIds).ToList();
            }
        }

        public async Task UpdateUnansweredQuestions(ExamPracticeSection examPracticeSection, ExamPracticeSectionResult examPracticeSectionResult)
        {
            ArgumentNullException.ThrowIfNull(examPracticeSection);
            var questionIds = await GetUnansweredQuestionIds(examPracticeSection, examPracticeSectionResult);
            if (questionIds == null || !questionIds.Any())
            {
                return;
            }
            var examPracticeAnswers = new List<ExamPracticeAnswer>();
            if (examPracticeSection.CourseSkill == EnumCourseSkill.Reading || examPracticeSection.CourseSkill == EnumCourseSkill.Listening)
            {
                var questions = await _questionRepository.Queryable.WhereBulkContains(questionIds, x => x.Id).Select(x => new
                {
                    QuestionId = x.Id,
                    QuestionType = x.QuestionType
                }).ToListAsync();
                examPracticeAnswers = questions.Select(x => new ExamPracticeAnswer
                {
                    Answer = AnswerTypeHelper.GetConfigEmpty(x.QuestionType),
                    ExamPracticeSectionResultId = examPracticeSectionResult.Id,
                    ExamPracticeResultId = examPracticeSectionResult.ExamPracticeResultId,
                    IsCorrect = null,
                    Status = EnumAnswerStatus.Done,
                    QuestionId = x.QuestionId
                }).ToList();
            }
            else
            {
                examPracticeAnswers = questionIds.Select(x => new ExamPracticeAnswer
                {
                    Answer = null,
                    ExamPracticeSectionResultId = examPracticeSectionResult.Id,
                    ExamPracticeResultId = examPracticeSectionResult.ExamPracticeResultId,
                    IsCorrect = null,
                    Status = EnumAnswerStatus.Done,
                    ExamPracticeSectionId = x,
                }).ToList();
            }

            try
            {
                await _examPracticeAnswerRepository.BulkMergeAsync(examPracticeAnswers, bulk =>
                {
                    bulk.ColumnPrimaryKeyExpression = c => new { c.ExamPracticeSectionId, c.ExamPracticeResultId, c.ExamPracticeSectionResultId, c.QuestionId };
                });
            }
            catch (Exception ex)
            {
            }
        }

        public async Task<IList<Guid>?> GetUnansweredQuestionIds(ExamPracticeResult examPracticeResult)
        {
            ArgumentNullException.ThrowIfNull(examPracticeResult);
            var examPracticeSections = await _examPracticeSectionRepository.Queryable.Include(x => x.Questions).Where(x => x.ExamPracticeId == examPracticeResult.ExamPracticeId)
                                                                           .OrderBy(x => x.DisplayOrder)
                                                                           .ToListAsync();
            var examPracticeAnswers = await _examPracticeAnswerRepository.Queryable.Where(x => x.ExamPracticeResultId == examPracticeResult.Id).ToListAsync();

            var questionIds = examPracticeSections.SelectMany(x => x.Questions).Select(x => x.Id);
            var questionCompleteIds = examPracticeAnswers.Where(x => x.QuestionId.HasValue).Select(x => x.QuestionId.GetValueOrDefault()).ToList();
            return questionIds.Except(questionCompleteIds).ToList();
        }

        public async Task UpdateUnansweredQuestions(ExamPracticeResult examPracticeResult)
        {
            ArgumentNullException.ThrowIfNull(examPracticeResult);
            var questionIds = await GetUnansweredQuestionIds(examPracticeResult);
            if (questionIds == null || !questionIds.Any())
            {
                return;
            }
            var examPracticeAnswers = new List<ExamPracticeAnswer>();
            var questions = await _questionRepository.Queryable.WhereBulkContains(questionIds, x => x.Id).Select(x => new
            {
                QuestionId = x.Id,
                QuestionType = x.QuestionType,
                ExamPracticeSectionId = x.ExamPracticeSectionId,
            }).ToListAsync();

            var examPracticeSectionResults = await _examPracticeSectionResultRepository.Queryable.WhereBulkContains(questions.Select(x => x.ExamPracticeSectionId), x => x.ExamPracticeSectionId)
                                                        .Where(x => x.ExamPracticeResultId == examPracticeResult.Id).ToListAsync();

            examPracticeAnswers = questions.Select(question =>
            {
                var examPracticeSectionResult = examPracticeSectionResults.FirstOrDefault(x => x.ExamPracticeSectionId == question.ExamPracticeSectionId);
                return new ExamPracticeAnswer
                {
                    Answer = AnswerTypeHelper.GetConfigEmpty(question.QuestionType),
                    ExamPracticeSectionResultId = examPracticeSectionResult?.Id,
                    ExamPracticeResultId = examPracticeResult.Id,
                    IsCorrect = null,
                    Status = EnumAnswerStatus.Done,
                    QuestionId = question.QuestionId
                };
            }).ToList();

            try
            {
                await _examPracticeAnswerRepository.BulkMergeAsync(examPracticeAnswers, bulk =>
                {
                    bulk.ColumnPrimaryKeyExpression = entity => new { entity.ExamPracticeSectionId, entity.QuestionId, entity.ExamPracticeResultId, entity.ExamPracticeSectionResultId };
                });
            }
            catch (Exception ex)
            {
            }
        }

        public async Task UpdateAnswerProcessByTest(ExamPracticeResult examPracticeResult)
        {
            ArgumentNullException.ThrowIfNull(examPracticeResult);
            var examPracticeAnswers = await _examPracticeAnswerRepository.Queryable.Where(x => x.ExamPracticeResultId == examPracticeResult.Id && x.Status == EnumAnswerStatus.Process)
                                                                              .Where(x => x.CreatedDate >= examPracticeResult.CreatedDate)
                                                                              .ToListAsync();
            await _examPracticeAnswerRepository.BulkUpdateList(examPracticeAnswers.Select(x =>
            {
                x.Status = EnumAnswerStatus.Done;
                return x;
            }).ToList(), bulk =>
            {
                bulk.ColumnInputExpression = entity => new { entity.Status };
            });
        }
    }
}
