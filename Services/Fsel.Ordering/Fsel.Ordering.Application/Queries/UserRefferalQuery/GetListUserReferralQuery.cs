// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Queries.UserRefferalQuery
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Ordering.Domain.Entities;
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

        public GetListUserReferralQueryHandler(IUserReferralRepository userReferralRepository)
        {
            _userReferralRepository = userReferralRepository;
        }

        public async Task<MethodResult<IList<UserReferralModel>>> Handle(GetListUserReferralQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<UserReferralModel>>();

            var userReferral = await _userReferralRepository.Queryable
                                    .Select(x => new UserReferralModel
                                    {
                                        Id = x.Id,
                                        CreatedDate = x.CreatedDate,
                                        IndexNumber = x.IndexNumber,
                                        SenderId = x.SenderId,
                                        ReceiverId = x.ReceiverId,
                                    }).ToListAsync(cancellationToken);
            methodResult.Result = userReferral;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
