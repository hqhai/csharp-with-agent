// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Common
{
    using AutoMapper;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Microsoft.EntityFrameworkCore;

    public class SectionGroupConverter
    {
        private readonly ISectionGroupResultRepository _sectionGroupResultRepository;
        private readonly QuestionTypeConverter _questionTypeConverter;
        private readonly IMapper _mapper;
        private readonly DateTimeConverter _dateTimeConverter;
        private readonly IQuestionRepository _questionRepository;
        private readonly LinQHelper _linQHelper;
        private readonly ISectionRepository _sectionRepository;
        private readonly IFinalTestAnswerRepository _finalTestAnswerRepository;
        private readonly IPlacementTestAnswerRepository _placementTestAnswerRepository;
        private readonly IMockTestAnswerRepository _mockTestAnswerRepository;

        #region Clean Code

        public SectionGroupConverter(ISectionGroupResultRepository sectionGroupResultRepository, QuestionTypeConverter questionTypeConverter, IMapper mapper, DateTimeConverter dateTimeConverter, IQuestionRepository questionRepository, LinQHelper linQHelper, ISectionRepository sectionRepository, IFinalTestAnswerRepository finalTestAnswerRepository, IPlacementTestAnswerRepository placementTestAnswerRepository, IMockTestAnswerRepository mockTestAnswerRepository)
        {
            _sectionGroupResultRepository = sectionGroupResultRepository;
            _questionTypeConverter = questionTypeConverter;
            _mapper = mapper;
            _dateTimeConverter = dateTimeConverter;
            _questionRepository = questionRepository;
            _linQHelper = linQHelper;
            _sectionRepository = sectionRepository;
            _finalTestAnswerRepository = finalTestAnswerRepository;
            _placementTestAnswerRepository = placementTestAnswerRepository;
            _mockTestAnswerRepository = mockTestAnswerRepository;
        }

        public async Task<int> GetHighestStreak(SectionGroupResult sectionGroupResult)
        {
            ArgumentNullException.ThrowIfNull(sectionGroupResult);
            if (sectionGroupResult.MockTestResultId.HasValue)
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

        public async Task<SectionGroupResult> UpdateSectionGroupResultAsync(SectionGroupResult sectionGroupResult, SectionGroup sectionGroup)
        {
            ArgumentNullException.ThrowIfNull(sectionGroup);
            ArgumentNullException.ThrowIfNull(sectionGroupResult);
            var skillScore = await GetSkillScore(sectionGroup, sectionGroupResult);
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
            await _sectionGroupResultRepository.UnitOfWork.SaveEntitiesAsync().ConfigureAwait(false);
            return sectionGroupResult;
        }

        private async Task<SkillScores> GetSkillScore(SectionGroup sectionGroup, SectionGroupResult sectionGroupResult)
        {
            if (sectionGroupResult.MockTestResultId.HasValue)
            {
                return await GetSkillScoreMockTest(sectionGroup, sectionGroupResult);
            }
            else if (sectionGroupResult.FinalTestResultId.HasValue)
            {
                return await GetSkillScoreFinalTest(sectionGroup, sectionGroupResult);
            }
            else
            {
                return await GetSkillScorePlacementTest(sectionGroup, sectionGroupResult);
            }
        }

        private async Task<SkillScores> GetSkillScoreFinalTest(SectionGroup sectionGroup, SectionGroupResult sectionGroupResult)
        {
            var finalTestAnswers = await _finalTestAnswerRepository.Queryable.Include(x => x.SectionQuestion).Where(x => x.SectionGroupResultId == sectionGroupResult.Id && x.FinalTestResultId == sectionGroupResult.FinalTestResultId).ToListAsync();
            var skillScore = GetSkillScore(sectionGroup, new List<BaseAnswer>(finalTestAnswers));

            var questionIds = finalTestAnswers.Select(x => x.SectionQuestion).Select(x => x!.QuestionId).ToList();
            skillScore.TotalCount = await _questionRepository.Queryable.Where(x => questionIds.Contains(x.Id)).SumAsync(x => x.CorrectTotal);
            skillScore.TotalQuestion = questionIds.Count;
            return skillScore;
        }

        private async Task<SkillScores> GetSkillScorePlacementTest(SectionGroup sectionGroup, SectionGroupResult sectionGroupResult)
        {
            var placementTestAnswers = await _placementTestAnswerRepository.Queryable.Include(x => x.SectionQuestion).Where(x => x.SectionGroupResultId == sectionGroupResult.Id && x.PlacementTestResultId == sectionGroupResult.PlacementTestResultId).ToListAsync();
            var skillScore = GetSkillScore(sectionGroup, new List<BaseAnswer>(placementTestAnswers));

            var questionIds = placementTestAnswers.Select(x => x.SectionQuestion).Select(x => x!.QuestionId).ToList();
            skillScore.TotalCount = await _questionRepository.Queryable.Where(x => questionIds.Contains(x.Id)).SumAsync(x => x.CorrectTotal);
            skillScore.TotalQuestion = questionIds.Count;
            return skillScore;
        }

        private async Task<SkillScores> GetSkillScoreMockTest(SectionGroup sectionGroup, SectionGroupResult sectionGroupResult)
        {
            ArgumentNullException.ThrowIfNull(sectionGroup);
            var maxTotalCorrect = 36;
            int totalQuestion = default;

            var mockTestAnswers = await _mockTestAnswerRepository.Queryable.Include(x => x.SectionQuestion).Where(x => x.SectionGroupResultId == sectionGroupResult.Id).ToListAsync();
            var skillScore = GetSkillScore(sectionGroup, new List<BaseAnswer>(mockTestAnswers));

            if (sectionGroup.CourseSkill == EnumCourseSkill.Listening || sectionGroup.CourseSkill == EnumCourseSkill.Reading)
            {
                var questionIds = mockTestAnswers.Select(x => x.SectionQuestion).Select(x => x.QuestionId).ToList();
                maxTotalCorrect = await _questionRepository.Queryable.Where(x => questionIds.Contains(x.Id)).SumAsync(x => x.CorrectTotal);
                totalQuestion = questionIds.Count;
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

        public SkillScores GetSkillScore(SectionGroup sectionGroup, IList<BaseAnswer>? baseAnswers)
        {
            ArgumentNullException.ThrowIfNull(sectionGroup);
            ArgumentNullException.ThrowIfNull(baseAnswers);
            return new SkillScores
            {
                CorrectCount = baseAnswers.Sum(x => x.CorrectCount),
                CountQuestion = baseAnswers.Count,
                Skill = sectionGroup.CourseSkill,
                Scores = baseAnswers.Sum(x => x.CorrectCount).GetIeltsScore(sectionGroup.CourseSkill)
            };
        }

        public async Task<IList<Section>> GetSectionsAsync(SectionGroup sectionGroup, SectionGroupResult sectionGroupResult)
        {
            ArgumentNullException.ThrowIfNull(sectionGroup);
            ArgumentNullException.ThrowIfNull(sectionGroupResult);
            var query = _sectionRepository.Queryable;
            if (sectionGroupResult.MockTestResultId.HasValue)
            {
                if (sectionGroup.CourseSkill == EnumCourseSkill.Reading || sectionGroup.CourseSkill == EnumCourseSkill.Listening)
                {
                    query = query.Include(x => x.SectionParts).ThenInclude(x => x.SectionQuestions).ThenInclude(x => x.MockTestAnswers.Where(x => x.SectionGroupResultId == sectionGroupResult.Id));
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

        public async Task<(IList<Section>, long)> GetSectionsAndTotalQuestionAsync(SectionGroup sectionGroup, SectionGroupResult sectionGroupResult)
        {
            var sections = await GetSectionsAsync(sectionGroup, sectionGroupResult);
            return (sections, sections?.Select(x => GetTotalQuestion(x, sectionGroup.CourseSkill)).Sum() ?? default);
        }

        private static long GetTotalQuestion(Section section, EnumCourseSkill courseSkill)
        {
            ArgumentNullException.ThrowIfNull(section);
            if (courseSkill == EnumCourseSkill.Speaking)
            {
                return section.SectionTimeCodes.Any() ? section.SectionTimeCodes.Count : default;
            }
            else if (courseSkill == EnumCourseSkill.Writing)
            {
                return 1;
            }
            return section.SectionParts.Any() ? section.SectionParts.SelectMany(x => x.SectionQuestions).Count() : section.SectionQuestions.Count;
        }

        private static IList<Guid>? GetUnansweredMockTests(IList<Section>? sections, SectionGroup sectionGroup)
        {
            ArgumentNullException.ThrowIfNull(sections);
            if (sectionGroup.CourseSkill == EnumCourseSkill.Reading || sectionGroup.CourseSkill == EnumCourseSkill.Listening)
            {
                var sectionQuestions = sections.SelectMany(x => x.SectionParts).SelectMany(x => x.SectionQuestions);
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

        private async Task<IList<Guid>?> GetUnansweredQuestionIds(SectionGroup sectionGroup, SectionGroupResult sectionGroupResult)
        {
            ArgumentNullException.ThrowIfNull(sectionGroup);
            ArgumentNullException.ThrowIfNull(sectionGroupResult);
            var sections = await GetSectionsAsync(sectionGroup, sectionGroupResult);
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

        public async Task UpdateUnansweredQuestions(SectionGroup sectionGroup, SectionGroupResult sectionGroupResult)
        {
            var questionIds = await GetUnansweredQuestionIds(sectionGroup, sectionGroupResult);
            if (questionIds != null && questionIds.Any())
            {
                if (sectionGroupResult.FinalTestResultId.HasValue)
                {
                    await _finalTestAnswerRepository.AddList(questionIds.Select(x => new FinalTestAnswer
                    {
                        Answer = null,
                        SectionQuestionId = x,
                        SectionGroupResultId = sectionGroupResult.Id,
                        FinalTestResultId = sectionGroupResult.FinalTestResultId ?? default,
                        IsCorrect = null,
                        Status = EnumAnswerStatus.Done
                    }).ToList());
                    await _finalTestAnswerRepository.UnitOfWork.SaveEntitiesAsync().ConfigureAwait(false);
                }
                else if (sectionGroupResult.PlacementTestResultId.HasValue)
                {
                    await _placementTestAnswerRepository.AddList(questionIds.Select(x => new PlacementTestAnswer
                    {
                        Answer = null,
                        SectionQuestionId = x,
                        SectionGroupResultId = sectionGroupResult.Id,
                        PlacementTestResultId = sectionGroupResult.PlacementTestResultId ?? default,
                        IsCorrect = null,
                        Status = EnumAnswerStatus.Done
                    }).ToList());
                    await _placementTestAnswerRepository.UnitOfWork.SaveEntitiesAsync().ConfigureAwait(false);
                }
                else
                {
                    var mockTestAnswers = questionIds.Select(x =>
                     {
                         var mocktestAnswer = new MockTestAnswer
                         {
                             Answer = null,
                             SectionGroupResultId = sectionGroupResult.Id,
                             MockTestResultId = sectionGroupResult.MockTestResultId ?? default,
                             IsCorrect = null,
                             Status = EnumAnswerStatus.Done
                         };
                         mocktestAnswer.SectionId = sectionGroup.CourseSkill == EnumCourseSkill.Writing ? x : null;
                         mocktestAnswer.SectionTimeCodeId = sectionGroup.CourseSkill == EnumCourseSkill.Speaking ? x : null;
                         mocktestAnswer.SectionQuestionId = sectionGroup.CourseSkill != EnumCourseSkill.Writing && sectionGroup.CourseSkill != EnumCourseSkill.Speaking ? x : null;
                         return mocktestAnswer;
                     }).ToList();
                    await _mockTestAnswerRepository.AddList(mockTestAnswers);
                    await _mockTestAnswerRepository.UnitOfWork.SaveEntitiesAsync().ConfigureAwait(false);
                }
            }
        }

        public async Task UpdateAnswerProcessByTest(SectionGroupResult sectionGroupResult)
        {
            ArgumentNullException.ThrowIfNull(sectionGroupResult);
            if (sectionGroupResult.FinalTestResultId.HasValue)
            {
                var finalTestAnswers = await _finalTestAnswerRepository.Queryable.Where(x => x.SectionGroupResultId == sectionGroupResult.Id && x.Status == EnumAnswerStatus.Process).ToListAsync();
                _finalTestAnswerRepository.UpdateList(finalTestAnswers.Select(x =>
               {
                   x.Status = EnumAnswerStatus.Done;
                   return x;
               }).ToList());
                await _finalTestAnswerRepository.UnitOfWork.SaveEntitiesAsync().ConfigureAwait(false);
            }
            else if (sectionGroupResult.PlacementTestResultId.HasValue)
            {
                var placementTestAnswers = await _placementTestAnswerRepository.Queryable.Where(x => x.SectionGroupResultId == sectionGroupResult.Id && x.Status == EnumAnswerStatus.Process).ToListAsync();
                _placementTestAnswerRepository.UpdateList(placementTestAnswers.Select(x =>
               {
                   x.Status = EnumAnswerStatus.Done;
                   return x;
               }).ToList());
                await _placementTestAnswerRepository.UnitOfWork.SaveEntitiesAsync().ConfigureAwait(false);
            }
            else
            {
                var mockTestAnswers = await _mockTestAnswerRepository.Queryable.Where(x => x.SectionGroupResultId == sectionGroupResult.Id && x.Status == EnumAnswerStatus.Process).ToListAsync();
                _mockTestAnswerRepository.UpdateList(mockTestAnswers.Select(x =>
               {
                   x.Status = EnumAnswerStatus.Done;
                   return x;
               }).ToList());
                await _mockTestAnswerRepository.UnitOfWork.SaveEntitiesAsync().ConfigureAwait(false);
            }
        }

        #endregion Clean Code

        public IList<SectionGroupModel> GetSectionGroups(IList<SectionGroup>? sectionGroups, Guid objectResultId, string? objectResultType)
        {
            ArgumentNullException.ThrowIfNull(sectionGroups);
            var indexProcess = GetIndexProcess(sectionGroups, objectResultId, objectResultType);
            return sectionGroups.Select(x =>
            {
                var index = sectionGroups.IndexOf(x);
                var sectionGroup = _mapper.Map<SectionGroupModel>(x);
                sectionGroup.Status = GetResultStatus(indexProcess, index);
                sectionGroup.SectionGroupResult = _mapper.Map<SectionGroupResultModel>(x.SectionGroupResults.FirstOrDefault());
                return sectionGroup;
            }).ToList();
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

        private SectionGroupResultModel GetSectionGroupResult(SectionGroupResult sectionGroupResult, SectionGroup sectionGroup)
        {
            var sectionGroupResultDto = _mapper.Map<SectionGroupResultModel>(sectionGroupResult);
            sectionGroupResultDto.RemainingTime = sectionGroup.ExecutionTime - sectionGroupResult.WorkingTime;
            return sectionGroupResultDto;
        }

        public async Task<SectionGroupDtoModel> GetSectionGroupDto(SectionGroup sectionGroup, SectionGroupResult sectionGroupResult)
        {
            ArgumentNullException.ThrowIfNull(sectionGroup);
            ArgumentNullException.ThrowIfNull(sectionGroupResult);
            var (sections, totalCount) = await GetSectionsAndTotalQuestionAsync(sectionGroup, sectionGroupResult);
            var isSectionGroupResultDone = sectionGroupResult.Status == EnumResultStatus.Done;
            var sectonGroupDetail = _mapper.Map<SectionGroupDtoModel>(sectionGroup);
            sectonGroupDetail.TotalQuestion = totalCount;
            sectonGroupDetail.SectionGroupResult = GetSectionGroupResult(sectionGroupResult, sectionGroup);
            if (sectionGroupResult.MockTestResultId.HasValue)
            {
                sectonGroupDetail.Sections = sections.Select(x => GetSectionByMockTest(x, sectionGroup.CourseSkill, isSectionGroupResultDone)).ToList();
            }
            else
            {
                sectonGroupDetail.Sections = sections.Select(x => GetSectionDto(x, isSectionGroupResultDone)).ToList();
            }
            return sectonGroupDetail;
        }

        private SectionDtoModel GetSectionDto(Section section, bool isDone)
        {
            var sectionDto = _mapper.Map<SectionDtoModel>(section);
            sectionDto.QuestionTests = section.SectionQuestions.OrderBy(x => x.CreatedDate)
                                            .Select(x => new QuestionTestModel
                                            {
                                                QuestionId = x.QuestionId ?? default,
                                                Status = GetStatus(x, isDone)
                                            }).ToList();
            return sectionDto;
        }

        private SectionDtoModel GetSectionByMockTest(Section section, EnumCourseSkill skill, bool isDone)
        {
            var sectionDetail = _mapper.Map<SectionDtoModel>(section);
            if (skill == EnumCourseSkill.Reading || skill == EnumCourseSkill.Listening)
            {
                sectionDetail.SectionParts = section.SectionParts.OrderBy(x => x.CreatedDate).Select(x => GetSectionPartMockTest(x, isDone)).ToList();
            }
            else if (skill == EnumCourseSkill.Speaking)
            {
                sectionDetail.SectionTimeCodes = section.SectionTimeCodes.Select(x => GetSectionTimeCodeDto(x)).ToList();
            }
            return sectionDetail;
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
                                            .Select(x => new QuestionTestModel
                                            {
                                                QuestionId = x.QuestionId ?? default,
                                                Status = GetStatus(x, isDone)
                                            }).ToList();
            return sectionPartDto;
        }

        private static EnumSubAnswerStatus? GetStatus(SectionQuestion sectionQuestion, bool isDone)
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

        private static EnumSubAnswerStatus? GetStatus(BaseAnswer? baseAnswer, bool isDone)
        {
            if (baseAnswer != null)
            {
                if (isDone)
                {
                    return baseAnswer.IsCorrect == true ? EnumSubAnswerStatus.Correct : EnumSubAnswerStatus.Fail;
                }
                return EnumSubAnswerStatus.Process;
            }
            return null;
        }

        public SectionGroupModel GetSectionGroupModel(SectionGroup? sectionGroup, bool isDisableAnswers = false)
        {
            ArgumentNullException.ThrowIfNull(sectionGroup);
            var sectionGroupModel = _mapper.Map<SectionGroupModel>(sectionGroup);
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
            var sections = sectionGroup.Sections;
            if (sectionGroup.CourseSkill == EnumCourseSkill.Speaking)
            {
                var sectionTimeCode = sections.SelectMany(x => x.SectionTimeCodes);
                return sectionTimeCode.Any() ? sectionTimeCode.Count() : default;
            }
            else if (sectionGroup.CourseSkill == EnumCourseSkill.Writing)
            {
                return sections.Any() ? sections.Count : default;
            }
            else
            {
                var sectionParts = sections.SelectMany(x => x.SectionParts).ToList();
                return sectionParts.Any() ? sectionParts.SelectMany(x => x.SectionQuestions).Count() : sections.SelectMany(x => x.SectionQuestions).Count();
            }
        }

        private SectionModel GetSection(Section section, bool isDisableAnswers = false)
        {
            var sectionDto = _mapper.Map<SectionModel>(section);
            sectionDto.SectionParts = GetSectionPartDtos(section.SectionParts.ToList(), isDisableAnswers);
            sectionDto.Questions = GetQuestionDtos(section.SectionQuestions.ToList(), isDisableAnswers);
            sectionDto.SectionTimeCodes = GetSectionTimeCodeDtos(section.SectionTimeCodes.ToList());
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

        public IList<QuestionModel> GetQuestionDtos(IList<SectionQuestion> sectionQuestions, bool isDisableAnswers = false)
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

        #endregion Code Chưa Clearn
    }
}
