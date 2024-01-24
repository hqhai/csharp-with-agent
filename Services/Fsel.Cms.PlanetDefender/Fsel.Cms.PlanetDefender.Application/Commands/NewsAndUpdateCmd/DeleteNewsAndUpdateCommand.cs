// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Application.Commands.NewsAndUpdateCmd
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Cms.PlanetDefender.Domain.IRepositories;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class DeleteNewsAndUpdateCommand : IRequest<MethodResult<bool>>
    {
        public Guid Id { get; set; }
    }

    public class DeleteNewsAndUpdateCommandHandler : IRequestHandler<DeleteNewsAndUpdateCommand, MethodResult<bool>>
    {
        private readonly INewsAndUpdateRepository _newsAndUpdateRepository;

        public DeleteNewsAndUpdateCommandHandler(INewsAndUpdateRepository newsAndUpdateRepository)
        {
            _newsAndUpdateRepository = newsAndUpdateRepository;
        }

        public async Task<MethodResult<bool>> Handle(DeleteNewsAndUpdateCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            #region Validation

            var newsAndUpdate = await _newsAndUpdateRepository.GetByIdAsync(request.Id);
            if (newsAndUpdate == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(newsAndUpdate));
                return methodResult;
            }

            #endregion Validation

            await _newsAndUpdateRepository.ExecuteTransactionAsync(async () =>
            {
                var result = await _newsAndUpdateRepository.DeleteAsync(newsAndUpdate);
                await _newsAndUpdateRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = result;
                return methodResult;
            });

            return methodResult;
        }
    }
}
