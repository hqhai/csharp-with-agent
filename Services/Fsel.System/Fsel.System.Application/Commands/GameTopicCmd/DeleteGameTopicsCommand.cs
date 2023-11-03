// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.GameTopicCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.System.Domain.IRepositories;
    using global::System;
    using global::System.Collections.Generic;
    using global::System.Linq;
    using global::System.Threading.Tasks;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class DeleteGameTopicsCommand : IRequest<MethodResult<bool>>
    {
        public IList<Guid>? Ids { get; set; }
    }

    public class DeleteGameTopicsCommandHandler : IRequestHandler<DeleteGameTopicsCommand, MethodResult<bool>>
    {
        private readonly IGameTopicRepository _gameTopicRepository;

        public DeleteGameTopicsCommandHandler(IGameTopicRepository gameTopicRepository)
        {
            _gameTopicRepository = gameTopicRepository;
        }

        public async Task<MethodResult<bool>> Handle(DeleteGameTopicsCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            if (request.Ids == null || request.Ids.Count == 0)
            {
                return methodResult;
            }

            var gameTopics = await _gameTopicRepository.Queryable.Where(p => request.Ids.Contains(p.Id)).ToListAsync(cancellationToken);

            await _gameTopicRepository.ExecuteTransactionAsync(async () =>
            {
                await _gameTopicRepository.DeleteListAsync(gameTopics);
                await _gameTopicRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = true;
                return methodResult;
            });
            return methodResult;
        }
    }
}
