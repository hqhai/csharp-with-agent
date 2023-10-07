// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Application.Commands.QuestBankCmd
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Cms.PlanetDefender.Domain.IRepositories;
    using Fsel.Cms.PlanetDefender.Domain.Models.CommandModel;
    using Fsel.Cms.PlanetDefender.Domain.Models.EntityModels;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class UpdateStatusQuestBankCommand : UpdateStatusQuestBankCommandModel, IRequest<MethodResult<QuestBankModel>>
    {
    }

    public class UpdateStatusQuestBankCommandHandler : IRequestHandler<UpdateStatusQuestBankCommand, MethodResult<QuestBankModel>>
    {
        private readonly IMapper _mapper;
        private readonly IQuestBankRepository _questBankRepository;

        public UpdateStatusQuestBankCommandHandler(IMapper mapper, IQuestBankRepository questBankRepository)
        {
            _mapper = mapper;
            _questBankRepository = questBankRepository;
        }

        public async Task<MethodResult<QuestBankModel>> Handle(UpdateStatusQuestBankCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<QuestBankModel> methodResult = new MethodResult<QuestBankModel>();
            var questBank = await _questBankRepository.GetByIdAsync(request.Id);

            #region Validation

            if (questBank == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(questBank));
                return methodResult;
            }
            _mapper.Map(request, questBank);

            #endregion Validation

            await _questBankRepository.ExecuteTransactionAsync(async () =>
            {
                questBank = _questBankRepository.Update(questBank);

                await _questBankRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<QuestBankModel>(questBank);
                return methodResult;
            });

            return methodResult;
        }
    }
}
