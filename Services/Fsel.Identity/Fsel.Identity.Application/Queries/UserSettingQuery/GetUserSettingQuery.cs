// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.UserSettingQuery
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetUserSettingQuery : IRequest<MethodResult<UserSettingModel>>
    {
        public Guid UserId { get; set; }
    }

    public class GetUserSettingQueryHandler : IRequestHandler<GetUserSettingQuery, MethodResult<UserSettingModel>>
    {
        private readonly IMapper _mapper;
        private readonly IUserSettingRepository _userSettingRepository;
        private readonly AuthContext _authContext;

        public GetUserSettingQueryHandler(IMapper mapper, IUserSettingRepository userSettingRepository, AuthContext authContext)
        {
            _mapper = mapper;
            _userSettingRepository = userSettingRepository;
            _authContext = authContext;
        }

        public async Task<MethodResult<UserSettingModel>> Handle(GetUserSettingQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<UserSettingModel> methodResult = new MethodResult<UserSettingModel>();

            var userModel =await _userSettingRepository.Queryable.Where(x => x.UserId == _authContext.CurrentUserId).FirstOrDefaultAsync(cancellationToken);

            methodResult.Result = _mapper.Map<UserSettingModel>(userModel);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
