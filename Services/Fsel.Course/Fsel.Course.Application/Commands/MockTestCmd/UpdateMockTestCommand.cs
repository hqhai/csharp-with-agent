// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.MockTestCmd
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Application.Commands.UnitCmd;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.ClassForums;
    using Fsel.Course.Domain.Models.CommandModels.MockTests;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Repositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class UpdateMockTestCommand : UpdateMockTestCommandModel, IRequest<MethodResult<MockTestModel>>
    {
    }

    public class UpdateMockTestCommandHandler : IRequestHandler<UpdateMockTestCommand, MethodResult<MockTestModel>>
    {
        private readonly IMapper _mapper;
        private readonly IMockTestRepository _mockTestRepository;
        private readonly IUnitRepository _unitRepository;

        public UpdateMockTestCommandHandler(IMapper mapper
            , IMockTestRepository mockTestRepository
            , IUnitRepository unitRepository)
        {
            _mapper = mapper;
            _mockTestRepository = mockTestRepository;
            _unitRepository = unitRepository;
        }

        public async Task<MethodResult<MockTestModel>> Handle(UpdateMockTestCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<MockTestModel> methodResult = new MethodResult<MockTestModel>();

            var mockTest = await _mockTestRepository.Queryable
                                                    .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken: cancellationToken);
            if (mockTest == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumMockTestErrorCode.MockTestsNotExist), nameof(request.Id), request?.Id);
                return methodResult;
            }
            _mapper.Map(request, mockTest);

            if (!mockTest.IsValid())
            {
                methodResult.AddErrorBadRequest(mockTest.ErrorMessages);
                return methodResult;
            }
            var isUnitUsed = await _unitRepository.IsUnitUsed(request.Id);
            if (isUnitUsed)
            {
                methodResult.AddErrorBadRequest(nameof(EnumUnitErrorCode.UnitUsed), nameof(request.Id), request.Id);
                return methodResult;
            }

            await _mockTestRepository.ExecuteTransactionAsync(async () =>
            {
                mockTest = _mockTestRepository.Update(mockTest);
                await _mockTestRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<MockTestModel>(mockTest);
                return methodResult;
            });

            return methodResult;
        }
    }
}
