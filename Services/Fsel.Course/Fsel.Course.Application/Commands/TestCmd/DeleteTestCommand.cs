// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.TestCmd
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class DeleteTestCommand : IRequest<MethodResult<bool>>
    {
        public Guid Id { get; set; }
    }

    public class DeleteTestCommandHandler : IRequestHandler<DeleteTestCommand, MethodResult<bool>>
    {
        private readonly ITestRepository _testRepository;
        private readonly ITestSectionRepository _testSectionRepository;
        private readonly ITestSectionQuestionRepository _testSectionQuestionRepository;
        private readonly IQuestionRepository _questionRepository;
        private readonly ITestAISettingRepository _testAISettingRepository;

        public DeleteTestCommandHandler(ITestRepository testRepository,
            ITestSectionRepository testSectionRepository,
            ITestSectionQuestionRepository testSectionQuestionRepository,
            IQuestionRepository questionRepository,
            ITestAISettingRepository testAISettingRepository)
        {
            _testRepository = testRepository;
            _testSectionRepository = testSectionRepository;
            _testSectionQuestionRepository = testSectionQuestionRepository;
            _questionRepository = questionRepository;
            _testAISettingRepository = testAISettingRepository;
        }

        public async Task<MethodResult<bool>> Handle(DeleteTestCommand request, CancellationToken cancellationToken)
        {
            MethodResult<bool> methodResult = new MethodResult<bool>();
            var test = await _testRepository.Queryable.FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken: cancellationToken);
            if (test == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(test));
                return methodResult;
            }

            var isActive = await _testRepository.IsUsingByClient(test.OriginalId);
            if (isActive)
            {
                methodResult.AddErrorBadRequest(nameof(EnumTestErrorCode.TestIsActive), nameof(isActive), isActive);
                return methodResult;
            }

            await _testRepository.ExecuteTransactionAsync(async () =>
            {
                await DeleteDataAsync(test);
                var result = await _testRepository.DeleteAsync(test);
                await _testRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = result;
                return methodResult;
            });
            return methodResult;
        }

        private async Task<VoidMethodResult> DeleteDataAsync(Test test)
        {
            VoidMethodResult methodResult = new VoidMethodResult();
            var allSectionChilrens = await _testSectionRepository.Queryable.Where(x => x.TestId == test.Id).ToListAsync();
            if (allSectionChilrens.Any())
            {
                await _testRepository.ExecuteTransactionAsync(async () =>
                {
                    await _testSectionRepository.DeleteListAsync(allSectionChilrens);
                    await _testSectionRepository.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);
                    return methodResult;
                });
            }
            var questions = await _testSectionQuestionRepository.Queryable.WhereBulkContains(allSectionChilrens.Select(x => x.Id), x => x.TestSectionId).Select(x => x.Question).ToListAsync();
            if (questions.Any())
            {
                await _testRepository.ExecuteTransactionAsync(async () =>
                {
                    await _questionRepository.DeleteListAsync(questions);
                    await _questionRepository.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);
                    return methodResult;
                });
            }
            var testAISettings = await _testAISettingRepository.Queryable.Include(x => x.TestAICriteriaSettings).WhereBulkContains(allSectionChilrens.Select(x => x.Id), x => x.TestSectionId).ToListAsync();
            if (testAISettings.Any())
            {
                await _testRepository.ExecuteTransactionAsync(async () =>
                {
                    await _testAISettingRepository.DeleteListAsync(testAISettings).ConfigureAwait(false);
                    await _testAISettingRepository.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);
                    return methodResult;
                });
            }
            return methodResult;
        }
    }
}
