// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.TestCmd
{
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.Tests;
    using Fsel.Course.Domain.Models.EntityModels.TestModels;
    using Fsel.Course.Infrastructure.Common;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class UpdateTestCommand : UpdateTestCommandModel, IRequest<MethodResult<TestModel>>
    {
    }

    public class UpdateTestConfigCommandHandler : IRequestHandler<UpdateTestCommand, MethodResult<TestModel>>
    {
        private readonly IMapper _mapper;
        private readonly ITestRepository _testRepository;
        private readonly TestHelper _testHelper;

        public UpdateTestConfigCommandHandler(IMapper mapper
            , ITestRepository testRepository
            , TestHelper testHelper)
        {
            _mapper = mapper;
            _testRepository = testRepository;
            _testHelper = testHelper;
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

            var codeExists = await _testRepository.Queryable.AnyAsync(x => x.Code == request.Code && x.Id != request.Id, cancellationToken);
            if (codeExists)
            {
<<<<<<< Updated upstream
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(request.Code), request.Code);
=======
                methodResult.AddErrorBadRequest(newVersionTest.ErrorMessages);
                return methodResult;
            }
            var method = _testConverter.IsValidateQuestion(request.TestSections);
            if (!method.IsOK)
            {
                methodResult.AddErrorBadRequest(method.ErrorMessages);
                return methodResult;
            }
            if (!await newVersionTest.IsValid(_serviceProvider))
            {
                methodResult.AddErrorBadRequest(newVersionTest.ErrorMessages);
>>>>>>> Stashed changes
                return methodResult;
            }

            _mapper.Map(request, test);
            if (!test.IsValid())
            {
                methodResult.AddErrorBadRequest(test.ErrorMessages);
                return methodResult;
            }
            await _testHelper.UpdateSectionRecursive(request.TestSections, test: test);
            // Bắt đầu transaction
            await _testRepository.ExecuteTransactionAsync(async () =>
            {
                // Cập nhật Test
                await _testHelper.DeleteDataAsync(test);

                _testRepository.Update(test);
                await _testRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<TestModel>(test);
                return methodResult;
            });
            return methodResult;
        }
    }
}
