// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Queries.SupportTicketQuery
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Interaction.Domain.Enums.ErrorCodes;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class GetSupportTicketQuery : IRequest<MethodResult<SupportTicketModel>>
    {
        public Guid Id { get; set; }
    }

    public class GetSupportTicketQueryHandler : IRequestHandler<GetSupportTicketQuery, MethodResult<SupportTicketModel>>
    {
        private readonly IMapper _mapper;
        private readonly ISupportTicketRepository _supportTicketRepository;

        public GetSupportTicketQueryHandler(IMapper mapper, ISupportTicketRepository supportTicketRepository)
        {
            _mapper = mapper;
            _supportTicketRepository = supportTicketRepository;
        }

        public async Task<MethodResult<SupportTicketModel>> Handle(GetSupportTicketQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<SupportTicketModel> methodResult = new MethodResult<SupportTicketModel>();

            var supportTicket = await _supportTicketRepository.GetIncludeByIdAsync(request.Id);

            if (supportTicket == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSupportQuestionErrorCode.SupportQuestionNotExist), nameof(request.Id), request.Id);
                return methodResult;
            }

            methodResult.Result = _mapper.Map<SupportTicketModel>(supportTicket);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
