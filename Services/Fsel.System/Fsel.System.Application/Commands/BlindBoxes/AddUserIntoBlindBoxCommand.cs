namespace Fsel.System.Application.Commands.BlindBoxes
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.System.Domain.Entities.BlindBoxs;
    using Fsel.System.Domain.IRepositories.BlindBoxes;
    using Fsel.System.Domain.Models.CommandModels.BlindBoxes;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class AddUserIntoBlindBoxCommand : AddUserIntoBlindBoxCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class AddUserIntoBlindBoxCommandHandler : IRequestHandler<AddUserIntoBlindBoxCommand, MethodResult<bool>>
    {
        private readonly IBlindBoxUserRepository _blindBoxUserRepository;
        private readonly IBlindBoxRepository _blindBoxRepository;

        public AddUserIntoBlindBoxCommandHandler(IBlindBoxUserRepository blindBoxUserRepository, IBlindBoxRepository blindBoxRepository)
        {
            _blindBoxUserRepository = blindBoxUserRepository;
            _blindBoxRepository = blindBoxRepository;
        }

        public async Task<MethodResult<bool>> Handle(AddUserIntoBlindBoxCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            if (await _blindBoxUserRepository.Queryable.AnyAsync(p => p.UserId == request.UserId, cancellationToken))
            {
                return methodResult;
            }
            else
            {
                var currentDate = DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam);
                var blindBox = await _blindBoxRepository.Queryable.Where(p => p.CreatedDate <= currentDate && p.EndDate >= currentDate).FirstOrDefaultAsync(cancellationToken);
                if (blindBox == null)
                {
                    return methodResult;
                }

                await _blindBoxUserRepository.ExecuteTransactionAsync(async () =>
                {
                    _blindBoxUserRepository.Add(new BlindBoxUser()
                    {
                        BlindBoxId = blindBox.Id,
                        UserId = request.UserId,
                        NumberOpen = request.NumberOpen,
                        IsWin = request.IsWin,
                        IsShowPopUp = true
                    });
                    await _blindBoxUserRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                    return methodResult;
                });
            }
            return methodResult;
        }
    }
}
