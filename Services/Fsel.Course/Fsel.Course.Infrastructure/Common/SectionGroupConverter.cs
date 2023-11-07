// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Common
{
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Microsoft.EntityFrameworkCore;

    public class SectionGroupConverter
    {
        private readonly ISectionGroupRepository _sectionGroupRepository;
        private readonly ISectionRepository _sectionRepository;
        private readonly IFinalTestAnswerRepository _finalTestAnswerRepository;
        private readonly IPlacementTestAnswerRepository _placementTestAnswerRepository;
        private readonly IMockTestAnswerRepository _mockTestAnswerRepository;

        public SectionGroupConverter(ISectionGroupRepository sectionGroupRepository, ISectionRepository sectionRepository, IFinalTestAnswerRepository finalTestAnswerRepository, IPlacementTestAnswerRepository placementTestAnswerRepository, IMockTestAnswerRepository mockTestAnswerRepository)
        {
            _sectionGroupRepository = sectionGroupRepository;
            _sectionRepository = sectionRepository;
            _finalTestAnswerRepository = finalTestAnswerRepository;
            _placementTestAnswerRepository = placementTestAnswerRepository;
            _mockTestAnswerRepository = mockTestAnswerRepository;
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
                    PlacementTestResultId = sectionGroupResult.PlacementTestResultId ?? default
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
