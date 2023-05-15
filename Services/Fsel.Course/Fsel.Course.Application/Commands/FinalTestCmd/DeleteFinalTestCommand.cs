// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.FinalTestCmd
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class DeleteFinalTestCommand : IRequest<MethodResult<bool>>
    {
        public Guid Id { get; set; }
    }

    public class DeleteFinalTestCommandHandler : IRequestHandler<DeleteFinalTestCommand, MethodResult<bool>>
    {
        private readonly IFinalTestRepository _finalTestRepository;

        public DeleteFinalTestCommandHandler(IFinalTestRepository finalTestRepository)
        {
            _finalTestRepository = finalTestRepository;
        }

        public async Task<MethodResult<bool>> Handle(DeleteFinalTestCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();
            var finalTest = await _finalTestRepository.GetIncludeByIdAsync(request.Id);
            if (finalTest == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumFinalTestErrorCode.FinalTestsNotExist), nameof(request.Id), request?.Id);
                return methodResult;
            }
            if (finalTest.IsActive)
            {
                methodResult.AddErrorBadRequest(nameof(EnumFinalTestErrorCode.FinalTestInActiveState), nameof(finalTest.IsActive), finalTest.IsActive);
                return methodResult;
            }
            await _finalTestRepository.ExecuteTransactionAsync(async () =>
            {
                var result = await _finalTestRepository.DeleteAsync(finalTest);
                await _finalTestRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = result;
                return methodResult;
            });
            return methodResult;
        }
    }
}
