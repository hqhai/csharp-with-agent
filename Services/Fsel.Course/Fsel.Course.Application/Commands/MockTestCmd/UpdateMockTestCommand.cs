// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.MockTestCmd
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.MockTests;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class UpdateMockTestCommand : UpdateMockTestCommandModel, IRequest<MethodResult<MockTestModel>>
    {
    }

    public class UpdateMockTestCommandHandler : IRequestHandler<UpdateMockTestCommand, MethodResult<MockTestModel>>
    {
        private readonly IMapper _mapper;
        private readonly IMockTestRepository _mockTestRepository;
        private readonly ISectionRepository _sectionRepository;
        private readonly QuestionTypeConverter _questionTypeConverter;
        private readonly ISectionPartRepository _sectionPartRepository;
        private readonly IQuestionRepository _questionRepository;
        private readonly ISectionGroupRepository _sectionGroupRepository;
        private readonly ISectionQuestionRepository _sectionQuestionRepository;

        public UpdateMockTestCommandHandler(IMapper mapper
            , IMockTestRepository mockTestRepository
            , ISectionRepository sectionRepository
            , QuestionTypeConverter questionTypeConverter
            , ISectionPartRepository sectionPartRepository
            , IQuestionRepository questionRepository
            , ISectionGroupRepository sectionGroupRepository
            , ISectionQuestionRepository sectionQuestionRepository)
        {
            _mapper = mapper;
            _mockTestRepository = mockTestRepository;
            _sectionRepository = sectionRepository;
            _questionTypeConverter = questionTypeConverter;
            _sectionPartRepository = sectionPartRepository;
            _questionRepository = questionRepository;
            _sectionGroupRepository = sectionGroupRepository;
            _sectionQuestionRepository = sectionQuestionRepository;
        }

        public async Task<MethodResult<MockTestModel>> Handle(UpdateMockTestCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<MockTestModel> methodResult = new MethodResult<MockTestModel>();
            if (request.SectionGroups == null || request.SectionGroups.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSectionGroupErrorCode.SectionGroupsNull), nameof(request.SectionGroups));
                return methodResult;
            }
            var mockTest = await _mockTestRepository.GetIncludeByIdAsync(request.Id);
            if (mockTest == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumMockTestErrorCode.MockTestsNotExist), nameof(request.Id), request?.Id);
                return methodResult;
            }
            if (mockTest.IsActive)
            {
                methodResult.AddErrorBadRequest(nameof(EnumMockTestErrorCode.MockTestInActiveState), nameof(mockTest.IsActive), mockTest.IsActive);
                return methodResult;
            }
            var isType = mockTest.MockTestType == EnumMockTestType.SkillMockTest;
            List<SectionGroup> sectionGroups = mockTest.MockTestSections.Select(x => x.SectionGroup ?? new SectionGroup()).ToList();
            List<Section> sections = sectionGroups.SelectMany(x => x.Sections).ToList();
            List<SectionPart> sectionParts = sections.SelectMany(x => x.SectionParts).ToList();
            List<Question>? questions = null;
            List<SectionQuestion>? sectionQuestions = null;
            if (mockTest.MockTestType == EnumMockTestType.SkillMockTest)
            {
                sectionQuestions = sectionParts.SelectMany(x => x.SectionQuestions).ToList();
                questions = sectionQuestions.Select(x => x.Question ?? new Question()).ToList();
            }
            else
            {
                sectionQuestions = sections.SelectMany(x => x.SectionQuestions).ToList();
                questions = sections.SelectMany(x => x.SectionQuestions).Select(x => x.Question ?? new Question()).ToList();
            }
            _mapper.Map(request, mockTest);
            mockTest.MockTestSections.Clear();

            foreach (var sectionGroup in request.SectionGroups)
            {
                if (sectionGroup == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSectionGroupErrorCode.SectionGroupNull), nameof(sectionGroup));
                    return methodResult;
                }
                var newSectionGroup = _mapper.Map<SectionGroup>(sectionGroup);
                newSectionGroup.Sections.Clear();
                if (sectionGroup.Sections == null || sectionGroup.Sections.Count == 0)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSectionErrorCode.SectionsNull), nameof(sectionGroup.Sections));
                    return methodResult;
                }
                foreach (var section in sectionGroup.Sections)
                {
                    if (section == null)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSectionErrorCode.SectionNull), nameof(section));
                        return methodResult;
                    }
                    var newSection = _mapper.Map<Section>(section);

                    if (section.SectionParts != null && section.Questions != null && section.SectionParts.Count > 0 && section.Questions.Count > 0)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSectionErrorCode.OnlyOneOfTwoSectionPartsOrQuestions));
                        return methodResult;
                    }

                    newSection.SectionParts.Clear();
                    if (isType)
                    {
                        if (section.SectionParts == null || section.SectionParts.Count == 0)
                        {
                            methodResult.AddErrorBadRequest(nameof(EnumSectionPartErrorCode.SectionPartsNull), nameof(section.SectionParts));
                            return methodResult;
                        }
                        foreach (var sectionPart in section.SectionParts)
                        {
                            if (sectionPart == null)
                            {
                                methodResult.AddErrorBadRequest(nameof(EnumSectionPartErrorCode.SectionPartNull), nameof(sectionPart), sectionPart);
                                return methodResult;
                            }
                            else
                            {
                                var newSectionPart = _mapper.Map<SectionPart>(sectionPart);
                                newSectionPart.SectionQuestions.Clear();
                                if (sectionPart.Questions == null || sectionPart.Questions.Count == 0)
                                {
                                    methodResult.AddErrorBadRequest(nameof(EnumQuestionErrorCode.QuestionsNull), nameof(sectionPart.Questions));
                                    return methodResult;
                                }
                                foreach (var question in sectionPart.Questions)
                                {
                                    if (question == null)
                                    {
                                        methodResult.AddErrorBadRequest(nameof(EnumQuestionErrorCode.QuestionNull), nameof(question));
                                        return methodResult;
                                    }
                                    else
                                    {
                                        var newQuestion = _mapper.Map<Question>(question);
                                        var (config, correctTotal) = _questionTypeConverter.QuestionTypeConverterObject(question.Config, newQuestion.QuestionType, isShowCorrectTotal: !question.Ungraded, false);
                                        if (config == null)
                                        {
                                            methodResult.AddErrorBadRequest(nameof(EnumVideoErrorCode.ConfigIsInTheWrongFormat), nameof(question.Config), question.Config);
                                        }
                                        newSectionPart.SectionQuestions.Add(new SectionQuestion
                                        {
                                            Question = newQuestion,
                                            SectionPart = newSectionPart,
                                        });
                                        if (!newQuestion.IsValid())
                                        {
                                            methodResult.AddErrorBadRequest(newQuestion.ErrorMessages);
                                            return methodResult;
                                        }
                                    }
                                }
                                newSection.SectionParts.Add(newSectionPart);
                                if (!newSectionPart.IsValid())
                                {
                                    methodResult.AddErrorBadRequest(newSectionPart.ErrorMessages);
                                    return methodResult;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (section.Questions == null || section.Questions.Count == 0)
                        {
                            methodResult.AddErrorBadRequest(nameof(EnumQuestionErrorCode.QuestionNull), nameof(section.Questions));
                            return methodResult;
                        }

                        foreach (var question in section.Questions)
                        {
                            if (question == null)
                            {
                                methodResult.AddErrorBadRequest(nameof(EnumQuestionErrorCode.QuestionNull), nameof(question), question);
                            }
                            else
                            {
                                var newQuestion = _mapper.Map<Question>(question);
                                var (config, correctTotal) = _questionTypeConverter.QuestionTypeConverterObject(question.Config, newQuestion.QuestionType, isShowCorrectTotal: !question.Ungraded, false);
                                if (config == null)
                                {
                                    methodResult.AddErrorBadRequest(nameof(EnumVideoErrorCode.ConfigIsInTheWrongFormat), nameof(question.Config), question.Config);
                                }
                                newSection.SectionQuestions.Add(new SectionQuestion
                                {
                                    Question = newQuestion,
                                    Section = newSection,
                                });
                                if (!newQuestion.IsValid())
                                {
                                    methodResult.AddErrorBadRequest(newQuestion.ErrorMessages);
                                }
                            }
                        }
                    }

                    newSectionGroup.Sections.Add(newSection);

                    if (!newSection.IsValid())
                    {
                        methodResult.AddErrorBadRequest(newSection.ErrorMessages);
                    }
                }
                mockTest.MockTestSections.Add(new MockTestSection { SectionGroup = newSectionGroup });
                if (!newSectionGroup.IsValid())
                {
                    methodResult.AddErrorBadRequest(newSectionGroup.ErrorMessages);
                }
            }
            if (!mockTest.IsValid())
            {
                methodResult.AddErrorBadRequest(mockTest.ErrorMessages);
                return methodResult;
            }
            else if (!methodResult.IsOK)
            {
                return methodResult;
            }

            await _mockTestRepository.ExecuteTransactionAsync(async () =>
            {
                foreach (var item in sectionGroups)
                {
                    await _sectionGroupRepository.DeleteAsync(item);
                }
                await _sectionGroupRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                foreach (var item in sections)
                {
                    await _sectionRepository.DeleteAsync(item);
                }
                await _sectionRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                if (sectionParts.Count > 0)
                {
                    foreach (var item in sectionParts)
                    {
                        await _sectionPartRepository.DeleteAsync(item);
                    }
                    await _sectionPartRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                }

                foreach (var item in sectionQuestions)
                {
                    await _sectionQuestionRepository.DeleteAsync(item);
                }
                await _sectionQuestionRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                foreach (var item in questions)
                {
                    await _questionRepository.DeleteAsync(item);
                }
                await _questionRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                mockTest = _mockTestRepository.Update(mockTest);
                await _mockTestRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<MockTestModel>(mockTest);
                return methodResult;
            });

            return methodResult;
        }
    }
}
