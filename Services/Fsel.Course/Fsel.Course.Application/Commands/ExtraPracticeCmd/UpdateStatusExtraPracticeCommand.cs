// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.ExtraPracticeCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class UpdateStatusExtraPracticeCommand : IRequest<MethodResult<bool>>
    {
        public Guid Id { get; set; }
        public bool IsActive { get; set; }
    }

    public class UpdateStatusExtraPracticeCommandHandler : IRequestHandler<UpdateStatusExtraPracticeCommand, MethodResult<bool>>
    {
        private readonly IExtraPracticeRepository _extraPracticeRepository;

        public UpdateStatusExtraPracticeCommandHandler(IExtraPracticeRepository extraPracticeRepository)
        {
            _extraPracticeRepository = extraPracticeRepository;
        }

        public async Task<MethodResult<bool>> Handle(UpdateStatusExtraPracticeCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            #region Validation

            var extraPractice = await _extraPracticeRepository.GetByIdAsync(request.Id);
            if (extraPractice == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(extraPractice));
                return methodResult;
            }
            extraPractice.IsActive = request.IsActive;

            #endregion Validation

            await _extraPracticeRepository.ExecuteTransactionAsync(async () =>
            {
                _extraPracticeRepository.Update(extraPractice);
                await _extraPracticeRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = true;
                return methodResult;
            });

            return methodResult;
        }
    }
}
