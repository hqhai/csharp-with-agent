// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Commands.SupportCategoryCmd
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Interaction.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class DeleteSupportCategoryCommand : IRequest<MethodResult<bool>>
    {
        public Guid Id { get; set; }
    }

    public class DeleteSupportCategoryCommandHandler : IRequestHandler<DeleteSupportCategoryCommand, MethodResult<bool>>
    {
        private readonly ISupportCategoryRepository _supportCategoryRepository;

        public DeleteSupportCategoryCommandHandler(ISupportCategoryRepository supportCategoryRepository)
        {
            _supportCategoryRepository = supportCategoryRepository;
        }

        public async Task<MethodResult<bool>> Handle(DeleteSupportCategoryCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            #region Validation

            var supportCategory = await _supportCategoryRepository.GetByIdAsync(request.Id);
            if (supportCategory == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(supportCategory));
                return methodResult;
            }

            #endregion Validation

            await _supportCategoryRepository.ExecuteTransactionAsync(async () =>
            {
                var result = await _supportCategoryRepository.DeleteAsync(supportCategory);
                await _supportCategoryRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = result;
                return methodResult;
            });

            return methodResult;
        }
    }
}
