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

    public class GetSenderConfigsQuery : IRequest<MethodResult<IList<SenderConfigModel>>>
    {
    }

    public class GetSenderConfigsQueryHandler : IRequestHandler<GetSenderConfigsQuery, MethodResult<IList<SenderConfigModel>>>
    {
        private readonly ISenderConfigRepository _senderConfigRepository;
        private readonly IMapper _mapper;

        public GetSenderConfigsQueryHandler(ISenderConfigRepository senderConfigRepository,
                                            IMapper mapper)
        {
            _senderConfigRepository = senderConfigRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<SenderConfigModel>>> Handle(GetSenderConfigsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<SenderConfigModel>> methodResult = new MethodResult<IList<SenderConfigModel>>();

            var senderConfigs = await _senderConfigRepository.Queryable.AsNoTracking().ToListAsync(cancellationToken);
            if (senderConfigs == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(senderConfigs));
                return methodResult;
            }

            methodResult.Result = _mapper.Map<IList<SenderConfigModel>>(senderConfigs);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
