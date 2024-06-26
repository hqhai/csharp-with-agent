// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.ProsodyCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.ProsodyCommandModel;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class UpdateProsodyScoreCommand : ProsodyCommandModel, IRequest<MethodResult<ProsodyScoreModel>>
    {
    }

    public class UpdateProsodyScoreCommandHandler : IRequestHandler<UpdateProsodyScoreCommand, MethodResult<ProsodyScoreModel>>
    {
        private readonly IMapper _mapper;
        private readonly IProsodyScoreRepository _scoreRepository;
        public UpdateProsodyScoreCommandHandler(IMapper mapper, IProsodyScoreRepository scoreRepository)
        {
            _mapper = mapper;
            _scoreRepository = scoreRepository;
        }

        public async Task<MethodResult<ProsodyScoreModel>> Handle(UpdateProsodyScoreCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<ProsodyScoreModel> methodResult = new MethodResult<ProsodyScoreModel>();

            var prosodyScore = _scoreRepository.Queryable.FirstOrDefault(x => x.Id == request.Id);

            if (prosodyScore == null || request == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            prosodyScore = _mapper.Map(request, prosodyScore);

            await _scoreRepository.ExecuteTransactionAsync(async () =>
            {

                _scoreRepository.Update(prosodyScore);
                await _scoreRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<ProsodyScoreModel>(prosodyScore);
                return methodResult;

            });
            return methodResult;
        }
    }
}
