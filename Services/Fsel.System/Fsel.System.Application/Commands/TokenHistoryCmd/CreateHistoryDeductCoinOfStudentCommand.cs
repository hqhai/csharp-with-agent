using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Shared.Enums;
using Fsel.Shared.Models.ShareModels;
using Fsel.System.Domain.Entities;
using Fsel.System.Domain.IRepositories;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Fsel.System.Application.Commands.TokenHistoryCmd
{
    public class CreateHistoryDeductCoinOfStudentCommand : CreateHistoryDeductCoinOfStudentCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class CreateHistoryDeductCoinOfStudentCommandHandler : IRequestHandler<CreateHistoryDeductCoinOfStudentCommand, MethodResult<bool>>
    {
        private readonly ITokenHistoryRepository _tokenHistoryRepository;
        private readonly IMapper _mapper;

        public CreateHistoryDeductCoinOfStudentCommandHandler(ITokenHistoryRepository tokenHistoryRepository, IMapper mapper)
        {
            _tokenHistoryRepository = tokenHistoryRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<bool>> Handle(CreateHistoryDeductCoinOfStudentCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var tokenHistory = _mapper.Map<TokenHistory>(request);
            tokenHistory.Type = EnumTokenHistoryType.Exchanged;
            if (!tokenHistory.IsValid())
            {
                methodResult.AddError(tokenHistory.ErrorMessages);
                return methodResult;
            }

            await _tokenHistoryRepository.ExecuteTransactionAsync(async () =>
            {
                _tokenHistoryRepository.Add(tokenHistory);
                await _tokenHistoryRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = true;
                return methodResult;
            });

            return methodResult;
        }
    }
}
