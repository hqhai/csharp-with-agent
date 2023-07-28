// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.PlacementTestCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class UpdateStatusPlacementTestCommand : IRequest<MethodResult<bool>>
    {
        public Guid Id { get; set; }
        public bool IsActive { get; set; }
    }

    public class UpdateStatusPlacementTestCommandHandler : IRequestHandler<UpdateStatusPlacementTestCommand, MethodResult<bool>>
    {
        private readonly IPlacementTestRepository _placementTestRepository;

        public UpdateStatusPlacementTestCommandHandler(IPlacementTestRepository placementTestRepository
            )
        {
            _placementTestRepository = placementTestRepository;
        }

        public async Task<MethodResult<bool>> Handle(UpdateStatusPlacementTestCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            #region Validation

            var placementTest = await _placementTestRepository.GetByIdAsync(request.Id);
            if (placementTest == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Id));
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
