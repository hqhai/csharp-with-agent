// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.TestConfigCmd
{
    using System.Text.RegularExpressions;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.TestConfig;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.TestConfig;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.EntityModels.TestConfig;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Infrastructure.Repositories;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class CreateTestConfigCommand : CreateTestConfigCommandModel, IRequest<MethodResult<TestConfigModel>>
    {
    }

    public class CreateTestConfigCommandHandler : IRequestHandler<CreateTestConfigCommand, MethodResult<TestConfigModel>>
    {
        private readonly IMapper _mapper;
        private readonly ITestConfigRepository _testConfigRepository;

        public CreateTestConfigCommandHandler(IMapper mapper
            , ITestConfigRepository testConfigRepository
            )
        {
            _mapper = mapper;
            _testConfigRepository = testConfigRepository;
        }

        public async Task<MethodResult<TestConfigModel>> Handle(CreateTestConfigCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<TestConfigModel> methodResult = new MethodResult<TestConfigModel>();
            var testNameRegex = new Regex(@"^[a-zA-Z0-9_ ]{1,150}$");
            var testCodeRegex = new Regex(@"^[a-zA-Z0-9_]{1,150}$");
            if (string.IsNullOrEmpty(request.Name) || (!string.IsNullOrEmpty(request.Name) && testNameRegex.IsMatch(request.Name)))
            {
                methodResult.AddErrorBadRequest(nameof(EnumCategoryErrorCode.NameNotValid), nameof(request.Name), request.Name);
                return methodResult;
            }

            if (string.IsNullOrEmpty(request.Code) || (!string.IsNullOrEmpty(request.Code) && testCodeRegex.IsMatch(request.Code)))
            {
                methodResult.AddErrorBadRequest(nameof(EnumCategoryErrorCode.CodeNotValid), nameof(request.Code), request.Code);
                return methodResult;
            }
            var checkCode = await _testConfigRepository.Queryable.AnyAsync(x => x.Code == request.Code.Trim(), cancellationToken);
            if (checkCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(request.Code), request.Code);
                return methodResult;
            }
            var testConfig = _mapper.Map<TestConfig>(request);
            if (!testConfig.IsValid())
            {
                methodResult.AddErrorBadRequest(testConfig.ErrorMessages);
                return methodResult;
            }




            return methodResult;
        }
    }
}
