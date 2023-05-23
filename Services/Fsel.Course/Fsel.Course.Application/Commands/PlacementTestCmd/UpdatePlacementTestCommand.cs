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
    public class UpdatePlacementTestCommand : UpdatePlacementTestCommandModel, IRequest<MethodResult<PlacementTestModel>>
    {
    }

    public class UpdatePlacementTestCommandHandler : IRequestHandler<UpdatePlacementTestCommand, MethodResult<PlacementTestModel>>
    {
        private readonly IPlacementTestRepository _placementTestRepository;
        private readonly ISectionRepository _sectionRepository;
        private readonly QuestionTypeConverter _questionTypeConverter;
        private readonly ISectionPartRepository _sectionPartRepository;
        private readonly ISectionGroupRepository _sectionGroupRepository;
        private readonly IMapper _mapper;

        public UpdatePlacementTestCommandHandler(IPlacementTestRepository placementTestRepository,
            ISectionRepository sectionRepository,
            QuestionTypeConverter questionTypeConverter,
            ISectionPartRepository sectionPartRepository,
            ISectionGroupRepository sectionGroupRepository,
            IMapper mapper)
        {
            _placementTestRepository = placementTestRepository;
            _sectionRepository = sectionRepository;
            _questionTypeConverter = questionTypeConverter;
            _sectionPartRepository = sectionPartRepository;
            _sectionGroupRepository = sectionGroupRepository;
            _mapper = mapper;
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

            List<SectionGroup> sectionGroups = placementTest.PlacementTestSections.Select(x => x.SectionGroup ?? new SectionGroup()).ToList();
            List<Section> sections = sectionGroups.SelectMany(x => x.Sections).ToList();
            List<SectionPart> sectionParts = sections.SelectMany(x => x.SectionParts).ToList();
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
                        var correctCount = newSection.SectionParts.SelectMany(x => x.SectionQuestions).Select(x => x.Question).Sum(x => x!.CorrectTotal);
                        if (!ValidateSection.IsCheckSection(newSectionGroup.CourseSkill, section.DisplayOrder, correctCount))
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

                        foreach (var question in section.Questions)
                        {
                            GetSectionQuestion(methodResult, question, newSection, null);
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

                placementTest = _placementTestRepository.Update(placementTest);
                await _placementTestRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<PlacementTestModel>(placementTest);
                return methodResult;
            });

            return methodResult;
        }

        private void GetSectionQuestion(MethodResult<PlacementTestModel> methodResult, UpdateQuestionCommandModel question, Section? section, SectionPart? sectionPart)
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
