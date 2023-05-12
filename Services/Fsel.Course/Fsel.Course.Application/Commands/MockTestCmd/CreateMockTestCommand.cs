// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.MockTestCmd
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.MockTests;
    using Fsel.Course.Domain.Models.CommandModels.Questions;
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

            #region Validation

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

            foreach (var sectionGroup in request.SectionGroups)
            {
                if (sectionGroup == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSectionGroupErrorCode.SectionGroupNull), nameof(sectionGroup));
                    return methodResult;
                }
                else
                {
                    if (sectionGroup.Sections == null || sectionGroup.Sections.Count == 0)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSectionErrorCode.SectionsNull), nameof(sectionGroup.Sections));
                        return methodResult;
                    }

                    SectionGroup newSectionGroup = _mapper.Map<SectionGroup>(sectionGroup);
                    foreach (var section in sectionGroup.Sections)
                    {
                        Section newSection = newSectionGroup.Sections.ElementAt(sectionGroup.Sections.IndexOf(section));
                        if (section.SectionParts != null && section.Questions != null && section.SectionParts.Count > 0 && section.Questions.Count > 0)
                        {
                            methodResult.AddErrorBadRequest(nameof(EnumSectionErrorCode.OnlyOneOfTwoSectionPartsOrQuestions), nameof(section.SectionParts), nameof(section.Questions));
                            return methodResult;
                        }

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
                                SectionPart newSectionPart = newSection.SectionParts.ElementAt(section.SectionParts.IndexOf(sectionPart));
                                if (sectionPart.Questions == null || sectionPart.Questions.Count == 0)
                                {
                                    methodResult.AddErrorBadRequest(nameof(EnumQuestionErrorCode.QuestionsNull), nameof(sectionPart.Questions));
                                    return methodResult;
                                }
                                foreach (var question in sectionPart.Questions)
                                {
                                    GetSectionQuestion(methodResult, question, null, newSectionPart);
                                }
                                if (!newSectionPart.IsValid())
                                {
                                    methodResult.AddErrorBadRequest(newSectionPart.ErrorMessages);
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

                    mockTest.MockTestSections.Add(new MockTestSection
                    {
                        SectionGroup = newSectionGroup
                    });
                    if (!newSectionGroup.IsValid())
                    {
                        methodResult.AddErrorBadRequest(newSectionGroup.ErrorMessages);
                        return methodResult;
                    }
                }
            }

            if (!methodResult.IsOK)
            {
                return methodResult;
            }

            #endregion Validation

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

        private void GetSectionQuestion(MethodResult<MockTestModel> methodResult, CreateQuestionCommandModel question, Section? section, SectionPart? sectionPart)
        {
            if (question == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumQuestionErrorCode.QuestionNull), nameof(question));
            }
            else
            {
                Question newQuestion = _mapper.Map<Question>(question);
                var (config, correctTotal) = _questionTypeConverter.QuestionTypeConverterObject(question.Config, newQuestion.QuestionType, isShowCorrectTotal: !question.Ungraded, false);
                if (config == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumVideoErrorCode.ConfigIsInTheWrongFormat), nameof(question.Config), question.Config);
                }
                if (!newQuestion.IsValid())
                {
                    methodResult.AddErrorBadRequest(newQuestion.ErrorMessages);
                }
                newQuestion.CorrectTotal = correctTotal;

                if (section != null)
                {
                    section.SectionQuestions.Add(new SectionQuestion
                    {
                        Question = newQuestion,
                    });
                }
                else if (sectionPart != null)
                {
                    sectionPart.SectionQuestions.Add(new SectionQuestion
                    {
                        Question = newQuestion,
                    });
                }
            }
        }
    }
}
