// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Commands.SupportQuetionCmd
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Interaction.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class DeleteSupportQuestionCommand : IRequest<MethodResult<bool>>
    {
        public Guid Id { get; set; }
    }

    public class DeleteSupportQuestionCommandHandler : IRequestHandler<DeleteSupportQuestionCommand, MethodResult<bool>>
    {
        private readonly ISupportQuestionRepository _supportQuetionRepository;

        public DeleteSupportQuestionCommandHandler(ISupportQuestionRepository supportQuetionRepository)
        {
            _supportQuetionRepository = supportQuetionRepository;
        }

        public async Task<MethodResult<bool>> Handle(DeleteSupportQuestionCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            #region Validation

            var supportQuestion = await _supportQuetionRepository.GetByIdAsync(request.Id);
            if (supportQuestion == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Id));
                return methodResult;
            }

            #endregion Validation

            await _supportQuetionRepository.ExecuteTransactionAsync(async () =>
            {
                var result = await _supportQuetionRepository.DeleteAsync(supportQuestion);
                await _supportQuetionRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = result;
                return methodResult;
            });

            return methodResult;
        }
    }
}
