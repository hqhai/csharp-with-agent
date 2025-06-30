// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.TestConfigCmd
{
    using System.Text.RegularExpressions;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.TestConfig;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.TestConfigs;
    using Fsel.Course.Domain.Models.EntityModels.TestConfig;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class UpdateTestConfigCommand : UpdateTestConfigCommandModel, IRequest<MethodResult<TestConfigModel>>
    {
    }

    public class UpdateTestConfigCommandHandler : IRequestHandler<UpdateTestConfigCommand, MethodResult<TestConfigModel>>
    {
        private readonly IMapper _mapper;
        private readonly ITestConfigRepository _testConfigRepository;

        public UpdateTestConfigCommandHandler(IMapper mapper
            , ITestConfigRepository testConfigRepository)

        {
            _mapper = mapper;
            _testConfigRepository = testConfigRepository;
        }

        public async Task<MethodResult<TestConfigModel>> Handle(UpdateTestConfigCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<TestConfigModel> methodResult = new MethodResult<TestConfigModel>();

            var testConfig = await _testConfigRepository.GetIncludeByIdAsync(request.Id);
            if (testConfig == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(testConfig));
                return methodResult;
            }
            var testNameRegex = new Regex(@"^[a-zA-Z0-9_ ]{1,150}$");
            var testCodeRegex = new Regex(@"^[a-zA-Z0-9_]{1,150}$");
            if (string.IsNullOrEmpty(request.Name) || (!string.IsNullOrEmpty(request.Name) && !testNameRegex.IsMatch(request.Name)))
            {
                methodResult.AddErrorBadRequest(nameof(EnumCategoryErrorCode.NameNotValid), nameof(request.Name), request.Name);
                return methodResult;
            }

            if (string.IsNullOrEmpty(request.Code) || (!string.IsNullOrEmpty(request.Code) && !testCodeRegex.IsMatch(request.Code)))
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
            _mapper.Map(request, testConfig);

            if (!testConfig.IsValid())
            {
                methodResult.AddErrorBadRequest(testConfig.ErrorMessages);
                return methodResult;
            }

            return methodResult;
        }
    }
}
