// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.MockTestCmd
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Application.Commands.UnitCmd;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
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
            var mockTest = await _mockTestRepository.Queryable
                                                    .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken: cancellationToken);
            if (mockTest == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumMockTestErrorCode.MockTestsNotExist), nameof(request.Id), request?.Id);
                return methodResult;
            }
            var isUnitUsed = await _mockTestRepository.IsUnitSkillMockTest(request.Id);
            if (isUnitUsed)
            {
                methodResult.AddErrorBadRequest(nameof(EnumMockTestErrorCode.UnitUsed), nameof(request.Id), request.Id);
                return methodResult;
            }
            var isCourseUsed = await _mockTestRepository.IsCourseUnitMockTest(request.Id);
            if (isCourseUsed)
            {
                methodResult.AddErrorBadRequest(nameof(EnumMockTestErrorCode.CourseUsed), nameof(request.Id), request.Id);
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
