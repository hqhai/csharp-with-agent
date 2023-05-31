// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.MockTestCmd
{
    using System;
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
        private readonly IQuestionRepository _questionRepository;
        private readonly ISectionGroupRepository _sectionGroupRepository;
        private readonly ISectionQuestionRepository _sectionQuestionRepository;
        private readonly SectionConverter _sectionConverter;

        public UpdateMockTestCommandHandler(IMapper mapper
            , IMockTestRepository mockTestRepository
            , IQuestionRepository questionRepository
            , ISectionGroupRepository sectionGroupRepository
            , ISectionQuestionRepository sectionQuestionRepository
            , SectionConverter sectionConverter)

        {
            _mapper = mapper;
            _mockTestRepository = mockTestRepository;
            _questionRepository = questionRepository;
            _sectionGroupRepository = sectionGroupRepository;
            _sectionQuestionRepository = sectionQuestionRepository;
            _sectionConverter = sectionConverter;
            _sectionConverter = sectionConverter;
        }

        public async Task<MethodResult<MockTestModel>> Handle(UpdateMockTestCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<MockTestModel> methodResult = new MethodResult<MockTestModel>();

            #region Validation

            if (request.SectionGroups == null || request.SectionGroups.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSectionGroupErrorCode.SectionGroupsNull), nameof(request.SectionGroups));
                return methodResult;
            }

            var mockTest = await _mockTestRepository.GetIncludeByIdAsync(request.Id);
            if (mockTest == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumMockTestErrorCode.MockTestsNotExist), nameof(request.Id), request.Id);
                return methodResult;
            }

            if (mockTest.CourseUnitMockTests.Any() || mockTest.UnitSkillMockTests.Any())
            {
                methodResult.AddErrorBadRequest(nameof(EnumMockTestErrorCode.MockTestInActiveState), nameof(mockTest.IsActive), mockTest.IsActive);
                return methodResult;
            }

            var sectionGroups = mockTest.MockTestSections.Select(x => x.SectionGroup ?? new SectionGroup()).ToList();
            var sections = sectionGroups.SelectMany(x => x.Sections).ToList();
            List<Question>? questions = null;
            List<SectionTimeCode>? sectionTimeCodes = null;
            List<SectionQuestion>? sectionQuestions = null;
            if (mockTest.MockTestType == EnumMockTestType.SkillMockTest)
            {
                var sectionGroup = sectionGroups.FirstOrDefault();
                if (sectionGroup != null)
                {
                    if (sectionGroup.CourseSkill == EnumCourseSkill.Reading || sectionGroup.CourseSkill == EnumCourseSkill.Listening)
                    {
                        var sectionParts = sections.SelectMany(x => x.SectionParts).ToList();
                        sectionQuestions = sectionParts.SelectMany(x => x.SectionQuestions).ToList();
                        questions = sectionQuestions.Select(x => x.Question ?? new Question()).ToList();
                    }
                    else if (sectionGroup.CourseSkill == EnumCourseSkill.Speaking)
                    {
                        sectionTimeCodes = sections.SelectMany(x => x.SectionTimeCodes).ToList();
                    }
                }
            }
            else
            {
                var sectionParts = sections.SelectMany(x => x.SectionParts).ToList();
                sectionQuestions = sectionParts.SelectMany(x => x.SectionQuestions).ToList();
                questions = sectionQuestions.Select(x => x.Question ?? new Question()).ToList();
                sectionTimeCodes = sections.SelectMany(x => x.SectionTimeCodes).ToList();
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

                    if (sectionGroup.CourseSkill != EnumCourseSkill.Speaking && sectionGroup.CourseSkill != EnumCourseSkill.Writing)
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
                                var correctCount = newSection.SectionParts.SelectMany(x => x.SectionQuestions).Select(x => x.Question).Sum(x => x!.CorrectTotal);
                                if (!SectionValidation.IsCheckSection(newSectionGroup.CourseSkill, section.DisplayOrder, correctCount))
                                {
                                    methodResult.AddErrorBadRequest(nameof(EnumMockTestErrorCode.MockTestMustCorrectScore), nameof(correctCount), correctCount);
                                    return methodResult;
                                }
                            }
                        }
                    }
                    else if (sectionGroup.CourseSkill == EnumCourseSkill.Speaking)
                    {
                        if (section.SectionTimeCodes == null || section.SectionTimeCodes!.Count == 0)
                        {
                            methodResult.AddErrorBadRequest(nameof(EnumSectionTimeCodeErrorCode.TimeCodeCanNotNull), nameof(sectionGroup.CourseSkill));
                            return methodResult;
                        }
                        foreach (var sectionTimeCode in section.SectionTimeCodes)
                        {
                            if (sectionTimeCode == null)
                            {
                                methodResult.AddErrorBadRequest(nameof(EnumSectionTimeCodeErrorCode.SectionTimeCodesNull), nameof(sectionTimeCode), sectionTimeCode);
                                return methodResult;
                            }
                            else
                            {
                                SectionTimeCode newSectionTimeCode = newSection.SectionTimeCodes.ElementAt(section.SectionTimeCodes.IndexOf(sectionTimeCode));
                            }
                        }
                    }

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

            #endregion Validation

            await _mockTestRepository.ExecuteTransactionAsync(async () =>
            {
                foreach (var item in sectionGroups)
                {
                    await _sectionGroupRepository.DeleteAsync(item);
                }
                await _sectionGroupRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                if (sectionQuestions != null)
                {
                    foreach (var item in sectionQuestions)
                    {
                        await _sectionQuestionRepository.DeleteAsync(item);
                    }
                    await _sectionQuestionRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                }
                if (questions != null)
                {
                    foreach (var item in questions)
                    {
                        await _questionRepository.DeleteAsync(item);
                    }
                    await _questionRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                }

                mockTest = _mockTestRepository.Update(mockTest);
                await _mockTestRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<MockTestModel>(mockTest);
                return methodResult;
            });

            return methodResult;
        }
    }
}
