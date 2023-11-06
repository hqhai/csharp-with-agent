// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Common
{
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Shared.Enums;
    using Microsoft.EntityFrameworkCore;

    public class SectionGroupConverter
    {
        private readonly ISectionGroupRepository _sectionGroupRepository;
        private readonly ISectionRepository _sectionRepository;

        public SectionGroupConverter(ISectionGroupRepository sectionGroupRepository, ISectionRepository sectionRepository)
        {
            _sectionGroupRepository = sectionGroupRepository;
            _sectionRepository = sectionRepository;
        }

        private async Task<(IList<Guid>?, IList<Guid>?, IList<Guid>?)> GetUnansweredQuestionIds(SectionGroup? sectionGroup, string? type)
        {
            if (string.IsNullOrEmpty(type) || sectionGroup == null)
            {
                return default;
            }
            var sections = await GetSectionsAsync(sectionGroup.Id, type, sectionGroup.CourseSkill);

            if (type == nameof(MockTest))
            {
                if (sectionGroup.CourseSkill == EnumCourseSkill.Reading || sectionGroup.CourseSkill == EnumCourseSkill.Listening)
                {
                    var sectionQuestions = sections.SelectMany(x => x.SectionParts).SelectMany(x => x.SectionQuestions).ToList();
                    var sectionQuestionCompleteIds = sectionQuestions.SelectMany(x => x.MockTestAnswers).Where(x => x.SectionQuestionId.HasValue).Select(x => x.SectionQuestionId!.Value).ToList();
                    return (sectionQuestionCompleteIds.Except(sectionQuestions.Select(x => x.Id)).ToList(), default, default);
                }
                else if (sectionGroup.CourseSkill == EnumCourseSkill.Writing)
                {
                    var sectionCompleteIds = sections.SelectMany(x => x.MockTestAnswers).Where(x => x.SectionId.HasValue).Select(x => x.SectionId!.Value).ToList();
                    return (default, sectionCompleteIds.Except(sections.Select(x => x.Id)).ToList(), default);
                }
                else
                {
                    var sectionTimeCodes = sections.SelectMany(x => x.SectionTimeCodes).ToList();
                    var sectionTimeCodeCompleteIds = sectionTimeCodes.SelectMany(x => x.MockTestAnswers).Where(x => x.SectionTimeCodeId.HasValue).Select(x => x.SectionTimeCodeId!.Value).ToList();
                    return (default, default, sectionTimeCodeCompleteIds.Except(sectionTimeCodes.Select(x => x.Id)).ToList());
                }
            }
            else if (type == nameof(FinalTest))
            {
                var sectionQuestions = sections.SelectMany(x => x.SectionQuestions).ToList();
                var sectionQuestionCompleteIds = sectionQuestions.SelectMany(x => x.FinalTestAnswers).Select(x => x.SectionQuestionId).ToList();
                return (sectionQuestionCompleteIds.Except(sectionQuestions.Select(x => x.Id)).ToList(), default, default);
            }
            else
            {
                var sectionQuestions = sections.SelectMany(x => x.SectionQuestions).ToList();
                var sectionQuestionCompleteIds = sectionQuestions.SelectMany(x => x.PlacementTestAnswers).Select(x => x.SectionQuestionId).ToList();
                return (sectionQuestionCompleteIds.Except(sectionQuestions.Select(x => x.Id)).ToList(), default, default);
            }
        }

        private async Task<IList<Section>> GetSectionsAsync(Guid sectionGroupId, string? type, EnumCourseSkill skill)
        {
            if (type == nameof(FinalTest))
            {
                return await _sectionRepository.Queryable.Include(x => x.SectionQuestions).ThenInclude(x => x.FinalTestAnswers)
                                                        .Where(x => x.SectionGroupId == sectionGroupId).OrderBy(x => x.DisplayOrder)
                                                        .ToListAsync();
            }
            else if (type == nameof(PlacementTest))
            {
                return await _sectionRepository.Queryable.Include(x => x.SectionQuestions).ThenInclude(x => x.PlacementTestAnswers)
                                                        .Where(x => x.SectionGroupId == sectionGroupId).OrderBy(x => x.DisplayOrder)
                                                        .ToListAsync();
            }
            else
            {
                if (skill == EnumCourseSkill.Reading || skill == EnumCourseSkill.Listening)
                {
                    return await _sectionRepository.Queryable.Include(x => x.SectionParts)
                                                                .ThenInclude(x => x.SectionQuestions)
                                                                .ThenInclude(x => x.MockTestAnswers)
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
                    return await _sectionRepository.Queryable.Include(x => x.SectionTimeCodes).ThenInclude(x => x.MockTestAnswers)
                                                                .Where(x => x.SectionGroupId == sectionGroupId).OrderBy(x => x.DisplayOrder)
                                                                .ToListAsync();
                }
            }
        }

        public async Task<IList<FinalTestAnswer>?> GetFinalTestAnswers(SectionGroup sectionGroup, SectionGroupResult sectionGroupResult)
        {
            var questionIds = await GetUnansweredQuestionIds(sectionGroup, nameof(FinalTest));
            if (questionIds.Item1 != null && questionIds.Item1.Any())
            {
                return questionIds.Item1.Select(x => new FinalTestAnswer
                {
                    Answer = null,
                    SectionQuestionId = x,
                    FinalTestResultId = sectionGroupResult.FinalTestResultId ?? default
                }).ToList();
            }
            return default;
        }

        public async Task<IList<PlacementTestAnswer>?> GetPlacementTestAnswers(SectionGroup sectionGroup, SectionGroupResult sectionGroupResult)
        {
            var questionIds = await GetUnansweredQuestionIds(sectionGroup, nameof(PlacementTest));
            if (questionIds.Item1 != null && questionIds.Item1.Any())
            {
                return questionIds.Item1.Select(x => new PlacementTestAnswer
                {
                    Answer = null,
                    SectionQuestionId = x,
                    PlacementTestResultId = sectionGroupResult.PlacementTestResultId ?? default
                }).ToList();
            }
            return default;
        }

        public async Task<IList<MockTestAnswer>?> GetMockTestAnswers(SectionGroup sectionGroup, SectionGroupResult sectionGroupResult)
        {
            var questionIds = await GetUnansweredQuestionIds(sectionGroup, nameof(MockTest));
            if (questionIds.Item1 != null && questionIds.Item1.Any())
            {
                return questionIds.Item1.Select(x => new MockTestAnswer
                {
                    Answer = null,
                    SectionQuestionId = x,
                    MockTestResultId = sectionGroupResult.MockTestResultId ?? default
                }).ToList();
            }
            else if (questionIds.Item2 != null && questionIds.Item2.Any())
            {
                return questionIds.Item2.Select(x => new MockTestAnswer
                {
                    Answer = null,
                    SectionId = x,
                    MockTestResultId = sectionGroupResult.MockTestResultId ?? default
                }).ToList();
            }
            else if (questionIds.Item3 != null && questionIds.Item3.Any())
            {
                return questionIds.Item3.Select(x => new MockTestAnswer
                {
                    Answer = null,
                    SectionTimeCodeId = x,
                    MockTestResultId = sectionGroupResult.MockTestResultId ?? default
                }).ToList();
            }
            return default;
        }
    }
}
