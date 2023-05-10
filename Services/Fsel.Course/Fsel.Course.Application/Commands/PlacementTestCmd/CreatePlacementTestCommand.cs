// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Enums.ErrorCodes;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.CommandModels.PlacementTests;
using Fsel.Course.Domain.Models.EntityModels;
using Fsel.Course.Infrastructure.Common;
using Fsel.Shared.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Fsel.Course.Application.Commands.PlacementTestCmd
{
    public class CreatePlacementTestCommand : CreatePlacementTestCommandModel, IRequest<MethodResult<PlacementTestModel>>
    {
    }

    public class CreatePlacementTestCommandHandler : IRequestHandler<CreatePlacementTestCommand, MethodResult<PlacementTestModel>>
    {
        private readonly IPlacementTestRepository _placementTestRepository;
        private readonly QuestionTypeConverter _questionTypeConverter;
        private readonly IMapper _mapper;

        public CreatePlacementTestCommandHandler(IPlacementTestRepository placementTestRepository,
            QuestionTypeConverter questionTypeConverter,
            IMapper mapper)
        {
            _placementTestRepository = placementTestRepository;
            _questionTypeConverter = questionTypeConverter;
            _mapper = mapper;
        }

        public async Task<MethodResult<PlacementTestModel>> Handle(CreatePlacementTestCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<PlacementTestModel> methodResult = new MethodResult<PlacementTestModel>();

            #region Validation

            if (request.SectionGroups == null || request.SectionGroups.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSectionGroupErrorCode.SectionGroupsNull), nameof(request.SectionGroups));
                return methodResult;
            }

            PlacementTest placementTest = _mapper.Map<PlacementTest>(request);

            if (!placementTest.IsValid())
            {
                methodResult.AddErrorBadRequest(placementTest.ErrorMessages);
                return methodResult;
            }

            var isType = placementTest.Level == EnumPlacementTestLevel.IELTS;

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
                    placementTest.PlacementTestSections.Add(new PlacementTestSection
                    {
                        SectionGroup = newSectionGroup
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

            #endregion Validation

            await _placementTestRepository.ExecuteTransactionAsync(async () =>
            {
                placementTest = _placementTestRepository.Add(placementTest);
                await _placementTestRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<PlacementTestModel>(placementTest);
                return methodResult;
            });

            return methodResult;
        }
    }
}
