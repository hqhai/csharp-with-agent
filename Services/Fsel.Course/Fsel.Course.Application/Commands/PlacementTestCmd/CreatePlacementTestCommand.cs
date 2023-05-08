// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Enums.ErrorCodes;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.CommandModels.PlacementTests;
using Fsel.Course.Domain.Models.EntityModels;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Fsel.Course.Application.Commands.PlacementTestCmd
{
    public class CreatePlacementTestCommand : CreatePlacementTestCommandModel, IRequest<MethodResult<PlacementTestModel>>
    {
    }

    public class CreatePlacementTestCommandHandler : IRequestHandler<CreatePlacementTestCommand, MethodResult<PlacementTestModel>>
    {
        private readonly IPlacementTestRepository _placementTestRepository;
        private readonly IMapper _mapper;

        public CreatePlacementTestCommandHandler(IPlacementTestRepository placementTestRepository,
            IMapper mapper)
        {
            _placementTestRepository = placementTestRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<PlacementTestModel>> Handle(CreatePlacementTestCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<PlacementTestModel> methodResult = new MethodResult<PlacementTestModel>();

            #region Validation

            if (request.PlacementTestSections == null || request.PlacementTestSections.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumPlacementTestErrorCode.PlacementTestNotExist), nameof(request.PlacementTestSections), request.PlacementTestSections);
                return methodResult;
            }
            PlacementTest placementTest = _mapper.Map<PlacementTest>(request);

            if (!placementTest.IsValid())
            {
                methodResult.AddErrorBadRequest(placementTest.ErrorMessages);
                return methodResult;
            }

            #endregion Validation

            await _placementTestRepository.ExecuteTransactionAsync(async () =>
            {
                placementTest = _placementTestRepository.Add(placementTest);
                await _placementTestRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<PlacementTestModel>(placementTest);
                return methodResult;
            });

            return methodResult;
        }
    }
}
