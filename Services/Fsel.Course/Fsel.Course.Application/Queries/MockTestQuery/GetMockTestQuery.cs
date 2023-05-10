// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.MockTestQuery
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class GetMockTestQuery : IRequest<MethodResult<MockTestModel>>
    {
        public Guid Id { get; set; }
    }

    public class GetMockTestQueryHandler : IRequestHandler<GetMockTestQuery, MethodResult<MockTestModel>>
    {
        private readonly IMapper _mapper;
        private readonly IMockTestRepository _mockTestRepository;

        public GetMockTestQueryHandler(IMapper mapper, IMockTestRepository mockTestRepository)
        {
            _mapper = mapper;
            _mockTestRepository = mockTestRepository;
        }

        public async Task<MethodResult<MockTestModel>> Handle(GetMockTestQuery request, CancellationToken cancellationToken)
        {
            MethodResult<MockTestModel> methodResult = new MethodResult<MockTestModel>();
            ArgumentNullException.ThrowIfNull(request);
            var mockTest = await _mockTestRepository.GetIncludeByIdAsync(request.Id);

            if (mockTest == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumMockTestErrorCode.MockTestsNotExist), nameof(request.Id), request.Id);
                return methodResult;
            }
            methodResult.Result = _mapper.Map<MockTestModel>(mockTest);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
