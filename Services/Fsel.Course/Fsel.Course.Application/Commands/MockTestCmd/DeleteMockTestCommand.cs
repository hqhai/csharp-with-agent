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
    using Fsel.Course.Infrastructure.Repositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class DeleteMockTestCommand : IRequest<MethodResult<bool>>
    {
        public Guid Id { get; set; }
    }

    public class DeleteMockTestCommandHandler : IRequestHandler<DeleteMockTestCommand, MethodResult<bool>>
    {
        private readonly IMockTestRepository _mockTestRepository;
        private readonly ISectionRepository _sectionRepository;
        private readonly ISectionPartRepository _sectionPartRepository;
        private readonly IQuestionRepository _questionRepository;
        private readonly ISectionGroupRepository _sectionGroupRepository;
        private readonly ISectionQuestionRepository _sectionQuestionRepository;

        public DeleteMockTestCommandHandler(IMockTestRepository mockTestRepository
            , ISectionRepository sectionRepository
            , ISectionPartRepository sectionPartRepository
            , IQuestionRepository questionRepository
            , ISectionGroupRepository sectionGroupRepository
            , ISectionQuestionRepository sectionQuestionRepository)
        {
            _mockTestRepository = mockTestRepository;
            _sectionRepository = sectionRepository;
            _sectionPartRepository = sectionPartRepository;
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
            if (!mockTest.IsActive)
            {
                methodResult.AddErrorBadRequest(nameof(EnumMockTestErrorCode.MockTestInActiveState), nameof(mockTest.IsActive), mockTest.IsActive);
                return methodResult;
            }

            List<SectionGroup> sectionGroups = mockTest.MockTestSections.Select(x => x.SectionGroup!).ToList();
            List<Section> sections = sectionGroups.SelectMany(x => x.Sections).ToList();
            List<SectionPart> sectionParts = sections.SelectMany(x => x.SectionParts).ToList();

            await _mockTestRepository.ExecuteTransactionAsync(async () =>
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
