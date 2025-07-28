// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.TokenHistoryCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using Fsel.System.Domain.IRepositories;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class RecallGiftCommand : IRequest<MethodResult<bool>>
    {
        public IList<Guid>? Ids { get; set; }
    }

    public class RecallGiftCommandHandler : IRequestHandler<RecallGiftCommand, MethodResult<bool>>
    {
        private readonly ITokenHistoryRepository _tokenHistoryRepository;
        private readonly IMediator _mediator;

        public RecallGiftCommandHandler(ITokenHistoryRepository tokenHistoryRepository, IMediator mediator)
        {
            _tokenHistoryRepository = tokenHistoryRepository;
            _mediator = mediator;
        }
        public async Task<MethodResult<bool>> Handle(RecallGiftCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(request.Ids);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            var tokenHistories = await _tokenHistoryRepository.Queryable
                                                              .Where(x => request.Ids.Contains(x.Id))
                                                              .OrderBy(x => x.UserId)
                                                              .ThenBy(x => x.CreatedDate)
                                                              .ToListAsync(cancellationToken);
            if (tokenHistories == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist));
                return methodResult;
            }

            foreach (var tokenHistory in tokenHistories)
            {
                var reclaimGiftCoins = new List<TokenHistoryQueueModel>
                {
                    new TokenHistoryQueueModel
                    {
                        VolatileToken = tokenHistory.VolatileToken,
                        UserId = tokenHistory.UserId,
                        Feature = EnumTokenFeature.ReclaimGift,
                        Mission = EnumTokenMission.ReclaimGiftCoins,
                        Type = EnumTokenHistoryType.Recevived,
                        Config = tokenHistory.Config
                    }
                };

                await _mediator.Send(new CreateTokenHistoryCommand { TokenHistorys = reclaimGiftCoins }, cancellationToken);
            }

            methodResult.Result = true;
            return methodResult;
        }
    }
}
