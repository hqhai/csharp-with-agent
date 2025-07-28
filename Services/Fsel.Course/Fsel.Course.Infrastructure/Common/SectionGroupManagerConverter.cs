// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Common
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.Questions;
    using Fsel.Course.Domain.Models.CommandModels.Sections;
    using Fsel.Shared.Enums;
    using static Fsel.Shared.Constants.ValueSettings;

    public class SectionGroupManagerConverter
    {
        private readonly QuestionTypeConverter _questionTypeConverter;
        private readonly QuestionConverter _questionConverter;
        private readonly IMapper _mapper;
        private readonly ISectionGroupRepository _sectionGroupRepository;
        private readonly ISectionQuestionRepository _sectionQuestionRepository;
        private readonly IQuestionRepository _questionRepository;
        private int NumberQuestion = 0;
        private static int MaxCorrectTotalSkill = 40;

        public SectionGroupManagerConverter(QuestionTypeConverter questionTypeConverter, QuestionConverter questionConverter, IMapper mapper, ISectionGroupRepository sectionGroupRepository, ISectionQuestionRepository sectionQuestionRepository, IQuestionRepository questionRepository)
        {
            _questionTypeConverter = questionTypeConverter;
            _questionConverter = questionConverter;
            _mapper = mapper;
            _sectionGroupRepository = sectionGroupRepository;
            _sectionQuestionRepository = sectionQuestionRepository;
            _questionRepository = questionRepository;
        }

        public VoidMethodResult AddQuestionToSession(dynamic section, IList<CreateQuestionCommandModel>? questionModels)
        {
            VoidMethodResult methodResult = new VoidMethodResult();
            var questions = _mapper.Map<IList<Question>>(questionModels);
            //if (questions == null || !questions.Any())
            //{
            //    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(questions));
            //    return methodResult;
            //}
            if (questions == null || !questions.Any())
            {
                return methodResult;
            }

            foreach (var question in questions)
            {
                var newQuestion = _mapper.Map<Question>(question);
                var method = _questionConverter.HandleQuestion(newQuestion);
                if (!method.IsOK)
                {
                    methodResult.AddErrorBadRequest(method.ErrorMessages);
                    return methodResult;
                }
                if (method.Result != null)
                {
                    var data = Enumerable.Range(NumberQuestion + 1, method.Result.CorrectTotal).ToList();
                    method.Result.SubQuestionIndexs = data;
                    NumberQuestion += method.Result.CorrectTotal;
                }
                section.SectionQuestions.Add(new SectionQuestion
                {
                    Question = method.Result,
                });
            }

            if (!section.IsValid())
            {
                methodResult.AddErrorBadRequest(section.ErrorMessages);
                return methodResult;
            }

            return methodResult;
        }

        public VoidMethodResult AddSessionToSessionGroup(dynamic sectionGroup, IList<CreateSectionCommandModel>? sectionModels, EnumCourseType type = EnumCourseType.Academic)
        {
            VoidMethodResult methodResult = new VoidMethodResult();
            //if (type == EnumCourseType.Ielts)
            //{
            //    var methodSG = ValidateSectionGroup(sectionGroup);
            //    if (!methodSG.IsOK)
            //    {
            //        methodResult.AddErrorBadRequest(methodSG.ErrorMessages);
            //        return methodResult;
            //    }
            //}

            //if (sectionModels == null || sectionModels.Count == 0)
            //{
            //    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(sectionGroup.Sections));
            //    return methodResult;
            //}
            if (sectionModels == null || sectionModels.Count == 0)
            {
                return methodResult;
            }
            IList<Section> sections = sectionGroup.Sections;

            foreach (var section in sectionModels)
            {
                //if (section == null)
                //{
                //    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(section));
                //    return methodResult;
                //}
                if (section == null)
                {
                    continue;
                }
                var index = sectionModels.IndexOf(section) + 1;
                Section newSection = sections.ElementAt(sectionModels.IndexOf(section));
                if (sectionGroup.CourseSkill != EnumCourseSkill.Speaking && sectionGroup.CourseSkill != EnumCourseSkill.Writing)
                {
                    //if (section.SectionParts != null && section.Questions != null && section.SectionParts.Count > 0 && section.Questions.Count > 0)
                    //{
                    //    methodResult.AddErrorBadRequest(nameof(EnumSectionErrorCode.OnlyOneOfTwoSectionPartsOrQuestions));
                    //    return methodResult;
                    //}
                    //if (section.SectionParts == null || section.SectionParts.Count == 0)
                    //{
                    //    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(section.SectionParts));
                    //    return methodResult;
                    //}
                    if (section.SectionParts != null && section.SectionParts.Count > 0 && type == EnumCourseType.Ielts)
                    {
                        foreach (var sectionPart in section.SectionParts)
                        {
                            //if (sectionPart == null)
                            //{
                            //    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(sectionPart));
                            //    return methodResult;
                            //}
                            if (sectionPart != null)
                            {
                                SectionPart newSectionPart = newSection.SectionParts.ElementAt(section.SectionParts.IndexOf(sectionPart));
                                if (sectionPart.Questions != null && sectionPart.Questions.Count > 0)
                                {
                                    var method = AddQuestionToSession(newSectionPart, sectionPart.Questions);
                                    if (!method.IsOK)
                                    {
                                        methodResult.AddErrorBadRequest(method.ErrorMessages);
                                        return methodResult;
                                    }
                                }
                                if (!newSectionPart.IsValid())
                                {
                                    methodResult.AddErrorBadRequest(newSectionPart.ErrorMessages);
                                    return methodResult;
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
                    if (section.Questions != null && section.Questions.Count > 0)
                    {
                        var method = AddQuestionToSession(newSection, section.Questions);
                        if (!method.IsOK)
                        {
                            methodResult.AddErrorBadRequest(method.ErrorMessages);
                            return methodResult;
                        }

                        //var correctCount = newSection.SectionQuestions.Select(x => x.Question).Sum(x => x!.CorrectTotal);
                        //if (type == EnumCourseType.Ielts && !SectionValidation.IsCheckSection(sectionGroup.CourseSkill, index, correctCount))
                        //{
                        //    methodResult.AddErrorBadRequest(nameof(EnumSectionErrorCode.ExceededValidScore), new Error[]{
                        //        new Error
                        //        {
                        //            FieldName = nameof(index),
                        //            ErrorValues = new List<object>{ index }
                        //        },
                        //        new Error
                        //        {
                        //            FieldName = nameof(sectionGroup.CourseSkill),
                        //            ErrorValues = new List<object>{ correctCount }
                        //        }
                        //    });
                        //    return methodResult;
                        //}
                    }
                }
                else if (sectionGroup.CourseSkill == EnumCourseSkill.Speaking)
                {
                    //if (section.SectionTimeCodes == null || section.SectionTimeCodes.Count == 0)
                    //{
                    //    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(sectionGroup.CourseSkill));
                    //    return methodResult;
                    //}
                    if (section.SectionTimeCodes == null || section.SectionTimeCodes.Count == 0)
                    {
                        continue;
                    }
                    foreach (var sectionTimeCode in section.SectionTimeCodes)
                    {
                        //if (sectionTimeCode == null)
                        //{
                        //    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(sectionTimeCode));
                        //    return methodResult;
                        //}
                        if (sectionTimeCode == null)
                        {
                            continue;
                        }
                        SectionTimeCode newSectionTimeCode = newSection.SectionTimeCodes.ElementAt(section.SectionTimeCodes.IndexOf(sectionTimeCode));
                        if (!newSectionTimeCode.IsValid())
                        {
                            methodResult.AddErrorBadRequest(newSectionTimeCode.ErrorMessages);
                            return methodResult;
                        }
                    }
                }
                if (!newSection.IsValid())
                {
                    methodResult.AddErrorBadRequest(newSection.ErrorMessages);
                    return methodResult;
                }
            }
            var listSkillScore = new List<EnumCourseSkill> { EnumCourseSkill.Reading, EnumCourseSkill.Listening };
            if (type == EnumCourseType.Ielts && listSkillScore.Any(x => x == sectionGroup.CourseSkill))
            {
                var correctTotal = sections.SelectMany(x => x.SectionQuestions).Any() ? sections.SelectMany(x => x.SectionQuestions).Select(x => x.Question).Sum(x => x!.CorrectTotal) :
                                                                                        sections.SelectMany(x => x.SectionParts).SelectMany(x => x.SectionQuestions).Select(x => x.Question).Sum(x => x!.CorrectTotal);
                if (correctTotal > MaxCorrectTotalSkill)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumMockTestErrorCode.BelowOrEqualTo40), nameof(correctTotal), correctTotal);
                    return methodResult;
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

        public VoidMethodResult ValidateSectionGroup(SectionGroup sectionGroup)
        {
            VoidMethodResult methodResult = new VoidMethodResult();
            ArgumentNullException.ThrowIfNull(sectionGroup);
            if (sectionGroup.Sections == null || sectionGroup.Sections.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(sectionGroup.Sections));
                return methodResult;
            }
            if (sectionGroup.CourseSkill == EnumCourseSkill.Reading && sectionGroup.Sections.Count != SectionGroupIELST.MaxSectionSkillReading)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(sectionGroup.Sections), sectionGroup.Sections.Count);
                return methodResult;
            }
            else if (sectionGroup.CourseSkill == EnumCourseSkill.Listening && sectionGroup.Sections.Count != SectionGroupIELST.MaxSectionSkillListening)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(sectionGroup.Sections), sectionGroup.Sections.Count);
                return methodResult;
            }
            else if (sectionGroup.CourseSkill == EnumCourseSkill.Writing && sectionGroup.Sections.Count != SectionGroupIELST.MaxSectionSkillWriting)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(sectionGroup.Sections), sectionGroup.Sections.Count);
                return methodResult;
            }
            return methodResult;
        }
    }
}
