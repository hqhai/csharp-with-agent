// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.UserSettingQuery
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetListUserSettingsQuery : IRequest<MethodResult<List<UserSettingModel>>>
    {
        public List<Guid?>? UserIds { get; set; }
    }

    public class GetListUserSettingsQueryHandler : IRequestHandler<GetListUserSettingsQuery, MethodResult<List<UserSettingModel>>>
    {
        private readonly IMapper _mapper;
        private readonly IUserSettingRepository _userSettingRepository;

        public GetListUserSettingsQueryHandler(IMapper mapper, IUserSettingRepository userSettingRepository)
        {
            _mapper = mapper;
            _userSettingRepository = userSettingRepository;
        }

        public async Task<MethodResult<List<UserSettingModel>>> Handle(GetListUserSettingsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<List<UserSettingModel>> methodResult = new MethodResult<List<UserSettingModel>>();

            if (request.UserIds == null || request.UserIds.Count == 0)
            {
                methodResult.Result = new List<UserSettingModel>();
                return methodResult;
            }

            var userModel = await _userSettingRepository.Queryable.Where(x => request.UserIds.Contains(x.UserId)).ToListAsync(cancellationToken);

            methodResult.Result = _mapper.Map<List<UserSettingModel>>(userModel);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
