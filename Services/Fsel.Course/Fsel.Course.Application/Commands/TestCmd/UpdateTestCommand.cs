// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.TestCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.Tests;
    using Fsel.Course.Domain.Models.EntityModels.TestModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Infrastructure.Common.TestHelper;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class UpdateTestCommand : UpdateTestCommandModel, IRequest<MethodResult<TestModel>>
    {
    }

    public class UpdateTestConfigCommandHandler : IRequestHandler<UpdateTestCommand, MethodResult<TestModel>>
    {
        private readonly IMapper _mapper;
        private readonly ITestRepository _testRepository;
        private readonly TestConverter _testHelper;
        private readonly QuestionConverter _questionConverter;
        private readonly IServiceProvider _serviceProvider;
        private readonly TestConverter _testConverter;
        private readonly IVersionEntityUpdater<Test> _versionEntityUpdater;

        public UpdateTestConfigCommandHandler(IMapper mapper
            , ITestRepository testRepository
            , TestConverter testHelper
            , QuestionConverter questionConverter
            , IServiceProvider serviceProvider
            , TestConverter testConverter
            , IVersionEntityUpdater<Test> versionEntityUpdater)
        {
            _mapper = mapper;
            _testRepository = testRepository;
            _testHelper = testHelper;
            _questionConverter = questionConverter;
            _serviceProvider = serviceProvider;
            _testConverter = testConverter;
            _versionEntityUpdater = versionEntityUpdater;
        }

        public async Task<MethodResult<TestModel>> Handle(UpdateTestCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<TestModel>();

            var test = await _testRepository.GetByIdAsync(request.Id);
            if (test == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Id), request.Id);
                return methodResult;
            }
            var isUsingByClient = await _testRepository.IsUsingByClient(test.OriginalId);

            var newVersionTest = TestFactory.Create(request, _mapper, _questionConverter).Build(test.OriginalId, true);
            if (await newVersionTest.ValidateDuplicateTest(_testRepository).ConfigureAwait(false))
            {
                methodResult.AddErrorBadRequest(newVersionTest.ErrorMessages);
                return methodResult;
            }
            var method = _testConverter.IsValidateQuestion(newVersionTest.TestSections);
            if (!method.IsOK)
            {
                methodResult.AddErrorBadRequest(method.ErrorMessages);
                return methodResult;
            }
            if (!await newVersionTest.IsValid(_serviceProvider))
            {
                methodResult.AddErrorBadRequest(newVersionTest.ErrorMessages);
                return methodResult;
            }

            if (isUsingByClient)
            {
                await _versionEntityUpdater.UpdateEntity(test, newVersionTest,
                        async (_, entity) => isUsingByClient,
                        async (oldEntity, newEntity) =>
                        {
                            await Task.Yield();
                        }
                    );
            }
            else
            {
                var methodHelper = await _testHelper.UpdateSectionRecursive(request.TestSections, test: test);
                if (!methodHelper.IsOK)
                {
                    methodResult.AddErrorBadRequest(methodHelper.ErrorMessages);
                    return methodResult;
                }
                await _testRepository.ExecuteTransactionAsync(async () =>
                {
                    // Cập nhật Test
                    await _testHelper.DeleteDataAsync(test);

                    _testRepository.Update(test);
                    await _testRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                    methodResult.StatusCode = StatusCodes.Status200OK;
                    methodResult.Result = _mapper.Map<TestModel>(test);
                    return methodResult;
                });
            }

            return methodResult;
        }
    }
}
