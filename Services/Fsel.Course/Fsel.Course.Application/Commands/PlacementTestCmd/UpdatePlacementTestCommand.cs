using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Common.Helpers;
using Fsel.Course.Domain.Enums.ErrorCodes;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.CommandModels.PlacementTests;
using Fsel.Course.Domain.Models.EntityModels;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Fsel.Course.Application.Commands.PlacementTestCmd
{
    public class UpdatePlacementTestCommand : UpdatePlacementTestCommandModel, IRequest<MethodResult<PlacementTestModel>>
    {
    }

    public class UpdatePlacementTestCommandHandler : IRequestHandler<UpdatePlacementTestCommand, MethodResult<PlacementTestModel>>
    {
        private readonly IPlacementTestRepository _placementTestRepository;
        private readonly IMapper _mapper;

        public UpdatePlacementTestCommandHandler(IPlacementTestRepository placementTestRepository,
            IMapper mapper)
        {
            _placementTestRepository = placementTestRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<PlacementTestModel>> Handle(UpdatePlacementTestCommand request, CancellationToken cancellationToken)
        {
            MethodResult<PlacementTestModel> methodResult = new MethodResult<PlacementTestModel>();

            #region Validation

            var placementTest = await _placementTestRepository.GetByIdAsync(request.Id);
            if (placementTest == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumPlacementTestErrorCode.PlacementTestNotExist),
                                                nameof(request.Id), request.Id);
                return methodResult;
            }
            _mapper.Map(request, placementTest);

            if (!placementTest.IsValid())
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddResultFromErrorList(placementTest.ErrorMessages);
                return methodResult;
            }

            #endregion Validation

            await _placementTestRepository.ExecuteTransactionAsync(async () =>
            {
                placementTest = _placementTestRepository.Update(placementTest);
                await _placementTestRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<PlacementTestModel>(placementTest);
                return methodResult;
            });

            return methodResult;
        }
    }
}
