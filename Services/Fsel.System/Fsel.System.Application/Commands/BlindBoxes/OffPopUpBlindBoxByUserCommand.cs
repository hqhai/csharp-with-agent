namespace Fsel.System.Application.Commands.BlindBoxes
{
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.System.Domain.IRepositories.BlindBoxes;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class OffPopUpBlindBoxByUserCommand : IRequest<MethodResult<bool>>
    {
    }

    public class OffPopUpBlindBoxByUserCommandHandler : IRequestHandler<OffPopUpBlindBoxByUserCommand, MethodResult<bool>>
    {
        private readonly IBlindBoxUserRepository _blindBoxUserRepository;
        private readonly AuthContext _authContext;

        public OffPopUpBlindBoxByUserCommandHandler(IBlindBoxUserRepository blindBoxUserRepository, AuthContext authContext)
        {
            _blindBoxUserRepository = blindBoxUserRepository;
            _authContext = authContext;
        }

        public async Task<MethodResult<bool>> Handle(OffPopUpBlindBoxByUserCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var blindBoxUser = await _blindBoxUserRepository.Queryable.FirstOrDefaultAsync(p => p.UserId == _authContext.CurrentUserId, cancellationToken);
            if (blindBoxUser == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumBuyBlindBoxErrorCode.NotIncludedInTheEvent), nameof(blindBoxUser), _authContext.CurrentUserId);
                return methodResult;
            }
            await _blindBoxUserRepository.ExecuteTransactionAsync(async () =>
            {
                blindBoxUser.IsShowPopUp = false;
                _blindBoxUserRepository.Update(blindBoxUser);
                await _blindBoxUserRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.Result = true;
                return methodResult;
            });

            return methodResult;
        }
    }
}
