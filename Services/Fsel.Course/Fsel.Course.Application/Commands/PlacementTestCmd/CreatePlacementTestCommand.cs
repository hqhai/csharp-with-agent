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
                    if (sectionGroup.Sections == null || sectionGroup.Sections.Count == 0)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSectionErrorCode.SectionsNull), nameof(sectionGroup.Sections));
                        return methodResult;
                    }

                    foreach (var section in sectionGroup.Sections)
                    {
                        if (section.SectionParts != null && section.Questions != null && section.SectionParts.Count > 0 && section.Questions.Count > 0)
                        {
                            methodResult.AddErrorBadRequest(nameof(EnumSectionErrorCode.OnlyOneOfTwoSectionPartsOrQuestions), nameof(section.SectionParts), nameof(section.Questions));
                            return methodResult;
                        }
                        if (placementTest.Level == EnumPlacementTestLevel.IELTS)
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
                            }
                        }
                    }
                    foreach (var section in newSectionGroup.Sections)
                    {
                        if (placementTest.Level == EnumPlacementTestLevel.IELTS)
                        {
                            foreach (var sectionPart in section.SectionParts)
                            {
                                var questions = sectionGroup.Sections.Where(x => x.SectionParts != null).SelectMany(x => x.SectionParts!.Where(x => x.Questions != null)).SelectMany(x => x.Questions!).ToList();
                                foreach (var question in questions)
                                {
                                    Question newQuestion = _mapper.Map<Question>(question);
                                    var (config, correctTotal) = _questionTypeConverter.QuestionTypeConverterObject(question.Config, newQuestion.QuestionType, isShowCorrectTotal: !question.Ungraded, false);
                                    if (config == null)
                                    {
                                        methodResult.AddErrorBadRequest(nameof(EnumVideoErrorCode.ConfigIsInTheWrongFormat), nameof(question.Config), question.Config);
                                    }
                                    sectionPart.SectionQuestions.Add(new SectionQuestion
                                    {
                                        Question = newQuestion,
                                        SectionPart = sectionPart
                                    });
                                    if (!newQuestion.IsValid())
                                    {
                                        methodResult.AddErrorBadRequest(newQuestion.ErrorMessages);
                                        return methodResult;
                                    }
                                }
                            }
                        }
                        else
                        {
                            var questions = sectionGroup.Sections.Where(x => x.Questions != null).SelectMany(x => x.Questions!).ToList();
                            foreach (var question in questions)
                            {
                                Question newQuestion = _mapper.Map<Question>(question);
                                var (config, correctTotal) = _questionTypeConverter.QuestionTypeConverterObject(question.Config, newQuestion.QuestionType, isShowCorrectTotal: !question.Ungraded, false);
                                if (config == null)
                                {
                                    methodResult.AddErrorBadRequest(nameof(EnumVideoErrorCode.ConfigIsInTheWrongFormat), nameof(question.Config), question.Config);
                                }
                                section.SectionQuestions.Add(new SectionQuestion
                                {
                                    Question = newQuestion,
                                    Section = section
                                });
                                if (!newQuestion.IsValid())
                                {
                                    methodResult.AddErrorBadRequest(newQuestion.ErrorMessages);
                                    return methodResult;
                                }
                            }
                        }
                    }

                    placementTest.PlacementTestSections.Add(new PlacementTestSection
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
