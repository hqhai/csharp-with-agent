// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.RubyScopeCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class DeleteRubyScopeCommand : IRequest<MethodResult<bool>>
    {
        public Guid Id { get; set; }
    }

    public class DeleteRubyScopeCommandHandler : IRequestHandler<DeleteRubyScopeCommand, MethodResult<bool>>
    {
        private readonly IRubyScopeRepository _rubyScopeRepository;

        public DeleteRubyScopeCommandHandler(IRubyScopeRepository rubyScopeRepository)
        {
            _rubyScopeRepository = rubyScopeRepository;
        }

        public async Task<MethodResult<bool>> Handle(DeleteRubyScopeCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var rubyText = await _rubyScopeRepository.GetByIdAsync(request.Id);

            if (rubyText == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(rubyText));
                return methodResult;
            }

            await _rubyScopeRepository.ExecuteTransactionAsync(async () =>
            {
                var result = await _rubyScopeRepository.DeleteAsync(rubyText);
                await _rubyScopeRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = result;
                return methodResult;
            });

            return methodResult;
        }
    }
}
