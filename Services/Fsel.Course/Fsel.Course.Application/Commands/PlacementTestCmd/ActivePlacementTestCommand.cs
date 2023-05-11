// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.PlacementTestCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class ActivePlacementTestCommand : IRequest<MethodResult<bool>>
    {
        public Guid Id { get; set; }
        public bool IsActive { get; set; }
    }

    public class ActivePlacementTestCommandHandler : IRequestHandler<ActivePlacementTestCommand, MethodResult<bool>>
    {
        private readonly IPlacementTestRepository _placementTestRepository;

        public ActivePlacementTestCommandHandler(IPlacementTestRepository placementTestRepository
            )
        {
            _placementTestRepository = placementTestRepository;
        }

        public async Task<MethodResult<bool>> Handle(ActivePlacementTestCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            #region Validation

            var placementTest = await _placementTestRepository.GetIncludeByIdAsync(request.Id);
            if (placementTest == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumPlacementTestErrorCode.PlacementTestNotExist), nameof(request.Id), request.Id);
                return methodResult;
            }
            placementTest.IsActive = request.IsActive;

            #endregion Validation

            await _placementTestRepository.ExecuteTransactionAsync(async () =>
            {
                _placementTestRepository.Update(placementTest);
                await _placementTestRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = true;
                return methodResult;
            });

            return methodResult;
        }
    }
}
