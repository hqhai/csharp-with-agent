namespace Fsel.System.Application.Queries.BlindBoxes
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.System.Domain.IRepositories.BlindBoxes;
    using Fsel.System.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetBlindBoxByUserQuery : IRequest<MethodResult<BlindBoxModel>>
    {
    }

    public class GetBlindBoxByUserQueryHandler : IRequestHandler<GetBlindBoxByUserQuery, MethodResult<BlindBoxModel>>
    {
        private readonly AuthContext _authContext;
        private readonly IBlindBoxUserRepository _blindBoxUserRepository;
        private readonly IBlindBoxRepository _blindBoxRepository;
        private readonly IBlindBoxHistoryRepository _blindBoxHistoryRepository;
        private readonly IMapper _mapper;

        public GetBlindBoxByUserQueryHandler(AuthContext authContext, IBlindBoxUserRepository blindBoxUserRepository, IBlindBoxRepository blindBoxRepository, IBlindBoxHistoryRepository blindBoxHistoryRepository, IMapper mapper)
        {
            _authContext = authContext;
            _blindBoxUserRepository = blindBoxUserRepository;
            _blindBoxRepository = blindBoxRepository;
            _blindBoxHistoryRepository = blindBoxHistoryRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<BlindBoxModel>> Handle(GetBlindBoxByUserQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<BlindBoxModel>();

            var blindBoxUsers = _blindBoxUserRepository.Queryable;

            var blindBoxUser = blindBoxUsers.FirstOrDefault(p => p.UserId == _authContext.CurrentUserId);
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

            var blindBoxModel = _mapper.Map<BlindBoxModel>(blindBox);
            blindBoxModel.TotalUser = blindBoxUsers.Count();
            blindBoxModel.IsShowPopUp = blindBoxUser.IsShowPopUp;
            var blindBoxHistories = await _blindBoxHistoryRepository.Queryable.Where(p => p.CreatedUserId == _authContext.CurrentUserId).ToListAsync(cancellationToken);

            var blindBoxChestConfigs = blindBox.BlindBoxChests.SelectMany(p => p.BlindBoxChestConfigs).ToList();

            if (blindBoxModel.BlindBoxChests == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumBuyBlindBoxErrorCode.DataNotExist), nameof(blindBoxModel.BlindBoxChests), _authContext.CurrentUserId);
                return methodResult;
            }

            blindBoxModel.BlindBoxChests = blindBoxModel.BlindBoxChests.OrderBy(p => p.Index).ToList();

            foreach (var item in blindBoxModel.BlindBoxChests)
            {
                var blindBoxChestConfig = blindBoxChestConfigs
                    .FirstOrDefault(p => p.BlindBoxChestId == item.Id && p.ConfigType == EnumBlindBoxConfigType.Piece);

                bool hasHistory = blindBoxChestConfig != null &&
                                  !blindBoxHistories.Any(p => p.BlindBoxChestConfigId == blindBoxChestConfig.Id);

                if (item.IsLast)
                {
                    item.IsActive = blindBox.MileStone <= blindBoxUsers.Count() && hasHistory;
                }
                else
                {
                    item.ImagePath = null;
                    item.IsActive = hasHistory;
                }

                var blindBoxChestConfigIds = blindBoxChestConfigs.Where(p => p.BlindBoxChestId == item.Id).Select(p => p.Id).ToList();
                item.OpenCount = blindBoxHistories.Where(p => blindBoxChestConfigIds.Contains(p.BlindBoxChestConfigId)).Count();

                if (item.IsActive)
                {
                    break;
                }

                item.ImagePath = blindBoxChestConfig?.ImagePath;
            }

            var lastBlindBoxChest = blindBoxModel.BlindBoxChests.FirstOrDefault(p => p.IsLast);
            var blindBoxChestConfigPiece = blindBoxChestConfigs.FirstOrDefault(p => p.BlindBoxChestId == lastBlindBoxChest?.Id && p.ConfigType == EnumBlindBoxConfigType.Piece);
            var lastBlindBoxChestHistory = blindBoxHistories.FirstOrDefault(p => p.BlindBoxChestConfigId == blindBoxChestConfigPiece?.Id && p.IsPiece && !string.IsNullOrEmpty(p.Code));

            blindBoxModel.IsReceived = lastBlindBoxChestHistory != null;

            methodResult.Result = blindBoxModel;

            return methodResult;
        }
    }
}
