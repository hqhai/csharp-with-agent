// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Enums.ErrorCodes;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.CommandModels.PlacementTests;
using Fsel.Course.Domain.Models.CommandModels.Questions;
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
        private readonly SectionConverter _sectionConverter;

        public CreatePlacementTestCommandHandler(IPlacementTestRepository placementTestRepository,
            QuestionTypeConverter questionTypeConverter,
            IMapper mapper,
            SectionConverter sectionConverter)
        {
            _placementTestRepository = placementTestRepository;
            _questionTypeConverter = questionTypeConverter;
            _mapper = mapper;
            _sectionConverter = sectionConverter;
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
            placementTest.IsActive = false;
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
                                    SectionPart newSectionPart = newSection.SectionParts.ElementAt(section.SectionParts.IndexOf(sectionPart));

                                    var method = _sectionConverter.AddQuestionToSession(newSectionPart, sectionPart.Questions);
                                    if (!method.IsOK)
                                    {
                                        methodResult.AddError(method.ErrorMessages);
                                    }
                                }
                            }
                            var correctCount = newSection.SectionParts.SelectMany(x => x.SectionQuestions).Select(x => x.Question).Sum(x => x!.CorrectTotal);
                            if (!SectionValidation.IsCheckSection(newSectionGroup.CourseSkill, section.DisplayOrder, correctCount))
                            {
                                methodResult.AddErrorBadRequest(nameof(EnumPlacementTestErrorCode.PlacementTestMustCorrectScore), nameof(section.DisplayOrder), section.DisplayOrder);
                                return methodResult;
                            }
                        }
                        else
                        {
                            if (section.Questions == null || section.Questions.Count == 0)
                            {
                                methodResult.AddErrorBadRequest(nameof(EnumQuestionErrorCode.QuestionsNull), nameof(section.Questions));
                                return methodResult;
                            }
                            var method = _sectionConverter.AddQuestionToSession(section, section.Questions);
                            if (!method.IsOK)
                            {
                                methodResult.AddError(method.ErrorMessages);
                            }
                        }

                        if (!newSection.IsValid())
                        {
                            methodResult.AddErrorBadRequest(newSection.ErrorMessages);
                            return methodResult;
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

            if (!methodResult.IsOK)
            {
                return methodResult;
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
