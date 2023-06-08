// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.ExtraPracticeCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Infrastructure.Common;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class DeleteExtraPracticeCommand : IRequest<MethodResult<bool>>
    {
        public Guid Id { get; set; }
    }

    public class DeleteExtraPracticeCommandHandler : IRequestHandler<DeleteExtraPracticeCommand, MethodResult<bool>>
    {
        private readonly IExtraPracticeRepository _extraPracticeRepository;
        private readonly ExtraPracticeConverter _extraPracticeConverter;

        public DeleteExtraPracticeCommandHandler(IExtraPracticeRepository extraPracticeRepository, ExtraPracticeConverter extraPracticeConverter)
        {
            _extraPracticeRepository = extraPracticeRepository;
            _extraPracticeConverter = extraPracticeConverter;
        }

        public async Task<MethodResult<bool>> Handle(DeleteExtraPracticeCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();
            var extraPractice = await _extraPracticeRepository.GetIncludeByIdAsync(request.Id);
            if (extraPractice == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumExtraPracticeErrorCode.ExtraPracticeNotExist), nameof(request.Id), request.Id);
                return methodResult;
            }
            if (extraPractice.IsActive)
            {
                methodResult.AddErrorBadRequest(nameof(EnumExtraPracticeErrorCode.ExtraPracticeInActiveState), nameof(extraPractice.IsActive), extraPractice.IsActive);
                return methodResult;
            }

            await _extraPracticeRepository.ExecuteTransactionAsync(async () =>
            {
                var method = await _extraPracticeConverter.DeleteExtraPractice(extraPractice, cancellationToken);
                if (!method.IsOK)
                {
                    methodResult.AddErrorBadRequest(method.ErrorMessages);
                    return methodResult;
                }
                var result = await _extraPracticeRepository.DeleteAsync(extraPractice);
                await _extraPracticeRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = result;
                return methodResult;
            });
            return methodResult;
        }
    }
}
