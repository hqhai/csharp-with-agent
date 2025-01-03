// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.DisplayOrderConfigQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetDisplayOrderConfigQuery : IRequest<MethodResult<IList<DisplayOrderConfigModel>>>
    {
    }

    public class GetDisplayOrderConfigQueryHandler : IRequestHandler<GetDisplayOrderConfigQuery, MethodResult<IList<DisplayOrderConfigModel>>>
    {
        private readonly IDisplayOrderConfigRepository _displayOrderConfigRepository;
        private readonly IMapper _mapper;

        public GetDisplayOrderConfigQueryHandler(IDisplayOrderConfigRepository displayOrderConfigRepository,
                                                 IMapper mapper)
        {
            _displayOrderConfigRepository = displayOrderConfigRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<DisplayOrderConfigModel>>> Handle(GetDisplayOrderConfigQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<DisplayOrderConfigModel>> methodResult = new MethodResult<IList<DisplayOrderConfigModel>>();

            var displayOrderConfigs = await _displayOrderConfigRepository.Queryable.ToListAsync(cancellationToken);
            if (displayOrderConfigs == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(displayOrderConfigs));
                return methodResult;
            }

            methodResult.Result = _mapper.Map<IList<DisplayOrderConfigModel>>(displayOrderConfigs.OrderBy(x => x.DisplayOrder));
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
