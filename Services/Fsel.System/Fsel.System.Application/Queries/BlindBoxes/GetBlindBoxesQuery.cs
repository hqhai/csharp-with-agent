namespace Fsel.System.Application.Queries.BlindBoxes
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Shared.Enums;
    using Fsel.System.Domain.IRepositories.BlindBoxes;
    using Fsel.System.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetBlindBoxesQuery : IRequest<MethodResult<BlindBoxModel>>
    {
    }

    public class GetBlindBoxesQueryHandler : IRequestHandler<GetBlindBoxesQuery, MethodResult<BlindBoxModel>>
    {
        private readonly AuthContext _authContext;
        private readonly IBlindBoxUserRepository _blindBoxUserRepository;
        private readonly IBlindBoxRepository _blindBoxRepository;
        private readonly IBlindBoxChestConfigRepository _blindBoxChestConfigRepository;
        private readonly IBlindBoxChestRepository _blindBoxChestRepository;
        private readonly IBlindBoxHistoryRepository _blindBoxHistoryRepository;
        private readonly IMapper _mapper;

        public GetBlindBoxesQueryHandler(AuthContext authContext, IBlindBoxUserRepository blindBoxUserRepository, IBlindBoxRepository blindBoxRepository, IBlindBoxChestConfigRepository blindBoxChestConfigRepository, IBlindBoxChestRepository blindBoxChestRepository, IBlindBoxHistoryRepository blindBoxHistoryRepository, IMapper mapper)
        {
            _authContext = authContext;
            _blindBoxUserRepository = blindBoxUserRepository;
            _blindBoxRepository = blindBoxRepository;
            _blindBoxChestConfigRepository = blindBoxChestConfigRepository;
            _blindBoxChestRepository = blindBoxChestRepository;
            _blindBoxHistoryRepository = blindBoxHistoryRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<BlindBoxModel>> Handle(GetBlindBoxesQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<BlindBoxModel>();

            var blindBoxUser = await _blindBoxUserRepository.Queryable.FirstOrDefaultAsync(p => p.UserId == _authContext.CurrentUserId, cancellationToken);
            if (blindBoxUser == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(blindBoxUser), _authContext.CurrentUserId);
                return methodResult;
            }

            var blindBox = await _blindBoxRepository.Queryable.Include(p => p.BlindBoxChests).ThenInclude(p => p.BlindBoxChestConfigs).FirstOrDefaultAsync(p => p.Id == blindBoxUser.BlindBoxId, cancellationToken);

            if (blindBox == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(blindBox), _authContext.CurrentUserId);
                return methodResult;
            }

            var blindBoxModel = _mapper.Map<BlindBoxModel>(blindBox);

            var blindBoxHistories = await _blindBoxHistoryRepository.Queryable.Where(p => p.CreatedUserId == _authContext.CurrentUserId).ToListAsync(cancellationToken);

            var blindBoxChestConfigs = blindBox.BlindBoxChests.SelectMany(p => p.BlindBoxChestConfigs).ToList();

            if (blindBoxModel.BlindBoxChests == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(blindBoxModel.BlindBoxChests), _authContext.CurrentUserId);
                return methodResult;
            }

            foreach (var item in blindBoxModel.BlindBoxChests.OrderBy(p => p.Index).ToList())
            {
                item.ImagePath = null;
                var blindBoxChestConfig = blindBoxChestConfigs.FirstOrDefault(p => p.BlindBoxChestId == item.Id && p.ConfigType == EnumBlindBoxConfigType.Piece);
                if (blindBoxChestConfig != null && !blindBoxHistories.Any(p => p.BlindBoxChestConfigId == blindBoxChestConfig.Id))
                {
                    item.IsActive = true;
                    break;
                }
                else
                {
                    item.ImagePath = blindBoxChestConfig?.ImagePath;
                    item.IsActive = false;
                }
            }

            return methodResult;
        }
    }
}
