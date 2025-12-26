// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Common
{
    using AutoMapper;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.QuestionTypeConfigs.Answers.V1i1;
    using Fsel.Course.Domain.Entities.QuestionTypeConfigs.Questions.V1i1;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    public class SectionGroupConverter
    {
        private readonly ISectionGroupResultRepository _sectionGroupResultRepository;
        private readonly ILogger<SectionGroupConverter> _logger;
        private readonly IStudentFeedbackRepository _studentFeedbackRepository;
        private readonly AnswerTypeConverter _answerTypeConverter;
        private readonly ISectionQuestionRepository _sectionQuestionRepository;
        private readonly QuestionTypeConverter _questionTypeConverter;
        private readonly IMapper _mapper;
        private readonly IQuestionRepository _questionRepository;
        private readonly ISectionRepository _sectionRepository;
        private readonly IFinalTestAnswerRepository _finalTestAnswerRepository;
        private readonly IPlacementTestAnswerRepository _placementTestAnswerRepository;
        private readonly IMockTestAnswerRepository _mockTestAnswerRepository;

        #region Clean Code

        public SectionGroupConverter(ISectionGroupResultRepository sectionGroupResultRepository,
            ILogger<SectionGroupConverter> logger,
            IStudentFeedbackRepository studentFeedbackRepository,
            AnswerTypeConverter answerTypeConverter,
            ISectionQuestionRepository sectionQuestionRepository,
            QuestionTypeConverter questionTypeConverter,
            IMapper mapper,
            IQuestionRepository questionRepository,
            ISectionRepository sectionRepository,
            IFinalTestAnswerRepository finalTestAnswerRepository,
            IPlacementTestAnswerRepository placementTestAnswerRepository,
            IMockTestAnswerRepository mockTestAnswerRepository)
        {
            _sectionGroupResultRepository = sectionGroupResultRepository;
            _logger = logger;
            _studentFeedbackRepository = studentFeedbackRepository;
            _answerTypeConverter = answerTypeConverter;
            _sectionQuestionRepository = sectionQuestionRepository;
            _questionTypeConverter = questionTypeConverter;
            _mapper = mapper;
            _questionRepository = questionRepository;
            _sectionRepository = sectionRepository;
            _finalTestAnswerRepository = finalTestAnswerRepository;
            _placementTestAnswerRepository = placementTestAnswerRepository;
            _mockTestAnswerRepository = mockTestAnswerRepository;
        }

        public async Task<int> GetHighestStreak(SectionGroupResult sectionGroupResult, SectionGroup sectionGroup, double version = (int)EnumVersion.V1)
        {
            ArgumentNullException.ThrowIfNull(sectionGroup);
            ArgumentNullException.ThrowIfNull(sectionGroupResult);
            var isHighestStreaks = new List<bool>();

            if (sectionGroupResult.MockTestResultId.HasValue && sectionGroup.CourseSkill != EnumCourseSkill.Writing && sectionGroup.CourseSkill != EnumCourseSkill.Speaking)
            {
                var mockTestAnswers = await _mockTestAnswerRepository.Queryable.Where(x => x.CreatedDate >= sectionGroupResult.CreatedDate)
                                                                     .Where(x => x.SectionGroupResultId == sectionGroupResult.Id)
                                                                     .OrderBy(x => x.CreatedDate)
                                                                     .ToListAsync();
                if (version == (int)EnumVersion.V1)
                {
                    isHighestStreaks = mockTestAnswers.Select(x => x.IsCorrect == true).ToList();
                }
                else if (version == (int)EnumVersion.V2)
                {
                    isHighestStreaks = mockTestAnswers.Where(x => x.Answer != null).Select(x => x.Answer.Deserialize<MultipleChoiceAnswerV1>())
                                                      .Where(x => x != null && x.Answers != null)
                                                      .SelectMany(x => x!.Answers)
                                                      .Select(x => x.IsExact.HasValue && x.IsExact == true)
                                                      .ToList();
                }
            }
            else if (sectionGroupResult.FinalTestResultId.HasValue)
            {
                isHighestStreaks = await _finalTestAnswerRepository.Queryable.Where(x => x.CreatedDate >= sectionGroupResult.CreatedDate)
                                                                   .Where(x => x.SectionGroupResultId == sectionGroupResult.Id)
                                                                   .Select(x => x.IsCorrect == true).ToListAsync();
            }
            return isHighestStreaks.GetHighestStreak();
        }

        public async Task<SectionGroupResult> UpdateSectionGroupResultAsync(SectionGroupResult sectionGroupResult, SectionGroup sectionGroup, double version = (int)EnumVersion.V1)
        {
            ArgumentNullException.ThrowIfNull(sectionGroup);
            ArgumentNullException.ThrowIfNull(sectionGroupResult);
            var skillScore = await GetSkillScore(sectionGroup, sectionGroupResult, version);
            sectionGroupResult.CorrectCount = (int)skillScore.CorrectCount;
            sectionGroupResult.CorrectTotal = (int)skillScore.TotalCount;
            sectionGroupResult.Status = EnumResultStatus.Done;
            sectionGroupResult.HighestStreak = await GetHighestStreak(sectionGroupResult, sectionGroup, version);
            if (sectionGroup.CourseSkill != EnumCourseSkill.Writing)
            {
                if (sectionGroupResult.SkillScores != null && sectionGroupResult.SkillScores.Any())
                {
                    sectionGroupResult.SkillScores.Add(skillScore);
                }
                else
                {
                    sectionGroupResult.SkillScores = new List<SkillScores> { skillScore };
                }
            }

            await _sectionGroupResultRepository.BulkUpdateList(new List<SectionGroupResult> { sectionGroupResult }, bulk =>
            {
                bulk.IgnoreOnUpdateExpression = entity => new
                {
                    entity.WorkingTime,
                    entity.SectionGroupId,
                    entity.PlacementTestResultId,
                    entity.MockTestResultId,
                    entity.FinalTestResultId
                };
            });
            return sectionGroupResult;
        }

        private async Task<SkillScores> GetSkillScore(SectionGroup sectionGroup, SectionGroupResult sectionGroupResult, double version = (int)EnumVersion.V1)
        {
            if (sectionGroupResult.MockTestResultId.HasValue)
            {
                return await GetSkillScoreMockTest(sectionGroup, sectionGroupResult, version);
            }
            else if (sectionGroupResult.FinalTestResultId.HasValue)
            {
                var finalTestAnswers = await _finalTestAnswerRepository.Queryable.Include(x => x.SectionQuestion)
                                                                       .Where(x => x.CreatedDate >= sectionGroupResult.CreatedDate)
                                                                       .Where(x => x.SectionGroupResultId == sectionGroupResult.Id).ToListAsync();
                return await GetSkillScore(GetSkillScore(sectionGroup, new List<BaseAnswer>(finalTestAnswers)), finalTestAnswers.Select(x => x.SectionQuestion).Select(x => x!.QuestionId).ToList());
            }
            else
            {
                var placementTestAnswers = await _placementTestAnswerRepository.Queryable.Include(x => x.SectionQuestion)
                                                                               .Where(x => x.CreatedDate >= sectionGroupResult.CreatedDate)
                                                                               .Where(x => x.SectionGroupResultId == sectionGroupResult.Id).ToListAsync();
                return await GetSkillScore(GetSkillScore(sectionGroup, new List<BaseAnswer>(placementTestAnswers)), placementTestAnswers.Select(x => x.SectionQuestion).Select(x => x!.QuestionId).ToList());
            }
        }

        private async Task<SkillScores> GetSkillScore(SkillScores skillScore, IList<Guid?>? ids)
        {
            if (ids != null && ids.Any())
            {
                skillScore.TotalCount = await _questionRepository.Queryable.WhereBulkContains(ids, x => x.Id).SumAsync(x => x.CorrectTotal);
                skillScore.TotalQuestion = ids.Count;
            }
            return skillScore;
        }

        private async Task<SkillScores> GetSkillScoreMockTest(SectionGroup sectionGroup, SectionGroupResult sectionGroupResult, double version = (int)EnumVersion.V1)
        {
            ArgumentNullException.ThrowIfNull(sectionGroup);
            var maxTotalCorrect = 36;
            int totalQuestion = default;

            var mockTestAnswers = await _mockTestAnswerRepository.Queryable.Include(x => x.SectionQuestion)
                .ThenInclude(x => x.Question)
                .Where(x => x.CreatedDate >= sectionGroupResult.CreatedDate)
                .Where(x => x.SectionGroupResultId == sectionGroupResult.Id).ToListAsync();
            var skillScore = GetSkillScore(sectionGroup, new List<BaseAnswer>(mockTestAnswers), version);
            if (version != (int)EnumVersion.V1)
            {
                var countQuestion = 0;
                foreach (var item in mockTestAnswers)
                {
                    var question = item.SectionQuestion?.Question;
                    countQuestion += _answerTypeConverter.GetTotalCorrectByAnswerType(question, item.Answer);
                }
                skillScore.CountQuestion = countQuestion;
            }

            if (sectionGroup.CourseSkill == EnumCourseSkill.Listening || sectionGroup.CourseSkill == EnumCourseSkill.Reading)
            {
                var questionIds = mockTestAnswers.Select(x => x.SectionQuestion).Select(x => x!.QuestionId).ToList();
                maxTotalCorrect = await _questionRepository.Queryable.Where(x => questionIds.Contains(x.Id)).SumAsync(x => x.CorrectTotal);
                totalQuestion = version == (int)EnumVersion.V1 ? questionIds.Count : maxTotalCorrect;
            }
            else if (sectionGroup.CourseSkill == EnumCourseSkill.Writing)
            {
                totalQuestion = mockTestAnswers.Select(x => x.SectionId).Count();
            }
            else if (sectionGroup.CourseSkill == EnumCourseSkill.Speaking)
            {
                totalQuestion = mockTestAnswers.Select(x => x.SectionTimeCodeId).Count();
            }

            skillScore.TotalCount = maxTotalCorrect;
            skillScore.TotalQuestion = totalQuestion;
            return skillScore;
        }

        public SkillScores GetSkillScore(SectionGroup sectionGroup, IList<BaseAnswer>? baseAnswers, double version = (int)EnumVersion.V1)
        {
            ArgumentNullException.ThrowIfNull(sectionGroup);
            return new SkillScores
            {
                CorrectCount = baseAnswers?.Sum(x => x.CorrectCount) ?? default,
                CountQuestion = baseAnswers?.Count ?? default,
                Skill = sectionGroup.CourseSkill,
                Scores = baseAnswers?.Sum(x => x.CorrectCount).GetIeltsScore(sectionGroup.CourseSkill) ?? default
            };
        }

        public async Task<IList<Section>> GetSectionsAsync(SectionGroup sectionGroup, SectionGroupResult sectionGroupResult, double version = (int)EnumVersion.V1)
        {
            ArgumentNullException.ThrowIfNull(sectionGroup);
            ArgumentNullException.ThrowIfNull(sectionGroupResult);
            var query = _sectionRepository.Queryable;
            if (sectionGroupResult.MockTestResultId.HasValue)
            {
                if (sectionGroup.CourseSkill == EnumCourseSkill.Reading || sectionGroup.CourseSkill == EnumCourseSkill.Listening)
                {
                    if (version == (int)EnumVersion.V1)
                    {
                        query = query.Include(x => x.SectionParts).ThenInclude(x => x.SectionQuestions).ThenInclude(x => x.MockTestAnswers.Where(x => x.SectionGroupResultId == sectionGroupResult.Id))
                                     .Include(x => x.SectionParts).ThenInclude(x => x.SectionQuestions).ThenInclude(x => x.Question);
                    }
                    else
                    {
                        query = query.Include(x => x.SectionQuestions).ThenInclude(x => x.MockTestAnswers.Where(x => x.SectionGroupResultId == sectionGroupResult.Id))
                                     .Include(x => x.SectionQuestions).ThenInclude(x => x.Question);
                    }
                }
                else if (sectionGroup.CourseSkill == EnumCourseSkill.Writing)
                {
                    query = query.Include(x => x.MockTestAnswers.Where(x => x.SectionGroupResultId == sectionGroupResult.Id));
                }
                else
                {
                    query = query.Include(x => x.SectionTimeCodes).ThenInclude(x => x.MockTestAnswers.Where(x => x.SectionGroupResultId == sectionGroupResult.Id));
                }
            }
            else if (sectionGroupResult.FinalTestResultId.HasValue)
            {
                query = query.Include(x => x.SectionQuestions).ThenInclude(x => x.FinalTestAnswers.Where(x => x.SectionGroupResultId == sectionGroupResult.Id));
            }
            else
            {
                query = query.Include(x => x.SectionQuestions).ThenInclude(x => x.PlacementTestAnswers.Where(x => x.SectionGroupResultId == sectionGroupResult.Id));
            }
            return await query.Where(x => x.SectionGroupId == sectionGroup.Id).OrderBy(x => x.DisplayOrder).ToListAsync();
        }

        public async Task<(IList<Section>, long)> GetSectionsAndTotalQuestionAsync(SectionGroup sectionGroup, SectionGroupResult sectionGroupResult, double version = (int)EnumVersion.V1)
        {
            var sections = await GetSectionsAsync(sectionGroup, sectionGroupResult, version);
            return (sections, GetTotalQuestion(sections, sectionGroup.CourseSkill));
        }

        private static long GetTotalQuestion(Section section, EnumCourseSkill courseSkill)
        {
            ArgumentNullException.ThrowIfNull(section);
            if (courseSkill == EnumCourseSkill.Speaking)
            {
                return section.SectionTimeCodes.Count;
            }
            else if (courseSkill == EnumCourseSkill.Writing)
            {
                return 1;
            }
            if (section.SectionParts.Any())
            {
                return section.SectionParts.SelectMany(x => x.SectionQuestions).Count();
            }
            if (section.SectionQuestions.Select(x => x.Question).Any() && section.SectionQuestions.Select(x => x.Question).Any(x => x != null && x.SubQuestionIndexs != null && x.SubQuestionIndexs.Any()))
            {
                return section.SectionQuestions.Where(x => x.Question != null).Select(x => x.Question!.SubQuestionNumber).Sum();
            }
            else
            {
                return section.SectionQuestions.Count;
            }
        }

        private static IList<Guid>? GetUnansweredMockTests(IList<Section>? sections, SectionGroup sectionGroup)
        {
            ArgumentNullException.ThrowIfNull(sections);
            if (sectionGroup.CourseSkill == EnumCourseSkill.Reading || sectionGroup.CourseSkill == EnumCourseSkill.Listening)
            {
                var sectionQuestions = new List<SectionQuestion>();
                var sectionParts = sections.SelectMany(x => x.SectionParts);
                if (sectionParts.Any())
                {
                    sectionQuestions = sections.SelectMany(x => x.SectionParts).SelectMany(x => x.SectionQuestions).ToList();
                }
                else
                {
                    sectionQuestions = sections.SelectMany(x => x.SectionQuestions).ToList();
                }
                var sectionQuestionCompleteIds = sectionQuestions.SelectMany(x => x.MockTestAnswers).Where(x => x.SectionQuestionId.HasValue).Select(x => x.SectionQuestionId!.Value).ToList();
                return sectionQuestions.Select(x => x.Id).Except(sectionQuestionCompleteIds).ToList();
            }
            else if (sectionGroup.CourseSkill == EnumCourseSkill.Writing)
            {
                var sectionCompleteIds = sections.SelectMany(x => x.MockTestAnswers).Where(x => x.SectionId.HasValue).Select(x => x.SectionId!.Value);
                return sections.Select(x => x.Id).Except(sectionCompleteIds).ToList();
            }
            else
            {
                var sectionTimeCodes = sections.SelectMany(x => x.SectionTimeCodes);
                var sectionTimeCodeCompleteIds = sectionTimeCodes.SelectMany(x => x.MockTestAnswers).Where(x => x.SectionTimeCodeId.HasValue).Select(x => x.SectionTimeCodeId!.Value).ToList();
                return sectionTimeCodes.Select(x => x.Id).Except(sectionTimeCodeCompleteIds).ToList();
            }
        }

        public async Task<IList<Guid>?> GetUnansweredQuestionIds(SectionGroup sectionGroup, SectionGroupResult sectionGroupResult, double version = (int)EnumVersion.V1)
        {
            ArgumentNullException.ThrowIfNull(sectionGroup);
            ArgumentNullException.ThrowIfNull(sectionGroupResult);
            var sections = await GetSectionsAsync(sectionGroup, sectionGroupResult, version);
            if (sectionGroupResult.MockTestResultId.HasValue)
            {
                return GetUnansweredMockTests(sections, sectionGroup);
            }
            else
            {
                var sectionQuestions = sections.SelectMany(x => x.SectionQuestions);
                var sectionQuestionCompleteIds = new List<Guid>();
                if (sectionGroupResult.FinalTestResultId.HasValue)
                {
                    sectionQuestionCompleteIds = sectionQuestions.SelectMany(x => x.FinalTestAnswers).Select(x => x.SectionQuestionId).ToList();
                }
                else
                {
                    sectionQuestionCompleteIds = sectionQuestions.SelectMany(x => x.PlacementTestAnswers).Select(x => x.SectionQuestionId).ToList();
                }
                return sectionQuestions.Select(x => x.Id).Except(sectionQuestionCompleteIds).ToList();
            }
        }

        public async Task UpdateUnansweredQuestions(SectionGroup sectionGroup, SectionGroupResult sectionGroupResult, double version = (int)EnumVersion.V1)
        {
            var questionIds = await GetUnansweredQuestionIds(sectionGroup, sectionGroupResult, version);
            if (questionIds == null || !questionIds.Any())
            {
                return;
            }
            if (sectionGroupResult.MockTestResultId.HasValue)
            {
                var mockTestAnswers = new List<MockTestAnswer>();
                if (sectionGroup.CourseSkill == EnumCourseSkill.Reading || sectionGroup.CourseSkill == EnumCourseSkill.Listening)
                {
                    var sectionQuestions = await _sectionQuestionRepository.Queryable.WhereBulkContains(questionIds, x => x.Id).Select(x => new
                    {
                        SectionQuestionId = x.Id,
                        QuestionType = x.Question!.QuestionType
                    }).ToListAsync();
                    mockTestAnswers = sectionQuestions.Select(x => new MockTestAnswer
                    {
                        Answer = _answerTypeConverter.GetConfigEmpty(x.QuestionType),
                        SectionGroupResultId = sectionGroupResult.Id,
                        MockTestResultId = sectionGroupResult.MockTestResultId ?? default,
                        IsCorrect = null,
                        Status = EnumAnswerStatus.Done,
                        SectionQuestionId = x.SectionQuestionId
                    }).ToList();
                }
                else
                {
                    mockTestAnswers = questionIds.Select(x => new MockTestAnswer
                    {
                        Answer = null,
                        SectionGroupResultId = sectionGroupResult.Id,
                        MockTestResultId = sectionGroupResult.MockTestResultId ?? default,
                        IsCorrect = null,
                        Status = EnumAnswerStatus.Done,
                        SectionId = sectionGroup.CourseSkill == EnumCourseSkill.Writing ? x : null,
                        SectionTimeCodeId = sectionGroup.CourseSkill == EnumCourseSkill.Speaking ? x : null
                    }).ToList();
                }

                try
                {
                    await _mockTestAnswerRepository.BulkMergeAsync(mockTestAnswers, bulk =>
                    {
                        bulk.ColumnPrimaryKeyExpression = c => new { c.SectionId, c.SectionQuestionId, c.SectionTimeCodeId, c.SectionGroupResultId, c.MockTestResultId, c.IsDeleted };
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Log Duplicate MockTestAnswer : {ex.Message}");
                }
            }
            else
            {
                var sectionQuestions = await _sectionQuestionRepository.Queryable.WhereBulkContains(questionIds, x => x.Id).Select(x => new
                {
                    SectionQuestionId = x.Id,
                    QuestionType = x.Question!.QuestionType
                }).ToListAsync();

                if (sectionGroupResult.FinalTestResultId.HasValue)
                {
                    var finalTestAnswers = sectionQuestions.Select(x => new FinalTestAnswer
                    {
                        Answer = _answerTypeConverter.GetConfigEmpty(x.QuestionType),
                        SectionQuestionId = x.SectionQuestionId,
                        SectionGroupResultId = sectionGroupResult.Id,
                        FinalTestResultId = sectionGroupResult.FinalTestResultId ?? default,
                        IsCorrect = null,
                        Status = EnumAnswerStatus.Done
                    }).ToList();

                    try
                    {
                        await _finalTestAnswerRepository.BulkMergeAsync(finalTestAnswers, bulk =>
                        {
                            bulk.ColumnPrimaryKeyExpression = c => new { c.SectionQuestionId, c.SectionGroupResultId, c.FinalTestResultId, c.IsDeleted };
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Log Duplicate FinalTestAnswer : {ex.Message}");
                    }
                }
                else
                {
                    var placementTestAnswers = sectionQuestions.Select(x => new PlacementTestAnswer
                    {
                        Answer = _answerTypeConverter.GetConfigEmpty(x.QuestionType),
                        SectionQuestionId = x.SectionQuestionId,
                        SectionGroupResultId = sectionGroupResult.Id,
                        PlacementTestResultId = sectionGroupResult.PlacementTestResultId ?? default,
                        IsCorrect = null,
                        Status = EnumAnswerStatus.Done
                    }).ToList();
                    try
                    {
                        await _placementTestAnswerRepository.BulkMergeAsync(placementTestAnswers, bulk =>
                        {
                            bulk.ColumnPrimaryKeyExpression = c => new { c.SectionQuestionId, c.SectionGroupResultId, c.PlacementTestResultId, c.IsDeleted };
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Log Duplicate placementTestAnswer : {ex.Message}");
                    }
                }
            }
        }

        public async Task UpdateAnswerProcessByTest(SectionGroupResult sectionGroupResult)
        {
            ArgumentNullException.ThrowIfNull(sectionGroupResult);
            if (sectionGroupResult.FinalTestResultId.HasValue)
            {
                var finalTestAnswers = await _finalTestAnswerRepository.Queryable
                                        .Where(x => x.CreatedDate >= sectionGroupResult.CreatedDate)
                                        .Where(x => x.SectionGroupResultId == sectionGroupResult.Id && x.Status == EnumAnswerStatus.Process).ToListAsync();
                await _finalTestAnswerRepository.BulkUpdateList(finalTestAnswers.Select(x =>
                {
                    x.Status = EnumAnswerStatus.Done;
                    return x;
                }).ToList(), bulk =>
                {
                    bulk.ColumnInputExpression = entity => new { entity.Status };
                });
            }
            else if (sectionGroupResult.PlacementTestResultId.HasValue)
            {
                var placementTestAnswers = await _placementTestAnswerRepository.Queryable.Where(x => x.SectionGroupResultId == sectionGroupResult.Id && x.Status == EnumAnswerStatus.Process)
                                                                               .Where(x => x.CreatedDate >= sectionGroupResult.CreatedDate)
                                                                               .ToListAsync();
                await _placementTestAnswerRepository.BulkUpdateList(placementTestAnswers.Select(x =>
                {
                    x.Status = EnumAnswerStatus.Done;
                    return x;
                }).ToList(), bulk =>
                {
                    bulk.ColumnInputExpression = entity => new { entity.Status };
                });
            }
            else
            {
                var mockTestAnswers = await _mockTestAnswerRepository.Queryable.Where(x => x.SectionGroupResultId == sectionGroupResult.Id && x.Status == EnumAnswerStatus.Process)
                                                                               .Where(x => x.CreatedDate >= sectionGroupResult.CreatedDate)
                                                                               .ToListAsync();
                await _mockTestAnswerRepository.BulkUpdateList(mockTestAnswers.Select(x =>
                {
                    x.Status = EnumAnswerStatus.Done;
                    return x;
                }).ToList(), bulk =>
                {
                    bulk.ColumnInputExpression = entity => new { entity.Status };
                });
            }
        }

        public async Task<SectionGroupResult?> UpdateSectionGroupToIsSubmit(SectionGroup sectionGroup, SectionGroupResult? sectionGroupResult, bool isSubmit, double version = (int)EnumVersion.V1)
        {
            if (isSubmit && sectionGroupResult != null)
            {
                await UpdateUnansweredQuestions(sectionGroup, sectionGroupResult, version);
                await UpdateAnswerProcessByTest(sectionGroupResult).ConfigureAwait(false);
                return await UpdateSectionGroupResultAsync(sectionGroupResult, sectionGroup, version);
            }
            return sectionGroupResult;
        }

        #endregion Clean Code

        public async Task<IList<SectionGroupModel>> GetSectionGroupsAsync(IList<SectionGroup>? sectionGroups, Guid objectResultId, string? objectResultType)
        {
            ArgumentNullException.ThrowIfNull(sectionGroups);
            sectionGroups = sectionGroups.OrderBy(x => x.CourseSkill).ToList();
            var indexProcess = GetIndexProcess(sectionGroups, objectResultId, objectResultType);
            var sectionGroupModels = new List<SectionGroupModel>();
            foreach (var x in sectionGroups)
            {
                var index = sectionGroups.IndexOf(x);
                var sectionGroup = _mapper.Map<SectionGroupModel>(x);
                sectionGroup.TotalQuestion = GetTotalQuestion(x);
                sectionGroup.Status = GetResultStatus(indexProcess, index);
                var sectionGroupResult = x.SectionGroupResults.FirstOrDefault();
                if (sectionGroupResult != null)
                {
                    sectionGroup.SectionGroupResult = await GetSectionGroupResult(sectionGroupResult);
                }
                sectionGroupModels.Add(sectionGroup);
            }
            ;
            return sectionGroupModels;
        }

        private static EnumResultStatus GetResultStatus(int? indexProcess, int index)
        {
            var resultStatus = EnumResultStatus.Unfinished;
            if (indexProcess == index)
            {
                resultStatus = EnumResultStatus.Process;
            }
            else if (indexProcess > index || indexProcess == null)
            {
                resultStatus = EnumResultStatus.Done;
            }
            return resultStatus;
        }

        private static int? GetIndexProcess(IList<SectionGroup>? sectionGroups, Guid objectResultId, string? objectResultType)
        {
            ArgumentNullException.ThrowIfNull(sectionGroups);
            if (string.IsNullOrEmpty(objectResultType))
            {
                return null;
            }
            var sectionGroup = sectionGroups.FirstOrDefault(x => !x.SectionGroupResults.Any() || x.SectionGroupResults.Any(x => x.GetPropValue<Guid>(objectResultType) == objectResultId && x.Status != EnumResultStatus.Done));
            return sectionGroup != null ? sectionGroups.IndexOf(sectionGroup) : null;
        }

        #region Code Chưa Clearn

        private async Task<SectionGroupResultModel> GetSectionGroupResult(SectionGroupResult sectionGroupResult)
        {
            var sectionGroupResultDto = _mapper.Map<SectionGroupResultModel>(sectionGroupResult);
            sectionGroupResultDto.IsFeedBack = await _studentFeedbackRepository.Queryable.AnyAsync(x => x.ObjectId == sectionGroupResult.Id);
            return sectionGroupResultDto;
        }

        public async Task<SectionGroupDtoModel> GetSectionGroupDto(SectionGroup sectionGroup, SectionGroupResult sectionGroupResult, double version = (int)EnumVersion.V1)
        {
            ArgumentNullException.ThrowIfNull(sectionGroup);
            ArgumentNullException.ThrowIfNull(sectionGroupResult);
            var (sections, totalCount) = await GetSectionsAndTotalQuestionAsync(sectionGroup, sectionGroupResult, version);
            var isSectionGroupResultDone = sectionGroupResult.Status == EnumResultStatus.Done;
            var sectonGroupDetail = _mapper.Map<SectionGroupDtoModel>(sectionGroup);
            sectonGroupDetail.TotalQuestion = totalCount;
            sectonGroupDetail.SectionGroupResult = await GetSectionGroupResult(sectionGroupResult);
            sectonGroupDetail.Sections = sections.Select(x => GetSectionDto(x, isSectionGroupResultDone)).ToList();
            return sectonGroupDetail;
        }

        private SectionDtoModel GetSectionDto(Section section, bool isDone)
        {
            var sectionDto = _mapper.Map<SectionDtoModel>(section);
            if (section.SectionTimeCodes.Any())
            {
                sectionDto.SectionTimeCodes = section.SectionTimeCodes.Select(x => GetSectionTimeCodeDto(x)).OrderBy(x => x.DisplayTime).ToList();
            }
            else
            {
                if (sectionDto.SectionParts != null && sectionDto.SectionParts.Any())
                {
                    sectionDto.SectionParts = section.SectionParts.OrderBy(x => x.CreatedDate).Select(x => GetSectionPartMockTest(x, isDone)).ToList();
                }
                else if (section.SectionQuestions.Any())
                {
                    sectionDto.QuestionTests = section.SectionQuestions.OrderBy(x => x.CreatedDate)
                                                   .Select(x => new QuestionCorrectStatusModel
                                                   {
                                                       QuestionId = x.QuestionId ?? default,
                                                       Status = GetStatus(x, isDone)
                                                   }).ToList();

                    var subQuestionIds = section.SectionQuestions.Select(x => x.Question)
                        .Where(x => x != null && x.QuestionType == EnumQuestionType.CheckListV1)
                        .Select(x => x!.Config.Deserialize<CheckListQuestionV1>())
                        .Where(x => x != null).SelectMany(x => x!.Answers).Select(x => x.Id).ToList();

                    sectionDto.CountQuestion = section.SectionQuestions.SelectMany(x => x.MockTestAnswers)
                        .Select(x => x.Answer.Deserialize<MultipleChoiceAnswerV1>())
                        .Where(x => x != null && x.Answers != null && x.Answers.Any())
                        .SelectMany(x => x.Answers)
                        .Where(y => !string.IsNullOrEmpty(y.Key) || !string.IsNullOrEmpty(y.Content) || subQuestionIds.Any(x => x.HasValue && x == y.Id))
                        .Count();
                }
            }
            return sectionDto;
        }

        private SectionTimeCodeDtoModel GetSectionTimeCodeDto(SectionTimeCode sectionTimeCode)
        {
            var sectionTimeCodeDto = _mapper.Map<SectionTimeCodeDtoModel>(sectionTimeCode);
            return sectionTimeCodeDto;
        }

        private SectionPartDtoModel GetSectionPartMockTest(SectionPart sectionPart, bool isDone)
        {
            var sectionPartDto = _mapper.Map<SectionPartDtoModel>(sectionPart);
            sectionPartDto.QuestionTests = sectionPart.SectionQuestions.OrderBy(x => x.CreatedDate)
                                            .Select(x => new QuestionCorrectStatusModel
                                            {
                                                QuestionId = x.QuestionId ?? default,
                                                Status = GetStatus(x, isDone)
                                            }).ToList();
            return sectionPartDto;
        }

        private static EnumCorrectStatus? GetStatus(SectionQuestion sectionQuestion, bool isDone)
        {
            if (sectionQuestion.MockTestAnswers.Any())
            {
                return GetStatus(sectionQuestion.MockTestAnswers.FirstOrDefault(), isDone);
            }
            else if (sectionQuestion.FinalTestAnswers.Any())
            {
                return GetStatus(sectionQuestion.FinalTestAnswers.FirstOrDefault(), isDone);
            }
            else if (sectionQuestion.PlacementTestAnswers.Any())
            {
                return GetStatus(sectionQuestion.PlacementTestAnswers.FirstOrDefault(), isDone);
            }
            return null;
        }

        private static EnumCorrectStatus? GetStatus(BaseAnswer? baseAnswer, bool isDone)
        {
            return baseAnswer != null ? (isDone ? (baseAnswer.IsCorrect == true ? EnumCorrectStatus.Correct : EnumCorrectStatus.Fail) : EnumCorrectStatus.Process) : null;
        }

        public SectionGroupModel GetSectionGroupModel(SectionGroup? sectionGroup, bool isDisableAnswers = false)
        {
            ArgumentNullException.ThrowIfNull(sectionGroup);
            var sectionGroupModel = _mapper.Map<SectionGroupModel>(sectionGroup);
            sectionGroupModel.SkillName = sectionGroup.Skill?.Name;
            sectionGroupModel.TotalQuestion = GetTotalQuestion(sectionGroup);
            sectionGroupModel.MockTestScores = _mapper.Map<IList<MockTestScoreModel>>(sectionGroup.MockTestScores);
            sectionGroupModel.Sections = sectionGroup.Sections.OrderBy(x => x.CreatedDate).Select(x => GetSection(x, isDisableAnswers)).ToList();
            return sectionGroupModel;
        }

        public long GetTotalQuestion(IList<Section>? sections, EnumCourseSkill courseSkill)
        {
            if (sections != null && sections.Any())
            {
                return sections.Select(x => GetTotalQuestion(x, courseSkill)).Sum();
            }
            return default;
        }

        public long GetTotalQuestion(IList<SectionGroup>? sectionGroups)
        {
            if (sectionGroups != null && sectionGroups.Any())
            {
                return sectionGroups.Select(x => GetTotalQuestion(x)).Sum();
            }
            return default;
        }

        public long GetTotalQuestion(SectionGroup? sectionGroup)
        {
            ArgumentNullException.ThrowIfNull(sectionGroup);
            var sections = sectionGroup.Sections.ToList();
            return GetTotalQuestion(sections, sectionGroup.CourseSkill);
        }

        private SectionModel GetSection(Section section, bool isDisableAnswers = false)
        {
            var sectionDto = _mapper.Map<SectionModel>(section);
            if (section.SectionParts.Any())
            {
                sectionDto.SectionParts = GetSectionPartDtos(section.SectionParts.ToList(), isDisableAnswers);
            }
            if (section.SectionQuestions.Any())
            {
                sectionDto.Questions = GetQuestionDtos(section.SectionQuestions.ToList(), isDisableAnswers);
            }
            if (section.SectionTimeCodes.Any())
            {
                sectionDto.SectionTimeCodes = GetSectionTimeCodeDtos(section.SectionTimeCodes.ToList());
            }
            sectionDto.MockTestAnswer = _mapper.Map<MockTestAnswerModel>(section.MockTestAnswers.FirstOrDefault());
            return sectionDto;
        }

        private IList<SectionPartModel> GetSectionPartDtos(IList<SectionPart> sectionParts, bool isDisableAnswers = false)
        {
            return sectionParts.OrderBy(x => x.CreatedDate).Select(x =>
            {
                var sectionPart = _mapper.Map<SectionPartModel>(x);
                sectionPart.Questions = GetQuestionDtos(x.SectionQuestions.ToList(), isDisableAnswers);
                return sectionPart;
            }).ToList();
        }

        private IList<SectionTimeCodeModel> GetSectionTimeCodeDtos(IList<SectionTimeCode> sectionTimeCodes)
        {
            return sectionTimeCodes.OrderBy(x => x.DisplayTime).Select(x =>
            {
                var sectionTimeCode = _mapper.Map<SectionTimeCodeModel>(x);
                sectionTimeCode.MockTestAnswer = _mapper.Map<MockTestAnswerModel>(x.MockTestAnswers.FirstOrDefault());
                return sectionTimeCode;
            }).ToList();
        }

        private IList<QuestionModel> GetQuestionDtos(IList<SectionQuestion> sectionQuestions, bool isDisableAnswers = false)
        {
            return sectionQuestions.OrderBy(x => x.CreatedDate).Select(x => GetQuestion(x.Question, x.MockTestAnswers.FirstOrDefault(), isDisableAnswers)).ToList();
        }

        private QuestionModel GetQuestion(Question? question, object? answer, bool isDisableAnswers = false)
        {
            ArgumentNullException.ThrowIfNull(question);
            var questionDto = _mapper.Map<QuestionModel>(question);
            questionDto.Config = _questionTypeConverter.QuestionTypeConverterObject(question.Config, question.QuestionType, isDisableAnswers: isDisableAnswers).Item1;
            questionDto.ResultAnswer = _mapper.Map<AnswerModel>(answer);
            return questionDto;
        }

        public async Task<bool> IsTeacherGraded(MockTestResult mockTestResult, IList<EnumCourseSkill>? courseSkills)
        {
            ArgumentNullException.ThrowIfNull(mockTestResult);
            var isTeacherGradedSkill = courseSkills?.Any(x => x == EnumCourseSkill.Speaking);
            var isAIGraded = courseSkills?.Any(x => x == EnumCourseSkill.Writing);
            if (isTeacherGradedSkill.HasValue && isTeacherGradedSkill.Value)
            {
                return mockTestResult.MockTestScores.Any();
            }
            if (isAIGraded.HasValue && isAIGraded.Value)
            {
                var mockTestAnswers = await _mockTestAnswerRepository.Queryable.Where(x => x.MockTestResultId == mockTestResult.Id)
                                                                               .Where(x => x.CreatedDate >= mockTestResult.CreatedDate)
                                                                               .Where(x => !(mockTestResult.Status == EnumResultStatus.Done && mockTestResult.UpdatedDate.HasValue) || x.CreatedDate <= mockTestResult.UpdatedDate)
                                                                               .ToListAsync();
                return mockTestAnswers.All(x => !string.IsNullOrEmpty(x.GradingAlFeedback));
            }
            return true;
        }

        #endregion Code Chưa Clearn
    }
}
