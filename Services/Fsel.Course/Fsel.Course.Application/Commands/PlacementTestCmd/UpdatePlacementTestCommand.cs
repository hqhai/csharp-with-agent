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
    public class UpdatePlacementTestCommand : UpdatePlacementTestCommandModel, IRequest<MethodResult<PlacementTestModel>>
    {
    }

    public class UpdatePlacementTestCommandHandler : IRequestHandler<UpdatePlacementTestCommand, MethodResult<PlacementTestModel>>
    {
        private readonly IPlacementTestRepository _placementTestRepository;
        private readonly ISectionQuestionRepository _sectionQuestionRepository;
        private readonly IQuestionRepository _questionRepository;
        private readonly ISectionGroupRepository _sectionGroupRepository;
        private readonly IMapper _mapper;
        private readonly SectionConverter _sectionConverter;

        public UpdatePlacementTestCommandHandler(IPlacementTestRepository placementTestRepository,
            ISectionQuestionRepository sectionQuestionRepository,
            IQuestionRepository questionRepository,
            ISectionGroupRepository sectionGroupRepository,
            IMapper mapper,
            SectionConverter sectionConverter)
        {
            _placementTestRepository = placementTestRepository;
            _sectionQuestionRepository = sectionQuestionRepository;
            _questionRepository = questionRepository;
            _sectionGroupRepository = sectionGroupRepository;
            _mapper = mapper;
            _sectionConverter = sectionConverter;
        }

        public async Task<MethodResult<PlacementTestModel>> Handle(UpdatePlacementTestCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<PlacementTestModel> methodResult = new MethodResult<PlacementTestModel>();

            #region Validation

            if (request.SectionGroups == null || request.SectionGroups.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSectionGroupErrorCode.SectionGroupsNull), nameof(request.SectionGroups));
                return methodResult;
            }

            var placementTest = await _placementTestRepository.GetIncludeByIdAsync(request.Id);
            if (placementTest == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumPlacementTestErrorCode.PlacementTestNotExist), nameof(request.Id), request.Id);
                return methodResult;
            }

            if (placementTest.IsActive)
            {
                methodResult.AddErrorBadRequest(nameof(EnumPlacementTestErrorCode.PlacementTestInActiveState), nameof(placementTest.IsActive), placementTest.IsActive);
                return methodResult;
            }

            var sectionGroups = placementTest.PlacementTestSections.Select(x => x.SectionGroup ?? new SectionGroup()).ToList();
            var sections = sectionGroups.SelectMany(x => x.Sections).ToList();
            var sectionParts = sections.SelectMany(x => x.SectionParts).ToList();
            List<Question>? questions = null;
            List<SectionQuestion>? sectionQuestions = null;
            if (placementTest.Level == EnumPlacementTestLevel.IELTS)
            {
                sectionQuestions = sectionParts.SelectMany(x => x.SectionQuestions).ToList();
                questions = sectionQuestions.Select(x => x.Question ?? new Question()).ToList();
            }
            else
            {
                sectionQuestions = sections.SelectMany(x => x.SectionQuestions).ToList();
                questions = sections.SelectMany(x => x.SectionQuestions).Select(x => x.Question ?? new Question()).ToList();
            }

            _mapper.Map(request, placementTest);
            placementTest.PlacementTestSections.Clear();

            foreach (var sectionGroup in request.SectionGroups)
            {
                if (sectionGroup == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSectionGroupErrorCode.SectionGroupNull), nameof(sectionGroup));
                    return methodResult;
                }
                var newSectionGroup = _mapper.Map<SectionGroup>(sectionGroup);
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
                    Section newSection = newSectionGroup.Sections.ElementAt(sectionGroup.Sections.IndexOf(section));
                    if (section.SectionParts != null && section.Questions != null && section.SectionParts.Count > 0 && section.Questions.Count > 0)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSectionErrorCode.OnlyOneOfTwoSectionPartsOrQuestions));
                        return methodResult;
                    }

                    if (request.Level == EnumPlacementTestLevel.IELTS)
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
                            methodResult.AddErrorBadRequest(nameof(EnumQuestionErrorCode.QuestionNull), nameof(section.Questions));
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
                    }
                }
                placementTest.PlacementTestSections.Add(new PlacementTestSection { SectionGroup = newSectionGroup });
                if (!newSectionGroup.IsValid())
                {
                    methodResult.AddErrorBadRequest(newSectionGroup.ErrorMessages);
                }
            }
            if (!placementTest.IsValid())
            {
                methodResult.AddErrorBadRequest(placementTest.ErrorMessages);
                return methodResult;
            }
            else if (!methodResult.IsOK)
            {
                return methodResult;
            }

            #endregion Validation

            await _placementTestRepository.ExecuteTransactionAsync(async () =>
            {
                foreach (var item in sectionGroups)
                {
                    await _sectionGroupRepository.DeleteAsync(item);
                }
                await _sectionGroupRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                if (placementTest.Level != EnumPlacementTestLevel.IELTS)
                {
                    foreach (var item in sectionQuestions)
                    {
                        await _sectionQuestionRepository.DeleteAsync(item);
                    }
                    await _sectionQuestionRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                }

                foreach (var item in questions)
                {
                    await _questionRepository.DeleteAsync(item);
                }
                await _questionRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                placementTest = _placementTestRepository.Update(placementTest);
                await _placementTestRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<PlacementTestModel>(placementTest);
                return methodResult;
            });

            return methodResult;
        }
    }
}
