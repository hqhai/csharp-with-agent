// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.SenderConfigQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetSenderConfigByIdQuery : IRequest<MethodResult<SenderConfigModel>>
    {
        public Guid Id { get; set; }
    }

    public class GetSenderConfigByIdQueryHandler : IRequestHandler<GetSenderConfigByIdQuery, MethodResult<SenderConfigModel>>
    {
        private readonly ISenderConfigRepository _senderConfigRepository;
        private readonly IMapper _mapper;

        public GetSenderConfigByIdQueryHandler(ISenderConfigRepository senderConfigRepository,
                                               IMapper mapper)
        {
            _senderConfigRepository = senderConfigRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<SenderConfigModel>> Handle(GetSenderConfigByIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<SenderConfigModel> methodResult = new MethodResult<SenderConfigModel>();

            var senderConfig = await _senderConfigRepository.Queryable.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (senderConfig == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(senderConfig));
                return methodResult;
            }

            methodResult.Result = _mapper.Map<SenderConfigModel>(senderConfig);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
