namespace Fsel.System.Application.Commands.BlindBoxes
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.System.Application.Queries.BlindBoxes;
    using Fsel.System.Domain.Entities.BlindBoxs;
    using Fsel.System.Domain.IRepositories.BlindBoxes;
    using Fsel.System.Domain.Models.CommandModels.BlindBoxes;
    using global::System;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class BuyBlindBoxCommand : BuyBlindBoxCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class BuyBlindBoxCommandHandler : IRequestHandler<BuyBlindBoxCommand, MethodResult<bool>>
    {
        private readonly IMediator _mediator;
        private readonly IBlindBoxUserRepository _blindBoxUserRepository;
        private readonly AuthContext _authContext;
        private readonly IBlindBoxChestConfigRepository _blindBoxChestConfigRepository;

        public BuyBlindBoxCommandHandler(IMediator mediator, IBlindBoxUserRepository blindBoxUserRepository, AuthContext authContext, IBlindBoxChestConfigRepository blindBoxChestConfigRepository)
        {
            _mediator = mediator;
            _blindBoxUserRepository = blindBoxUserRepository;
            _authContext = authContext;
            _blindBoxChestConfigRepository = blindBoxChestConfigRepository;
        }

        public async Task<MethodResult<bool>> Handle(BuyBlindBoxCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            var blindBoxUser = await _blindBoxUserRepository.Queryable.FirstOrDefaultAsync(p => p.UserId == _authContext.CurrentUserId, cancellationToken);

            if (blindBoxUser == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var blindBoxResult = await _mediator.Send(new GetBlindBoxesQuery(), cancellationToken);
            if (!blindBoxResult.IsOK)
            {
                methodResult.AddError(blindBoxResult.ErrorMessages);
                return methodResult;
            }

            var blindBox = blindBoxResult.Result;
            var blindBoxChestActive = blindBox?.BlindBoxChests?.FirstOrDefault(p => p.IsActive);
            if (blindBoxChestActive == null || blindBoxChestActive.Id != request.BlindBoxChestId)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }
            if (!blindBoxChestActive.IsLast)
            {
                var blindBoxChestConfigs = await _blindBoxChestConfigRepository.Queryable.Where(p => p.BlindBoxChestId == blindBoxChestActive.Id).ToListAsync(cancellationToken);
                if (blindBoxChestConfigs == null || blindBoxChestConfigs.Count == 0)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                    return methodResult;
                }

                var blindBoxChestConfig = GetRandomBlindBoxChestConfig(blindBoxChestConfigs);
                if (blindBoxChestConfig == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                    return methodResult;
                }
            }
            return methodResult;
        }

        public static BlindBoxChestConfig? GetRandomBlindBoxChestConfig(IList<BlindBoxChestConfig> blindBoxChestConfigs)
        {
            Random random = new Random();
            double totalWeight = blindBoxChestConfigs.Sum(g => g.Percentage);
            double randomValue = random.NextDouble() * totalWeight;
            double cumulative = 0;

            foreach (var blindBoxChestConfig in blindBoxChestConfigs.ToList())
            {
                cumulative += blindBoxChestConfig.Percentage;
                if (randomValue < cumulative)
                {
                    return blindBoxChestConfig;
                }
            }
            return null;
        }
    }
}
