// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.MockTestCmd
{
    using System;
    using System.Collections.Generic;
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
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class CreateMockTestCommand : CreateMockTestCommandModel, IRequest<MethodResult<MockTestModel>>
    {
    }

    public class CreateMockTestCommandHandler : IRequestHandler<CreateMockTestCommand, MethodResult<MockTestModel>>
    {
        private readonly IMapper _mapper;
        private readonly IMockTestRepository _mockTestRepository;
        private readonly QuestionTypeConverter _questionTypeConverter;

        public CreateMockTestCommandHandler(IMapper mapper, IMockTestRepository mockTestRepository, QuestionTypeConverter questionTypeConverter)
        {
            _mapper = mapper;
            _mockTestRepository = mockTestRepository;
            _questionTypeConverter = questionTypeConverter;
        }

        public async Task<MethodResult<MockTestModel>> Handle(CreateMockTestCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<MockTestModel> methodResult = new MethodResult<MockTestModel>();
            if (request.SectionGroups == null || request.SectionGroups.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSectionGroupErrorCode.SectionGroupsNull), nameof(request.SectionGroups));
                return methodResult;
            }

            MockTest mockTest = _mapper.Map<MockTest>(request);
            if (!mockTest.IsValid())
            {
                methodResult.AddErrorBadRequest(mockTest.ErrorMessages);
                return methodResult;
            }

            var isType = mockTest.MockTestType == EnumMockTestType.SkillMockTest;

            foreach (var sectionGroup in request.SectionGroups)
            {
                if (sectionGroup == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSectionGroupErrorCode.SectionGroupNull), nameof(sectionGroup));
                    return methodResult;
                }
                else
                {
                    SectionGroup newSectionGroup = _mapper.Map<SectionGroup>(sectionGroup);
                    newSectionGroup.Sections = new List<Section>();
                    mockTest.MockTestSections.Add(new MockTestSection
                    {
                        SectionGroup = newSectionGroup,
                    });
                    if (sectionGroup.Sections == null || sectionGroup.Sections.Count == 0)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSectionErrorCode.SectionsNull), nameof(sectionGroup.Sections));
                        return methodResult;
                    }
                    foreach (var section in sectionGroup.Sections)
                    {
                        if (section == null)
                        {
                            methodResult.AddErrorBadRequest(nameof(EnumSectionErrorCode.SectionNull), nameof(section), section);
                            return methodResult;
                        }
                        else
                        {
                            Section newSection = _mapper.Map<Section>(section);
                            newSection.SectionParts = new List<SectionPart>();
                            newSectionGroup.Sections.Add(newSection);
                            if (section.SectionParts != null && section.Questions != null && section.SectionParts.Count > 0 && section.Questions.Count > 0)
                            {
                                methodResult.AddErrorBadRequest(nameof(EnumSectionErrorCode.OnlyOneOfTwoSectionPartsOrQuestions), nameof(section.SectionParts), nameof(section.Questions));
                                return methodResult;
                            }
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
                                        SectionPart newSectionPart = _mapper.Map<SectionPart>(sectionPart);
                                        newSectionPart.SectionQuestions = new List<SectionQuestion>();
                                        newSection.SectionParts.Add(newSectionPart);
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
                                                Question newQuestion = _mapper.Map<Question>(question);
                                                var (config, correctTotal) = _questionTypeConverter.QuestionTypeConverterObject(question.Config, newQuestion.QuestionType, isShowCorrectTotal: !question.Ungraded, false);
                                                if (config == null)
                                                {
                                                    methodResult.AddErrorBadRequest(nameof(EnumVideoErrorCode.ConfigIsInTheWrongFormat), nameof(question.Config), question.Config);
                                                }
                                                newSectionPart.SectionQuestions.Add(new SectionQuestion
                                                {
                                                    Question = newQuestion,
                                                    SectionPart = newSectionPart,
                                                    Section = null
                                                });
                                                if (!newQuestion.IsValid())
                                                {
                                                    methodResult.AddErrorBadRequest(newQuestion.ErrorMessages);
                                                    return methodResult;
                                                }
                                            }
                                        }
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
                                    methodResult.AddErrorBadRequest(nameof(EnumQuestionErrorCode.QuestionsNull), nameof(section.Questions));
                                    return methodResult;
                                }
                                foreach (var question in section.Questions)
                                {
                                    if (question == null)
                                    {
                                        methodResult.AddErrorBadRequest(nameof(EnumQuestionErrorCode.QuestionNull), nameof(question));
                                        return methodResult;
                                    }
                                    else
                                    {
                                        Question newQuestion = _mapper.Map<Question>(question);
                                        var (config, correctTotal) = _questionTypeConverter.QuestionTypeConverterObject(question.Config, newQuestion.QuestionType, isShowCorrectTotal: !question.Ungraded, false);
                                        if (config == null)
                                        {
                                            methodResult.AddErrorBadRequest(nameof(EnumVideoErrorCode.ConfigIsInTheWrongFormat), nameof(question.Config), question.Config);
                                        }
                                        newSection.SectionQuestions.Add(new SectionQuestion
                                        {
                                            Question = newQuestion,
                                            Section = newSection,
                                            SectionPart = null
                                        });
                                        if (!newQuestion.IsValid())
                                        {
                                            methodResult.AddErrorBadRequest(newQuestion.ErrorMessages);
                                            return methodResult;
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
                    if (!newSectionGroup.IsValid())
                    {
                        methodResult.AddErrorBadRequest(newSectionGroup.ErrorMessages);
                        return methodResult;
                    }
                }
            }
            await _mockTestRepository.ExecuteTransactionAsync(async () =>
            {
                mockTest = _mockTestRepository.Add(mockTest);
                await _mockTestRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<MockTestModel>(mockTest);
                return methodResult;
            });

            return methodResult;
        }
    }
}
