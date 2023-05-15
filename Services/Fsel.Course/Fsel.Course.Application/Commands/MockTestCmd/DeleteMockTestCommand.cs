// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.MockTestCmd
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class DeleteMockTestCommand : IRequest<MethodResult<bool>>
    {
        public Guid Id { get; set; }
    }

    public class DeleteMockTestCommandHandler : IRequestHandler<DeleteMockTestCommand, MethodResult<bool>>
    {
        private readonly IMockTestRepository _mockTestRepository;
        private readonly IUnitRepository _unitRepository;
        private readonly ICourseRepository _courseRepository;

        public DeleteMockTestCommandHandler(IMockTestRepository mockTestRepository
            , IUnitRepository unitRepository,
            ICourseRepository courseRepository)
        {
            _mockTestRepository = mockTestRepository;
            _unitRepository = unitRepository;
            _courseRepository = courseRepository;
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
            if (mockTest.IsActive)
            {
                methodResult.AddErrorBadRequest(nameof(EnumMockTestErrorCode.MockTestInActiveState), nameof(mockTest.IsActive), mockTest.IsActive);
                return methodResult;
            }

            await _mockTestRepository.ExecuteTransactionAsync(async () =>
            {
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
