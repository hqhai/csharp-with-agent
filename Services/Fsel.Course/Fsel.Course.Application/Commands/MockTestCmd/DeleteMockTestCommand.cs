// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.MockTestCmd
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class DeleteMockTestCommand : IRequest<MethodResult<bool>>
    {
        public Guid Id { get; set; }
    }

    public class DeleteMockTestCommandHandler : IRequestHandler<DeleteMockTestCommand, MethodResult<bool>>
    {
        private readonly IMockTestRepository _mockTestRepository;
        private readonly IQuestionRepository _questionRepository;
        private readonly ISectionGroupRepository _sectionGroupRepository;
        private readonly ISectionQuestionRepository _sectionQuestionRepository;

        public DeleteMockTestCommandHandler(IMockTestRepository mockTestRepository
            , IQuestionRepository questionRepository
            , ISectionGroupRepository sectionGroupRepository
            , ISectionQuestionRepository sectionQuestionRepository)
        {
            _mockTestRepository = mockTestRepository;
            _questionRepository = questionRepository;
            _sectionGroupRepository = sectionGroupRepository;
            _sectionQuestionRepository = sectionQuestionRepository;
        }

        public async Task<MethodResult<bool>> Handle(DeleteMockTestCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            var mockTest = await _mockTestRepository.GetIncludeByIdAsync(request.Id);
            if (mockTest == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumMockTestErrorCode.MockTestsNotExist), nameof(request.Id), request?.Id);
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

                await _questionRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                var result = await _mockTestRepository.DeleteAsync(mockTest);
                await _mockTestRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = result;
                return methodResult;
            });
            return methodResult;
        }
    }
}
