// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Queries.UserRefferalQuery
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Ordering.Application.Services.UserService;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetListUserReferralQuery : IRequest<MethodResult<IList<UserReferralModel>>>
    {
    }

    public class GetListUserReferralQueryHandler : IRequestHandler<GetListUserReferralQuery, MethodResult<IList<UserReferralModel>>>
    {
        private readonly IUserReferralRepository _userReferralRepository;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;

        public GetListUserReferralQueryHandler(IUserReferralRepository userReferralRepository, AuthContext authContext, IUserService userService)
        {
            _userReferralRepository = userReferralRepository;
            _authContext = authContext;
            _userService = userService;
        }

        public async Task<MethodResult<IList<UserReferralModel>>> Handle(GetListUserReferralQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<UserReferralModel>>();

            var userReferral = await _userReferralRepository.Queryable
                                     .Where(x => x.SenderId == _authContext.CurrentUserId)
                                    .Select(x => new UserReferralModel
                                    {
                                        Id = x.Id,
                                        CreatedDate = x.CreatedDate,
                                        IndexNumber = x.IndexNumber,
                                        SenderId = x.SenderId,
                                        ReceiverId = x.ReceiverId,
                                    }).ToListAsync(cancellationToken);

            var listUser = await _userService.GetStudentsByIdsAsync(userReferral.Select(x => x.ReceiverId).ToList());
            var userResult = listUser.Content?.Result;

            foreach (var item in userReferral)
            {
                var user = userResult?.FirstOrDefault(x => x.UserId == item.ReceiverId);
                item.StudentName = user?.User?.FullName;
                item.StudentCode = user?.User?.Code;
            }
            methodResult.Result = userReferral;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
