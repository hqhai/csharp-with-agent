using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Common.Helpers;
using Fsel.Course.Domain.Enums.ErrorCodes;
using Fsel.Course.Domain.IRepositories;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Fsel.Course.Application.Commands.PlacementTestCmd
{
    public class DeletePlacementTestCommand : IRequest<MethodResult<bool>>
    {
        public Guid Id { get; set; }
    }

    public class DeletePlacementTestCommandHandler : IRequestHandler<DeletePlacementTestCommand, MethodResult<bool>>
    {
        private readonly IPlacementTestRepository _placementTestRepository;
        private readonly IMapper _mapper;

        public DeletePlacementTestCommandHandler(IPlacementTestRepository placementTestRepository,
            IMapper mapper)
        {
            _placementTestRepository = placementTestRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<bool>> Handle(DeletePlacementTestCommand request, CancellationToken cancellationToken)
        {
            MethodResult<bool> methodResult = new MethodResult<bool>();

            #region Validation

            var placementTest = await _placementTestRepository.GetByIdAsync(request.Id);
            if (placementTest == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumPlacementTestErrorCode.PlacementTestNotExist),
                                                nameof(request.Id), request.Id);
                return methodResult;
            }

            #endregion Validation

            await _placementTestRepository.ExecuteTransactionAsync(async () =>
            {
                var result = await _placementTestRepository.DeleteAsync(placementTest);
                await _placementTestRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = result;
                return methodResult;
            });

            return methodResult;
        }
    }
}
