// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.GameVocabularyCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.System.Domain.Enums.ErrorCodes;
    using Fsel.System.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class DeleteGameVocabulariesCommand : IRequest<MethodResult<bool>>
    {
        public IList<Guid>? Ids { get; set; }
    }

    public class DeleteGameVocabulariesCommandHandler : IRequestHandler<DeleteGameVocabulariesCommand, MethodResult<bool>>
    {
        private readonly IGameVocabularyRepository _gameVocabularyRepository;

        public DeleteGameVocabulariesCommandHandler(IGameVocabularyRepository gameVocabularyRepository)
        {
            _gameVocabularyRepository = gameVocabularyRepository;
        }

        public async Task<MethodResult<bool>> Handle(DeleteGameVocabulariesCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            if (request.Ids == null || request.Ids.Count == 0)
            {
                return methodResult;
            }

            var gameVocabularies = await _gameVocabularyRepository.Queryable.Where(p => request.Ids.Contains(p.Id)).Include(x => x.GameVocabularyTypes).Include(n => n.GameVocabularyPlatforms).ToListAsync(cancellationToken);
            if (gameVocabularies == null || gameVocabularies.Count != request.Ids.Count)
            {
                methodResult.AddErrorBadRequest(nameof(EnumGameVocabularyErrorCode.GameVocabularyNotExist));
                return methodResult;
            }

            await _gameVocabularyRepository.ExecuteTransactionAsync(async () =>
            {
                await _gameVocabularyRepository.DeleteListAsync(gameVocabularies);
                await _gameVocabularyRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = true;
                return methodResult;
            });

            return methodResult;
        }
    }
}
