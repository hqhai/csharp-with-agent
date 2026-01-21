// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.TestCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class DeleteTestGroupResultCommand : IRequest<MethodResult<bool>>
    {
        public Guid TestGroupResultId { get; set; }
    }

    public class DeleteTestGroupResultCommandHandler : IRequestHandler<DeleteTestGroupResultCommand, MethodResult<bool>>
    {
        private readonly ITestGroupResultRepository _testGroupResultRepository;

        public DeleteTestGroupResultCommandHandler(ITestGroupResultRepository testGroupResultRepository)
        {
            _testGroupResultRepository = testGroupResultRepository;
        }

        public async Task<MethodResult<bool>> Handle(DeleteTestGroupResultCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var testGroupResult = await _testGroupResultRepository.Queryable.FirstOrDefaultAsync(p => p.Id == request.TestGroupResultId, cancellationToken);
            if (testGroupResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(testGroupResult), request.TestGroupResultId);
                return methodResult;
            }
            await _testGroupResultRepository.ExecuteTransactionAsync(async () =>
            {
                await _testGroupResultRepository.DeleteAsync(testGroupResult);
                await _testGroupResultRepository.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = true;
                return methodResult;
            });
            return methodResult;
        }
    }
}
