// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Application.Commands.QuestBankCmd
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Cms.PlanetDefender.Domain.Entities;
    using Fsel.Cms.PlanetDefender.Domain.IRepositories;
    using Fsel.Cms.PlanetDefender.Domain.Models.CommandModel;
    using Fsel.Cms.PlanetDefender.Domain.Models.EntityModels;
    using Fsel.Common.ActionResults;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class MassUploadQuestBankCommand : MassUploadQuestBankCommandModel, IRequest<MethodResult<IList<QuestBankModel>>>
    {
    }

    public class MassUploadQuestBankCommandHandler : IRequestHandler<MassUploadQuestBankCommand, MethodResult<IList<QuestBankModel>>>
    {
        private readonly IQuestBankRepository _questBankRepository;
        private readonly IMapper _mapper;

        public MassUploadQuestBankCommandHandler(IQuestBankRepository questBankRepository, IMapper mapper)
        {
            _questBankRepository = questBankRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<QuestBankModel>>> Handle(MassUploadQuestBankCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<QuestBankModel>>();

            var questBanks = new List<QuestBank>();

            foreach (var item in request.GameVocabularyIds!)
            {
                var questBank = new QuestBank
                {
                    GameVocabularyId = item,
                    IsActive = false,
                };

                questBanks.Add(questBank);
            }
            await _questBankRepository.ExecuteTransactionAsync(async () =>
            {
                await _questBankRepository.AddList(questBanks);
                await _questBankRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<IList<QuestBankModel>>(questBanks);
                return methodResult;
            });

            return methodResult;
        }
    }
}
