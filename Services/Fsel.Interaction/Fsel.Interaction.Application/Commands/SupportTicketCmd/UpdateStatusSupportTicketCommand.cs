// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Commands.SupportTicketCmd
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Interaction.Domain.Entities;
    using Fsel.Interaction.Domain.Enums.ErrorCodes;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.CommandModels.SupportTickets;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
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

            if (supportTicket == null || supportTicket.Status == EnumSupportTicketStatus.Solved)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSupportQuestionErrorCode.SupportQuestionNotExist));
                return methodResult;
            }
            if (supportTicket.Status == EnumSupportTicketStatus.NotSeen && request.Status == EnumSupportTicketStatus.Solved)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSupportQuestionErrorCode.SupportQuestionNotExist));
                return methodResult;
            }
            if (supportTicket.Status == EnumSupportTicketStatus.Seen && request.Status == EnumSupportTicketStatus.NotSeen)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSupportQuestionErrorCode.SupportQuestionNotExist));
                return methodResult;
            }
            _mapper.Map(request, supportTicket);

            #endregion Validation

            switch (supportTicket.Status)
            {
                case EnumSupportTicketStatus.Seen:
                    await TimeSupportTicket(supportTicket, cancellationToken);
                    break;

                case EnumSupportTicketStatus.Solved:
                    await TimeSupportTicket(supportTicket, cancellationToken);
                    break;
            }

            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = _mapper.Map<SupportTicketModel>(supportTicket);
            return methodResult;
        }

        public async Task TimeSupportTicket(SupportTicket supportTicket, CancellationToken cancellationToken)
        {
            _supportTicketRepository.Update(supportTicket);
            await _supportTicketRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
        }
    }

}
