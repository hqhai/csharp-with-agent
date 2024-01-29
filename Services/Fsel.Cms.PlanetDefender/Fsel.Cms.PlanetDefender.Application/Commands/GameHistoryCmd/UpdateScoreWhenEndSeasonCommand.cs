// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Application.Commands.GameHistoryCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Cms.PlanetDefender.Domain.IRepositories;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class UpdateScoreWhenEndSeasonCommand : IRequest<MethodResult<bool>>
    {
    }

    public class UpdateScoreWhenEndSeasonCommandHandler : IRequestHandler<UpdateScoreWhenEndSeasonCommand, MethodResult<bool>>
    {
        private readonly IGameHistoryRepository _gameHistoryRepository;

        public UpdateScoreWhenEndSeasonCommandHandler(IGameHistoryRepository gameHistoryRepository)
        {
            _gameHistoryRepository = gameHistoryRepository;
        }
        public async Task<MethodResult<bool>> Handle(UpdateScoreWhenEndSeasonCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new();

            int yearCurrent = DateTime.Now.Year;
            DateTime endTimeSeason = new DateTime(yearCurrent, 12, 31, 23, 59, 59);

            var lstGameHistory = await _gameHistoryRepository.Queryable.ToListAsync(cancellationToken);

            if (DateTime.Now == endTimeSeason && lstGameHistory.Count != 0)
            {
                foreach (var item in lstGameHistory)
                {
                    item.Score = 0;
                }
                _gameHistoryRepository.UpdateList(lstGameHistory);
                await _gameHistoryRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

                return methodResult;
            }

            methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
            return methodResult;
        }
    }
}
