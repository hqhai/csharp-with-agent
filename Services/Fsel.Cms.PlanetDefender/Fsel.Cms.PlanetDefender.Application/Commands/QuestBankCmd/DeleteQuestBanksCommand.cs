// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Application.Commands.QuestBankCmd
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Fsel.Cms.PlanetDefender.Domain.IRepositories;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class DeleteQuestBanksCommand : IRequest<MethodResult<bool>>
    {
        public IList<Guid>? Ids { get; set; }
    }

    public class DeleteQuestBanksCommandHandler : IRequestHandler<DeleteQuestBanksCommand, MethodResult<bool>>
    {
        private readonly IQuestBankRepository _questBankRepository;

        public DeleteQuestBanksCommandHandler(IQuestBankRepository questBankRepository)
        {
            _questBankRepository = questBankRepository;
        }

        public async Task<MethodResult<bool>> Handle(DeleteQuestBanksCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            if (request.Ids == null || request.Ids.Count == 0)
            {
                return methodResult;
            }

            var gameVocabularies = await _questBankRepository.Queryable.Where(p => request.Ids.Contains(p.Id)).ToListAsync(cancellationToken);
            if (gameVocabularies == null || gameVocabularies.Count != request.Ids.Count)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(gameVocabularies));
                return methodResult;
            }

            await _questBankRepository.ExecuteTransactionAsync(async () =>
            {
                await _questBankRepository.DeleteListAsync(gameVocabularies);
                await _questBankRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = true;
                return methodResult;
            });

            return methodResult;
        }
    }
}
