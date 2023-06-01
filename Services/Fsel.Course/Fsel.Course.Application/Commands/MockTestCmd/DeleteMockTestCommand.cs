// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.MockTestCmd
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Infrastructure.Common;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class DeleteMockTestCommand : IRequest<MethodResult<bool>>
    {
        public Guid Id { get; set; }
    }

    public class DeleteMockTestCommandHandler : IRequestHandler<DeleteMockTestCommand, MethodResult<bool>>
    {
        private readonly IMockTestRepository _mockTestRepository;
        private readonly SectionConverter _sectionConverter;

        public DeleteMockTestCommandHandler(IMockTestRepository mockTestRepository
            , SectionConverter sectionConverter)
        {
            _mockTestRepository = mockTestRepository;
            _sectionConverter = sectionConverter;
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
            var sectionQuestions = sectionGroups.SelectMany(x => x.Sections).SelectMany(x => x.SectionParts).SelectMany(x => x.SectionQuestions).ToList();
            var questions = sectionQuestions.Select(x => x.Question ?? new Question()).ToList();

            await _mockTestRepository.ExecuteTransactionAsync(async () =>
            {
                await _sectionConverter.DeleteSectionGroup(sectionGroups, sectionQuestions, questions);

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
