// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.ProsodyCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.ProsodyCommandModel;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class CreateProsodyRangeCommand : CreateProsodyCommandModels, IRequest<MethodResult<IList<ProsodyScoreModel>>>
    {

    }

    public class CreateProsodyRangeCommandHandler : IRequestHandler<CreateProsodyRangeCommand, MethodResult<IList<ProsodyScoreModel>>>
    {
        private readonly IMapper _mapper;
        private readonly IProsodyScoreRepository _scoreRepository;
        public CreateProsodyRangeCommandHandler(IMapper mapper, IProsodyScoreRepository scoreRepository)
        {
            _mapper = mapper;
            _scoreRepository = scoreRepository;
        }

        public async Task<MethodResult<IList<ProsodyScoreModel>>> Handle(CreateProsodyRangeCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<ProsodyScoreModel>> methodResult = new MethodResult<IList<ProsodyScoreModel>>();

            IList<ProsodyScore> prosodyScores = new List<ProsodyScore>();


            prosodyScores = _mapper.Map<List<ProsodyScore>>(request.ProsodyModels);
            if (prosodyScores == null || prosodyScores.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }


            await _scoreRepository.ExecuteTransactionAsync(async () =>
            {

                await _scoreRepository.AddList(prosodyScores);
                await _scoreRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<List<ProsodyScoreModel>>(prosodyScores);
                return methodResult;

            });
            return methodResult;
        }
    }
}
