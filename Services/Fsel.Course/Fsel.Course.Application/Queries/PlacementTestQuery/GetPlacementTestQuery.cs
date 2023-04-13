// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Course.Domain.Enums.ErrorCodes;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.EntityModels;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Fsel.Course.Application.Queries.PlacementTestQuery
{
    public class GetPlacementTestQuery : IRequest<MethodResult<PlacementTestModel>>
    {
        public Guid Id { get; set; }
    }

    public class GetPlacementTestQueryHandler : IRequestHandler<GetPlacementTestQuery, MethodResult<PlacementTestModel>>
    {
        private readonly IPlacementTestRepository _placementTestRepository;
        private readonly IMapper _mapper;

        public GetPlacementTestQueryHandler(IMapper mapper, IPlacementTestRepository placementTestRepository)
        {
            _placementTestRepository = placementTestRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<PlacementTestModel>> Handle(GetPlacementTestQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<PlacementTestModel> methodResult = new MethodResult<PlacementTestModel>();

            var placementTest = await _placementTestRepository.GetByIdAsync(request.Id);

            if (placementTest == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumPlacementTestErrorCode.PlacementTestNotExist), nameof(request.Id), request.Id);
                return methodResult;
            }

            methodResult.Result = _mapper.Map<PlacementTestModel>(placementTest);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
