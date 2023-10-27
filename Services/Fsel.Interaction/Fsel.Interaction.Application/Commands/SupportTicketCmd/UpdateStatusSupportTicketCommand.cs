// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Commands.SupportTicketCmd
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Interaction.Domain.Enums.ErrorCodes;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.CommandModels.SupportTickets;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class UpdateStatusSupportTicketCommand : UpdateStatusSupportTicketCommandModel, IRequest<MethodResult<SupportTicketModel>>
    {
    }

    public class UpdateStatusSupportTicketCommandHandler : IRequestHandler<UpdateStatusSupportTicketCommand, MethodResult<SupportTicketModel>>
    {
        private readonly IMapper _mapper;
        private readonly ISupportTicketRepository _supportTicketRepository;

        public UpdateStatusSupportTicketCommandHandler(IMapper mapper, ISupportTicketRepository supportTicketRepository)
        {
            _mapper = mapper;
            _supportTicketRepository = supportTicketRepository;
        }

        public async Task<MethodResult<SupportTicketModel>> Handle(UpdateStatusSupportTicketCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<SupportTicketModel> methodResult = new MethodResult<SupportTicketModel>();
            var supportTicket = await _supportTicketRepository.GetByIdAsync(request.Id);

            #region Validation

            if (supportTicket == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSupportQuestionErrorCode.SupportQuestionNotExist));
                return methodResult;
            }
            _mapper.Map(request, supportTicket);

            #endregion Validation

            await _supportTicketRepository.ExecuteTransactionAsync(async () =>
            {
                supportTicket = _supportTicketRepository.Update(supportTicket);

                await _supportTicketRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<SupportTicketModel>(supportTicket);
                return methodResult;
            });

            return methodResult;
        }
    }
}
