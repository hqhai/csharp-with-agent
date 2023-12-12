// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Common
{
    using AutoMapper;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Microsoft.EntityFrameworkCore;

    public class SectionGroupConverter
    {
        private readonly ISectionGroupRepository _sectionGroupRepository;
        private readonly ISectionGroupResultRepository _sectionGroupResultRepository;
        private readonly DateTimeConverter _dateTimeConverter;
        private readonly IQuestionRepository _questionRepository;
        private readonly LinQHelper _linQHelper;
        private readonly IMapper _mapper;
        private readonly ISectionRepository _sectionRepository;
        private readonly IFinalTestAnswerRepository _finalTestAnswerRepository;
        private readonly IPlacementTestAnswerRepository _placementTestAnswerRepository;
        private readonly IMockTestAnswerRepository _mockTestAnswerRepository;

        public SectionGroupConverter(ISectionGroupRepository sectionGroupRepository, ISectionGroupResultRepository sectionGroupResultRepository, DateTimeConverter dateTimeConverter, IQuestionRepository questionRepository, LinQHelper linQHelper, IMapper mapper, ISectionRepository sectionRepository, IFinalTestAnswerRepository finalTestAnswerRepository, IPlacementTestAnswerRepository placementTestAnswerRepository, IMockTestAnswerRepository mockTestAnswerRepository)
        {
            _sectionGroupRepository = sectionGroupRepository;
            _sectionGroupResultRepository = sectionGroupResultRepository;
            _dateTimeConverter = dateTimeConverter;
            _questionRepository = questionRepository;
            _linQHelper = linQHelper;
            _mapper = mapper;
            _sectionRepository = sectionRepository;
            _finalTestAnswerRepository = finalTestAnswerRepository;
            _placementTestAnswerRepository = placementTestAnswerRepository;
            _mockTestAnswerRepository = mockTestAnswerRepository;
        }

        public async Task<int> GetHighestStreak(SectionGroupResult sectionGroupResult, bool isMockTest = false)
        {
            if (isMockTest)
            {
                var mockTestAnswers = await _mockTestAnswerRepository.Queryable.Where(x => x.SectionGroupResultId == sectionGroupResult.Id)
                    .Include(x => x.SectionQuestion)
                    .OrderBy(x => x.CreatedDate)
                    .Select(x => x.IsCorrect == true).ToListAsync();
                return _linQHelper.GetHighestStreak(mockTestAnswers);
            }
            else
            {
                var finalTestAnswers = await _finalTestAnswerRepository.Queryable.Where(x => x.SectionGroupResultId == sectionGroupResult.Id)
                    .Include(x => x.SectionQuestion)
                    .OrderBy(x => x.CreatedDate)
                    .Select(x => x.IsCorrect == true).ToListAsync();
                return _linQHelper.GetHighestStreak(finalTestAnswers);
            }
        }

        private async Task<SkillScores> GetSkillScoreFinalTest(SectionGroupResult sectionGroupResult, SectionGroup sectionGroup, CancellationToken cancellationToken)
        {
            var finalTestAnswers = await _finalTestAnswerRepository.Queryable.Include(x => x.SectionQuestion).Where(x => x.SectionGroupResultId == sectionGroupResult.Id && x.FinalTestResultId == sectionGroupResult.FinalTestResultId).ToListAsync(cancellationToken);
            var questionIds = finalTestAnswers.Select(x => x.SectionQuestion).Select(x => x!.QuestionId).ToList();
            var totalCorrect = await _questionRepository.Queryable.Where(x => questionIds.Contains(x.Id)).SumAsync(x => x.CorrectTotal, cancellationToken);
            var skillScore = GetSkillScore(sectionGroup, finalTestAnswers.Sum(x => x.CorrectCount), finalTestAnswers.Count, totalCorrect, questionIds.Count);
            return skillScore;
        }

        public async Task<SkillScores> GetSkillScoreMockTest(SectionGroupResult sectionGroupResult, SectionGroup sectionGroup, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(sectionGroup);
            var mockTestAnswers = await _mockTestAnswerRepository.Queryable.Include(x => x.SectionQuestion).Where(x => x.SectionGroupResultId == sectionGroupResult.Id && x.MockTestResultId == sectionGroupResult.MockTestResultId).ToListAsync(cancellationToken);
            var skillScore = new SkillScores();
            var maxTotalCorrect = 36;
            if (sectionGroup.CourseSkill == EnumCourseSkill.Listening || sectionGroup.CourseSkill == EnumCourseSkill.Reading)
            {
                var questionIds = mockTestAnswers.Select(x => x.SectionQuestion).Select(x => x.QuestionId).ToList();
                var totalCorrect = await _questionRepository.Queryable.Where(x => questionIds.Contains(x.Id)).SumAsync(x => x.CorrectTotal, cancellationToken);
                skillScore = GetSkillScore(sectionGroup, mockTestAnswers.Sum(x => x.CorrectCount), mockTestAnswers.Count, totalCorrect, questionIds.Count);
            }
            else if (sectionGroup.CourseSkill == EnumCourseSkill.Writing)
            {
                var sectionIds = mockTestAnswers.Select(x => x.SectionId).ToList();
                skillScore = GetSkillScore(sectionGroup, mockTestAnswers.Sum(x => x.CorrectCount), mockTestAnswers.Count, maxTotalCorrect, sectionIds.Count);
            }
            else if (sectionGroup.CourseSkill == EnumCourseSkill.Speaking)
            {
                var sectionTimeCodeIds = mockTestAnswers.Select(x => x.SectionTimeCodeId).ToList();
                skillScore = GetSkillScore(sectionGroup, mockTestAnswers.Sum(x => x.CorrectCount), mockTestAnswers.Count, maxTotalCorrect, sectionTimeCodeIds.Count);
            }
            return skillScore;
        }

        public async Task<SectionGroupResult> UpdateSectionGroupResultAsync(SectionGroupResult sectionGroupResult, SectionGroup sectionGroup, bool isMockTest, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(sectionGroupResult);
            ArgumentNullException.ThrowIfNull(sectionGroup);
            var skillScore = isMockTest ? await GetSkillScoreMockTest(sectionGroupResult, sectionGroup, cancellationToken) : await GetSkillScoreFinalTest(sectionGroupResult, sectionGroup, cancellationToken);
            sectionGroupResult.CorrectCount = (int)skillScore.CorrectCount;
            sectionGroupResult.CorrectTotal = (int)skillScore.TotalCount;
            sectionGroupResult.Status = EnumResultStatus.Done;
            sectionGroupResult.HighestStreak = await GetHighestStreak(sectionGroupResult);
            sectionGroupResult.WorkingTime = _dateTimeConverter.GetWorkingTime(sectionGroupResult.CreatedDate, DateTime.UtcNow, sectionGroup.ExecutionTime);
            if (sectionGroupResult.SkillScores != null && sectionGroupResult.SkillScores.Any())
            {
                sectionGroupResult.SkillScores.Add(skillScore);
            }
            else
            {
                sectionGroupResult.SkillScores = new List<SkillScores> { skillScore };
            }
            _sectionGroupResultRepository.Update(sectionGroupResult);
            await _sectionGroupResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            return sectionGroupResult;
        }

        private async Task<(IList<Guid>?, IList<Guid>?, IList<Guid>?)> GetUnansweredQuestionIds(SectionGroup? sectionGroup, string? type, SectionGroupResult sectionGroupResult)

        {
            if (string.IsNullOrEmpty(type) || sectionGroup == null)
            {
                return default;
            }
            var sections = await GetSectionsAsync(sectionGroup.Id, type, sectionGroup.CourseSkill, sectionGroupResult);

            if (type == nameof(MockTest))
            {
                if (sectionGroup.CourseSkill == EnumCourseSkill.Reading || sectionGroup.CourseSkill == EnumCourseSkill.Listening)
                {
                    var sectionQuestions = sections.SelectMany(x => x.SectionParts).SelectMany(x => x.SectionQuestions).ToList();
                    var sectionQuestionCompleteIds = sectionQuestions.SelectMany(x => x.MockTestAnswers).Where(x => x.SectionQuestionId.HasValue).Select(x => x.SectionQuestionId!.Value).ToList();
                    return (sectionQuestions.Select(x => x.Id).Except(sectionQuestionCompleteIds).ToList(), default, default);
                }
                else if (sectionGroup.CourseSkill == EnumCourseSkill.Writing)
                {
                    var sectionCompleteIds = sections.SelectMany(x => x.MockTestAnswers).Where(x => x.SectionId.HasValue).Select(x => x.SectionId!.Value).ToList();
                    return (default, sections.Select(x => x.Id).Except(sectionCompleteIds).ToList(), default);
                }
                else
                {
                    var sectionTimeCodes = sections.SelectMany(x => x.SectionTimeCodes).ToList();
                    var sectionTimeCodeCompleteIds = sectionTimeCodes.SelectMany(x => x.MockTestAnswers).Where(x => x.SectionTimeCodeId.HasValue).Select(x => x.SectionTimeCodeId!.Value).ToList();
                    return (default, default, sectionTimeCodes.Select(x => x.Id).Except(sectionTimeCodeCompleteIds).ToList());
                }
            }
            else if (type == nameof(FinalTest))
            {
                var sectionQuestions = sections.SelectMany(x => x.SectionQuestions).ToList();
                var sectionQuestionCompleteIds = sectionQuestions.SelectMany(x => x.FinalTestAnswers).Select(x => x.SectionQuestionId).ToList();
                return (sectionQuestions.Select(x => x.Id).Except(sectionQuestionCompleteIds).ToList(), default, default);
            }
            else
            {
                var sectionQuestions = sections.SelectMany(x => x.SectionQuestions).ToList();
                var sectionQuestionCompleteIds = sectionQuestions.SelectMany(x => x.PlacementTestAnswers).Select(x => x.SectionQuestionId).ToList();
                return (sectionQuestions.Select(x => x.Id).Except(sectionQuestionCompleteIds).ToList(), default, default);
            }
        }

        private async Task<IList<Section>> GetSectionsAsync(Guid sectionGroupId, string? type, EnumCourseSkill skill, SectionGroupResult sectionGroupResult)
        {
            if (type == nameof(FinalTest))
            {
                return await _sectionRepository.Queryable.Include(x => x.SectionQuestions).ThenInclude(x => x.FinalTestAnswers.Where(x => x.SectionGroupResultId == sectionGroupResult.Id))
                                                        .Where(x => x.SectionGroupId == sectionGroupId).OrderBy(x => x.DisplayOrder)
                                                        .ToListAsync();
            }
            else if (type == nameof(PlacementTest))
            {
                return await _sectionRepository.Queryable.Include(x => x.SectionQuestions).ThenInclude(x => x.PlacementTestAnswers.Where(x => x.SectionGroupResultId == sectionGroupResult.Id))
                                                        .Where(x => x.SectionGroupId == sectionGroupId).OrderBy(x => x.DisplayOrder)
                                                        .ToListAsync();
            }
            else
            {
                if (skill == EnumCourseSkill.Reading || skill == EnumCourseSkill.Listening)
                {
                    return await _sectionRepository.Queryable.Include(x => x.SectionParts)
                                                                .ThenInclude(x => x.SectionQuestions)
                                                                .ThenInclude(x => x.MockTestAnswers.Where(x => x.SectionGroupResultId == sectionGroupResult.Id))
                                                                .Where(x => x.SectionGroupId == sectionGroupId).OrderBy(x => x.DisplayOrder)
                                                                .ToListAsync();
                }
                else if (skill == EnumCourseSkill.Writing)
                {
                    return await _sectionRepository.Queryable.Include(x => x.MockTestAnswers).Where(x => x.SectionGroupId == sectionGroupId).OrderBy(x => x.DisplayOrder)
                                                                        .ToListAsync();
                }
                else
                {
                    return await _sectionRepository.Queryable.Include(x => x.SectionTimeCodes).ThenInclude(x => x.MockTestAnswers.Where(x => x.SectionGroupResultId == sectionGroupResult.Id))
                                                                .Where(x => x.SectionGroupId == sectionGroupId).OrderBy(x => x.DisplayOrder)
                                                                .ToListAsync();
                }
            }
        }

        public async Task UpdateFinalTestAnswers(SectionGroup sectionGroup, SectionGroupResult sectionGroupResult)
        {
            var questionIds = await GetUnansweredQuestionIds(sectionGroup, nameof(FinalTest), sectionGroupResult);
            if (questionIds.Item1 != null && questionIds.Item1.Any())
            {
                await _finalTestAnswerRepository.AddList(questionIds.Item1.Select(x => new FinalTestAnswer
                {
                    Answer = null,
                    SectionQuestionId = x,
                    SectionGroupResultId = sectionGroupResult.Id,
                    FinalTestResultId = sectionGroupResult.FinalTestResultId ?? default
                }).ToList());
                await _finalTestAnswerRepository.UnitOfWork.SaveEntitiesAsync().ConfigureAwait(false);
            }
        }

        public async Task UpdatePlacementTestAnswers(SectionGroup sectionGroup, SectionGroupResult sectionGroupResult)
        {
            var questionIds = await GetUnansweredQuestionIds(sectionGroup, nameof(PlacementTest), sectionGroupResult);
            if (questionIds.Item1 != null && questionIds.Item1.Any())
            {
                await _placementTestAnswerRepository.AddList(questionIds.Item1.Select(x => new PlacementTestAnswer
                {
                    Answer = null,
                    SectionQuestionId = x,
                    SectionGroupResultId = sectionGroupResult.Id,
                    PlacementTestResultId = sectionGroupResult.PlacementTestResultId ?? default,
                    IsCorrect = null
                }).ToList());
                await _placementTestAnswerRepository.UnitOfWork.SaveEntitiesAsync().ConfigureAwait(false);
            }
        }

        public SkillScores GetSkillScore(SectionGroup sectionGroup, int correctCount, int countQuestion, int totalCount, int totalQuestion)
        {
            ArgumentNullException.ThrowIfNull(sectionGroup);
            return new SkillScores
            {
                CorrectCount = correctCount,
                CountQuestion = countQuestion,
                Skill = sectionGroup.CourseSkill,
                TotalCount = totalCount,
                TotalQuestion = totalQuestion,
                Scores = correctCount.GetIeltsScore(sectionGroup.CourseSkill)
            };
        }

        public async Task UpdateMockTestAnswers(SectionGroup sectionGroup, SectionGroupResult sectionGroupResult)
        {
            var questionIds = await GetUnansweredQuestionIds(sectionGroup, nameof(MockTest), sectionGroupResult);
            var mockTestAnswers = new List<MockTestAnswer>();
            if (questionIds.Item1 != null && questionIds.Item1.Any())
            {
                mockTestAnswers = questionIds.Item1.Select(x => new MockTestAnswer
                {
                    Answer = null,
                    SectionQuestionId = x,
                    SectionGroupResultId = sectionGroupResult.Id,
                    MockTestResultId = sectionGroupResult.MockTestResultId ?? default
                }).ToList();
            }
            else if (questionIds.Item2 != null && questionIds.Item2.Any())
            {
                mockTestAnswers = questionIds.Item2.Select(x => new MockTestAnswer
                {
                    Answer = null,
                    SectionId = x,
                    SectionGroupResultId = sectionGroupResult.Id,
                    MockTestResultId = sectionGroupResult.MockTestResultId ?? default
                }).ToList();
            }
            else if (questionIds.Item3 != null && questionIds.Item3.Any())
            {
                mockTestAnswers = questionIds.Item3.Select(x => new MockTestAnswer
                {
                    Answer = null,
                    SectionTimeCodeId = x,
                    SectionGroupResultId = sectionGroupResult.Id,
                    MockTestResultId = sectionGroupResult.MockTestResultId ?? default
                }).ToList();
            }
            await _mockTestAnswerRepository.AddList(mockTestAnswers);
            await _mockTestAnswerRepository.UnitOfWork.SaveEntitiesAsync().ConfigureAwait(false);
        }
    }
}
