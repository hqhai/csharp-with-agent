// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.UserCourseSettingQuery
{
    using System.Linq.Dynamic.Core;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetUserCourseSettingsQuery : IRequest<MethodResult<IList<UserCourseSettingModel>>>
    {
        public Guid? UserId { get; set; }
    }

    public class GetUserCourseSettingsQueryHandler : IRequestHandler<GetUserCourseSettingsQuery, MethodResult<IList<UserCourseSettingModel>>>
    {
        private readonly IUserCourseSettingRepository _userCourseSettingRepository;
        private readonly AuthContext _authContext;
        private readonly IMapper _mapper;

        public GetUserCourseSettingsQueryHandler(IUserCourseSettingRepository userCourseSettingRepository, AuthContext authContext, IMapper mapper)
        {
            _userCourseSettingRepository = userCourseSettingRepository;
            _authContext = authContext;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<UserCourseSettingModel>>> Handle(GetUserCourseSettingsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<UserCourseSettingModel>>();
            var userId = request.UserId ?? _authContext.CurrentUserId;
            var userCourseSettings = await _userCourseSettingRepository.Queryable.Where(x => x.UserId == userId).OrderByDescending(x => x.CreatedDate).ToListAsync(cancellationToken);
            methodResult.Result = _mapper.Map<IList<UserCourseSettingModel>>(userCourseSettings);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
