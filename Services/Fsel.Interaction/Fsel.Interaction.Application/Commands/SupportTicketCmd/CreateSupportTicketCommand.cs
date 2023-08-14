// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Commands.SupportTicketCmd
{
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
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class CreateSupportTicketCommand : CreateSupportTicketCommandModel, IRequest<MethodResult<SupportTicketModel>>
    {
    }

    public class CreateSupportTicketCommandHandler : IRequestHandler<CreateSupportTicketCommand, MethodResult<SupportTicketModel>>
    {
        private readonly IMapper _mapper;
        private readonly ISupportTicketRepository _supportTicketRepository;
        private readonly ISupportCategoryRepository _supportCategoryRepository;
        private readonly ISupportQuestionRepository _supportQuestionRepository;

        public CreateSupportTicketCommandHandler(IMapper mapper, ISupportTicketRepository supportTicketRepository, ISupportCategoryRepository supportCategoryRepository, ISupportQuestionRepository supportQuestionRepository)
        {
            _mapper = mapper;
            _supportTicketRepository = supportTicketRepository;
            _supportCategoryRepository = supportCategoryRepository;
            _supportQuestionRepository = supportQuestionRepository;
        }

        public async Task<MethodResult<SupportTicketModel>> Handle(CreateSupportTicketCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<SupportTicketModel> methodResult = new MethodResult<SupportTicketModel>();

            SupportTicket supportTicket = _mapper.Map<SupportTicket>(request);
            if (!supportTicket.IsValid())
            {
                methodResult.AddErrorBadRequest(supportTicket.ErrorMessages);
                return methodResult;
            }
            if (!await _supportCategoryRepository.AnyAsync(request.SupportCategoryId ?? default))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSupportQuestionErrorCode.SupportCategoryIdNotExist), nameof(request.SupportCategoryId), request.SupportCategoryId);
                return methodResult;
            }

            if (!await _supportQuestionRepository.AnyAsync(request.SupportQuestionId ?? default))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSupportQuestionErrorCode.SupportCategoryIdNotExist), nameof(request.SupportCategoryId), request.SupportCategoryId);
                return methodResult;
            }

            await _supportTicketRepository.ExecuteTransactionAsync(async () =>
            {
                supportTicket.Code = NumberHelper.GenerateCodeNumber(8);
                supportTicket.Status = EnumSupportTicketStatus.NotSeen;
                supportTicket = _supportTicketRepository.Add(supportTicket);
                await _supportTicketRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<SupportTicketModel>(supportTicket);
                return methodResult;
            });

            return methodResult;
        }
    }
}
