// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.FocusTimeConfigQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using global::System;
    using global::System.Threading.Tasks;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetListFocusTimeConfigQuery : IRequest<MethodResult<IList<FocusTimeConfigModel>>>
    {
    }

    public class GetListFocusTimeConfigQueryHandler : IRequestHandler<GetListFocusTimeConfigQuery, MethodResult<IList<FocusTimeConfigModel>>>
    {
        private readonly IFocusTimeConfigRepository _focusTimeConfigRepository;
        private readonly IMapper _mapper;
        private readonly ITokenConfigRepository _tokenConfigRepository;

        public GetListFocusTimeConfigQueryHandler(IFocusTimeConfigRepository focusTimeConfigRepository, IMapper mapper, ITokenConfigRepository tokenConfigRepository)
        {
            _focusTimeConfigRepository = focusTimeConfigRepository;
            _mapper = mapper;
            _tokenConfigRepository = tokenConfigRepository;
        }

        public async Task<MethodResult<IList<FocusTimeConfigModel>>> Handle(GetListFocusTimeConfigQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<FocusTimeConfigModel>> methodResult = new MethodResult<IList<FocusTimeConfigModel>>();

            var listFocusTime = await _focusTimeConfigRepository.Queryable.ToListAsync(cancellationToken);

            var tokenConfig = await _tokenConfigRepository.Queryable
                .Where(x => x.Feature == EnumTokenFeature.FocusMode && x.Mission == EnumTokenMission.FocusTime)
                .FirstOrDefaultAsync(cancellationToken);

            var config = tokenConfig?.Config.Deserialize<TokenFocusTime>();

            var listFocusTimeModel = _mapper.Map<IList<FocusTimeConfigModel>>(listFocusTime);

            foreach (var item in listFocusTimeModel)
            {
                item.TokenNumber = config!.FocusTimes!.Where(x => x.FocusTimeId == item.Id).Select(x => x.Number ?? default).FirstOrDefault();
            }

            methodResult.Result = listFocusTimeModel;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
