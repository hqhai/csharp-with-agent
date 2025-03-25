namespace Fsel.System.Application.Commands.BlindBoxes
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
    using Fsel.System.Domain.IRepositories.BlindBoxes;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class ReceiveBlindBoxCommand : IRequest<MethodResult<bool>>
    {
    }

    public class ReceiveBlindBoxCommandHandler : IRequestHandler<ReceiveBlindBoxCommand, MethodResult<bool>>
    {
        private readonly IBlindBoxHistoryRepository _blindBoxHistoryRepository;
        private readonly IBlindBoxRepository _blindBoxRepository;
        private readonly AuthContext _authContext;
        private readonly IBlindBoxUserRepository _blindBoxUserRepository;

        public ReceiveBlindBoxCommandHandler(IBlindBoxHistoryRepository blindBoxHistoryRepository, IBlindBoxRepository blindBoxRepository, AuthContext authContext, IBlindBoxUserRepository blindBoxUserRepository)
        {
            _blindBoxHistoryRepository = blindBoxHistoryRepository;
            _blindBoxRepository = blindBoxRepository;
            _authContext = authContext;
            _blindBoxUserRepository = blindBoxUserRepository;
        }

        public async Task<MethodResult<bool>> Handle(ReceiveBlindBoxCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var blindBoxUser = await _blindBoxUserRepository.Queryable.FirstOrDefaultAsync(p => p.UserId == _authContext.CurrentUserId, cancellationToken);
            if (blindBoxUser == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumBuyBlindBoxErrorCode.NotIncludedInTheEvent), nameof(blindBoxUser), _authContext.CurrentUserId);
                return methodResult;
            }

            var currentDate = DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam);

            var blindBox = await _blindBoxRepository.Queryable.Include(p => p.BlindBoxChests).ThenInclude(p => p.BlindBoxChestConfigs).FirstOrDefaultAsync(p => p.Id == blindBoxUser.BlindBoxId && p.StartDate <= currentDate && p.EndDate >= currentDate, cancellationToken);

            if (blindBox == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumBuyBlindBoxErrorCode.EventHasExpired), nameof(blindBox), _authContext.CurrentUserId);
                return methodResult;
            }

            var blindBoxChestConfigPieceIds = blindBox.BlindBoxChests.SelectMany(p => p.BlindBoxChestConfigs).Where(p => p.ConfigType == Shared.Enums.EnumBlindBoxConfigType.Piece).Select(p => p.Id).ToList();

            var blindBoxHistories = await _blindBoxHistoryRepository.Queryable.WhereBulkContains(blindBoxChestConfigPieceIds, p => p.BlindBoxChestConfigId).Where(p => p.CreatedUserId == _authContext.CurrentUserId && p.IsPiece).ToListAsync(cancellationToken);

            blindBoxHistories = blindBoxHistories.DistinctBy(p => p.BlindBoxChestConfigId).ToList();

            if (blindBoxHistories == null || blindBoxHistories.Count != blindBoxChestConfigPieceIds.Count)
            {
                methodResult.AddErrorBadRequest(nameof(EnumBuyBlindBoxErrorCode.NotReceivedEnoughPieces), nameof(blindBox), _authContext.CurrentUserId);
                return methodResult;
            }

            var lastChestConfigs = blindBox.BlindBoxChests.FirstOrDefault(p => p.IsLast)?.BlindBoxChestConfigs.FirstOrDefault(p => p.ConfigType == Shared.Enums.EnumBlindBoxConfigType.Piece);

            if (lastChestConfigs == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumBuyBlindBoxErrorCode.DataNotExist), nameof(blindBox), _authContext.CurrentUserId);
                return methodResult;
            }

            var blindBoxHistory = blindBoxHistories.FirstOrDefault(p => p.BlindBoxChestConfigId == lastChestConfigs.Id && string.IsNullOrEmpty(p.Code));

            if (blindBoxHistory == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumBuyBlindBoxErrorCode.DataNotExist), nameof(blindBox), _authContext.CurrentUserId);
                return methodResult;
            }

            await _blindBoxHistoryRepository.ExecuteTransactionAsync(async () =>
            {
                blindBoxHistory.Code = NumberHelper.GenerateCode(8);
                _blindBoxHistoryRepository.Update(blindBoxHistory);
                await _blindBoxUserRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.Result = true;
                return methodResult;
            });

            return methodResult;
        }
    }
}
