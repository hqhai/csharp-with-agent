// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Common
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.Questions;
    using Fsel.Course.Domain.Models.CommandModels.Sections;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Microsoft.EntityFrameworkCore;

    public class SectionConverter
    {
        private readonly IMapper _mapper;
        private readonly QuestionTypeConverter _questionTypeConverter;
        private readonly IQuestionRepository _questionRepository;
        private readonly ISectionRepository _sectionRepository;
        private readonly ISectionGroupRepository _sectionGroupRepository;
        private readonly ISectionQuestionRepository _sectionQuestionRepository;

        public SectionConverter(IMapper mapper
            , QuestionTypeConverter questionTypeConverter
            , IQuestionRepository questionRepository
            , ISectionRepository sectionRepository
            , ISectionGroupRepository sectionGroupRepository
            , ISectionQuestionRepository sectionQuestionRepository)
        {
            _mapper = mapper;
            _questionTypeConverter = questionTypeConverter;
            _questionRepository = questionRepository;
            _sectionRepository = sectionRepository;
            _sectionGroupRepository = sectionGroupRepository;
            _sectionQuestionRepository = sectionQuestionRepository;
        }

        public SectionGroupResultModel GetSectionGroupResult(SectionGroupResult sectionGroupResult, SectionGroup sectionGroup)
        {
            ArgumentNullException.ThrowIfNull(sectionGroupResult);
            ArgumentNullException.ThrowIfNull(sectionGroup);
            var sectionGroupResultDto = _mapper.Map<SectionGroupResultModel>(sectionGroupResult);
            sectionGroupResultDto.RemainingTime = GetRemainingTime(sectionGroupResult, sectionGroup);
            return sectionGroupResultDto;
        }

        private static double GetRemainingTime(SectionGroupResult sectionGroupResult, SectionGroup sectionGroup)
        {
            if (sectionGroupResult.Status == EnumResultStatus.Done && sectionGroupResult.UpdatedDate.HasValue)
            {
                return sectionGroup.ExecutionTime - Shared.Helpers.DateTimeHelper.GetWorkingTime(sectionGroupResult.CreatedDate, sectionGroupResult.UpdatedDate.Value, sectionGroup.ExecutionTime);
            }
            return sectionGroup.ExecutionTime - Shared.Helpers.DateTimeHelper.GetWorkingTime(sectionGroupResult.CreatedDate, DateTime.UtcNow, sectionGroup.ExecutionTime);
        }

        public async Task<(IList<Section>, long)> GetSectionsAsync(Guid sectionGroupId, EnumCourseSkill skill, bool isMockTest = false)
        {
            var sections = new List<Section>();
            if (isMockTest)
            {
                if (skill == EnumCourseSkill.Reading || skill == EnumCourseSkill.Listening)
                {
                    sections = await _sectionRepository.Queryable.Include(x => x.SectionParts).ThenInclude(x => x.SectionQuestions)
                                                                        .Where(x => x.SectionGroupId == sectionGroupId).OrderBy(x => x.DisplayOrder)
                                                                        .ToListAsync();
                }
                else if (skill == EnumCourseSkill.Writing)
                {
                    sections = await _sectionRepository.Queryable.Include(x => x.MockTestAnswers).Where(x => x.SectionGroupId == sectionGroupId).OrderBy(x => x.DisplayOrder)
                                                                        .ToListAsync();
                }
                else
                {
                    sections = await _sectionRepository.Queryable.Include(x => x.SectionTimeCodes).ThenInclude(x => x.MockTestAnswers)
                                                                .Where(x => x.SectionGroupId == sectionGroupId).OrderBy(x => x.DisplayOrder)
                                                                .ToListAsync();
                }
                return (sections, GetTotalQuestion(sections, skill));
            }
            sections = await _sectionRepository.Queryable.Include(x => x.SectionQuestions)
                                                               .Where(x => x.SectionGroupId == sectionGroupId).OrderBy(x => x.DisplayOrder)
                                                               .ToListAsync();
            return (sections, GetTotalQuestion(sections, skill));
        }

        public SectionGroupDtoModel GetSectionGroupDto(long totalCount, IList<Section> sections, SectionGroup sectionGroup, SectionGroupResult sectionGroupResult, bool isMockTest = false)
        {
            ArgumentNullException.ThrowIfNull(sectionGroup);
            var sectonGroupDetail = _mapper.Map<SectionGroupDtoModel>(sectionGroup);
            sectonGroupDetail.SectionGroupResult = GetSectionGroupResult(sectionGroupResult, sectionGroup);
            sectonGroupDetail.Sections = isMockTest ? GetSectionsByMockTest(sections, sectionGroup.CourseSkill) : GetSections(sections);
            sectonGroupDetail.TotalQuestion = totalCount;
            return sectonGroupDetail;
        }

        private IList<SectionDtoModel> GetSections(IList<Section> sections)
        {
            var listSection = new List<SectionDtoModel>();
            return sections.Select(x => _mapper.Map<SectionDtoModel>(x)).ToList();
        }

        private IList<SectionDtoModel> GetSectionsByMockTest(IList<Section> sections, EnumCourseSkill skill)
        {
            var listSection = new List<SectionDtoModel>();
            return sections.Select(x => GetSectionByMockTest(x, skill)).ToList();
        }

        private SectionDtoModel GetSectionByMockTest(Section section, EnumCourseSkill skill)
        {
            var sectionDetail = _mapper.Map<SectionDtoModel>(section);
            if (skill == EnumCourseSkill.Reading || skill == EnumCourseSkill.Listening)
            {
                sectionDetail.SectionParts = section.SectionParts.OrderBy(x => x.CreatedDate).Select(x => _mapper.Map<SectionPartDtoModel>(x)).ToList();
            }
            else if (skill == EnumCourseSkill.Speaking)
            {
                sectionDetail.SectionTimeCodes = section.SectionTimeCodes.Select(x => _mapper.Map<SectionTimeCodeDtoModel>(x)).ToList();
            }
            return sectionDetail;
        }

        public SectionGroupModel GetSectionGroupModel(SectionGroup? sectionGroup, bool isDisableAnswers = false)
        {
            var sectionGroupModel = GetSectionGroup(sectionGroup);
            sectionGroupModel.MockTestScores = _mapper.Map<IList<MockTestScoreModel>>(sectionGroup?.MockTestScores);
            sectionGroupModel.Sections = GetSectionModels(sectionGroup?.Sections.ToList(), isDisableAnswers);
            return sectionGroupModel;
        }

        public SectionGroupModel GetSectionGroup(SectionGroup? sectionGroup)
        {
            var sectionGroupModel = _mapper.Map<SectionGroupModel>(sectionGroup);
            sectionGroupModel.TotalQuestion = GetTotalQuestion(sectionGroup);
            return sectionGroupModel;
        }

        public double GetExecutionTime(IList<SectionGroup>? sectionGroups)
        {
            if (sectionGroups != null && sectionGroups.Any())
            {
                return sectionGroups.Sum(x => x.ExecutionTime);
            }
            return default;
        }

        public IList<EnumCourseSkill>? GetCourseSkill(IList<SectionGroup>? sectionGroups)
        {
            if (sectionGroups != null && sectionGroups.Any())
            {
                return sectionGroups.Select(x => x.CourseSkill).ToList();
            }
            return default;
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

        public long GetTotalQuestion(SectionGroup? sectionGroup, bool isMockTest = false)
        {
            ArgumentNullException.ThrowIfNull(sectionGroup);

            if (sectionGroup.CourseSkill == EnumCourseSkill.Speaking && isMockTest)
            {
                var sectionTimeCode = sectionGroup.Sections.SelectMany(x => x.SectionTimeCodes);
                return sectionTimeCode.Any() ? sectionTimeCode.Count() : 0;
            }
            else if (sectionGroup.CourseSkill == EnumCourseSkill.Writing && isMockTest)
            {
                return sectionGroup.Sections.Any() ? sectionGroup.Sections.Count : 0;
            }
            else
            {
                var sectionParts = sectionGroup.Sections.SelectMany(x => x.SectionParts).ToList();
                if (sectionParts.Count > 0)
                {
                    return sectionParts.SelectMany(x => x.SectionQuestions).Count();
                }
                else
                {
                    var sectionQuestions = sectionGroup.Sections.SelectMany(x => x.SectionQuestions).ToList();
                    return sectionQuestions.Any() ? sectionQuestions.Count : default;
                }
            }
        }

        public long GetTotalQuestion(Section? section, EnumCourseSkill courseSkill)
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
            else
            {
                var sectionParts = section.SectionParts.ToList();
                if (sectionParts.Any())
                {
                    return sectionParts.SelectMany(x => x.SectionQuestions).Count();
                }
                else
                {
                    var sectionQuestions = section.SectionQuestions.ToList();
                    return sectionQuestions.Any() ? sectionQuestions.Count : default;
                }
            }
        }

        public IList<SectionModel>? GetSectionModels(IList<Section>? sections, bool isDisableAnswers = false)
        {
            return sections?.OrderBy(x => x.CreatedDate).Select(x => GetSection(x, isDisableAnswers)).ToList();
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

        public IList<SectionPartModel> GetSectionPartDtos(IList<SectionPart> sectionParts, bool isDisableAnswers = false)
        {
            return sectionParts.OrderBy(x => x.CreatedDate).Select(x => GetSectionPart(x, isDisableAnswers)).ToList();
        }

        private SectionPartModel GetSectionPart(SectionPart sectionPart, bool isDisableAnswers = false)
        {
            ArgumentNullException.ThrowIfNull(sectionPart);
            var questionDto = _mapper.Map<SectionPartModel>(sectionPart);
            questionDto.Questions = GetQuestionDtos(sectionPart.SectionQuestions.ToList(), isDisableAnswers);
            return questionDto;
        }

        public IList<SectionTimeCodeModel> GetSectionTimeCodeDtos(IList<SectionTimeCode> sectionTimeCodes)
        {
            return sectionTimeCodes.OrderBy(x => x.DisplayTime).Select(x => GetSectionTimeCode(x)).ToList();
        }

        private SectionTimeCodeModel GetSectionTimeCode(SectionTimeCode sectionTimeCode)
        {
            var sectionTimeCodeDto = _mapper.Map<SectionTimeCodeModel>(sectionTimeCode);
            sectionTimeCodeDto.MockTestAnswer = _mapper.Map<MockTestAnswerModel>(sectionTimeCode.MockTestAnswers.FirstOrDefault());
            return sectionTimeCodeDto;
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

        public VoidMethodResult AddQuestionToSession(dynamic section, IList<CreateQuestionCommandModel>? questionModels)
        {
            VoidMethodResult methodResult = new VoidMethodResult();
            var questions = _mapper.Map<IList<Question>>(questionModels);
            if (questions == null || !questions.Any())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(questions));
                return methodResult;
            }
            foreach (var question in questions)
            {
                var (config, correctTotal) = _questionTypeConverter.QuestionTypeConverterObject(question.Config, question.QuestionType, isShowCorrectTotal: !question.Ungraded, false);
                if (config == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumVideoErrorCode.ConfigIsInTheWrongFormat), nameof(question.Config), question.Config);
                }
                if (!question.IsValid())
                {
                    methodResult.AddErrorBadRequest(question.ErrorMessages);
                    return methodResult;
                }
                question.CorrectTotal = correctTotal;

                if (section != null)
                {
                    section.SectionQuestions.Add(new SectionQuestion
                    {
                        Question = question,
                    });
                }
            }

            if (!section.IsValid())
            {
                methodResult.AddErrorBadRequest(section.ErrorMessages);
                return methodResult;
            }

            return methodResult;
        }

        public VoidMethodResult AddSessionToSessionGroup(dynamic sectionGroup, IList<CreateSectionCommandModel>? sectionModels, EnumCourseType? type)
        {
            VoidMethodResult methodResult = new VoidMethodResult();
            //if (sectionModels == null || sectionModels.Count == 0)
            //{
            //    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(sectionGroup.Sections));
            //    return methodResult;
            //}
            if (sectionModels != null && sectionModels.Any())
            {
                IList<Section> sections = sectionGroup.Sections;
                foreach (var section in sectionModels)
                {
                    if (section == null)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(section));
                        return methodResult;
                    }
                    Section newSection = sections.ElementAt(sectionModels.IndexOf(section));
                    if (sectionGroup.CourseSkill != EnumCourseSkill.Speaking && sectionGroup.CourseSkill != EnumCourseSkill.Writing)
                    {
                        if (section.SectionParts != null && section.Questions != null && section.SectionParts.Count > 0 && section.Questions.Count > 0)
                        {
                            methodResult.AddErrorBadRequest(nameof(EnumSectionErrorCode.OnlyOneOfTwoSectionPartsOrQuestions));
                            return methodResult;
                        }
                        if (type == EnumCourseType.Ielts)
                        {
                            //if (section.SectionParts == null || section.SectionParts.Count == 0)
                            //{
                            //    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(section.SectionParts));
                            //    return methodResult;
                            //}

                            if (section.SectionParts != null && section.SectionParts.Any())
                            {
                                foreach (var sectionPart in section.SectionParts)
                                {
                                    if (sectionPart == null)
                                    {
                                        methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(sectionPart));
                                        return methodResult;
                                    }
                                    else
                                    {
                                        SectionPart newSectionPart = newSection.SectionParts.ElementAt(section.SectionParts.IndexOf(sectionPart));

                                        var method = AddQuestionToSession(newSectionPart, sectionPart.Questions);
                                        if (!method.IsOK)
                                        {
                                            methodResult.AddErrorBadRequest(method.ErrorMessages);
                                        }
                                    }
                                }
                            }

                            //var correctCount = newSection.SectionParts.SelectMany(x => x.SectionQuestions).Select(x => x.Question).Sum(x => x!.CorrectTotal);
                            //if (!SectionValidation.IsCheckSection(sectionGroup.CourseSkill, section.DisplayOrder, correctCount))
                            //{
                            //    methodResult.AddErrorBadRequest(nameof(EnumPlacementTestErrorCode.MustCorrectScore), nameof(section.DisplayOrder), section.DisplayOrder);
                            //    return methodResult;
                            //}
                        }
                        else
                        {
                            var method = AddQuestionToSession(newSection, section.Questions);
                            if (!method.IsOK)
                            {
                                methodResult.AddErrorBadRequest(method.ErrorMessages);
                                return methodResult;
                            }
                        }
                    }
                    else if (sectionGroup.CourseSkill == EnumCourseSkill.Speaking)
                    {
                        //if (section.SectionTimeCodes == null || section.SectionTimeCodes.Count == 0)
                        //{
                        //    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(sectionGroup.CourseSkill));
                        //    return methodResult;
                        //}
                        if (section.SectionTimeCodes != null && section.SectionTimeCodes.Any())
                        {
                            foreach (var sectionTimeCode in section.SectionTimeCodes)
                            {
                                if (sectionTimeCode == null)
                                {
                                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(sectionTimeCode));
                                    return methodResult;
                                }
                                else
                                {
                                    SectionTimeCode newSectionTimeCode = newSection.SectionTimeCodes.ElementAt(section.SectionTimeCodes.IndexOf(sectionTimeCode));
                                }
                            }
                        }
                    }

                    if (!newSection.IsValid())
                    {
                        methodResult.AddErrorBadRequest(newSection.ErrorMessages);
                        return methodResult;
                    }
                }
            }

            return methodResult;
        }

        public async Task<bool> DeleteSectionGroup(IList<SectionGroup> sectionGroups, IList<SectionQuestion> sectionQuestions, IList<Question> questions)
        {
            sectionGroups.ForEach(async x => await _sectionGroupRepository.DeleteAsync(x));
            await _sectionGroupRepository.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);

            sectionQuestions.ForEach(async x => await _sectionQuestionRepository.DeleteAsync(x));
            await _sectionQuestionRepository.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);

            questions.ForEach(async x => await _questionRepository.DeleteAsync(x));
            await _questionRepository.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);

            return true;
        }

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
            if (indexProcess < index)
            {
                return resultStatus;
            }
            else if (indexProcess == index)
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
            var sectionGroup = sectionGroups.FirstOrDefault(x => !x.SectionGroupResults.Any() || x.SectionGroupResults.Any(x => (Guid)x.GetPropValue(objectResultType) == objectResultId && x.Status != EnumResultStatus.Done));
            return sectionGroup != null ? sectionGroups.IndexOf(sectionGroup) : null;
        }
    }
}
