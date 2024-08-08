// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.UserReferrals
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Base.Managers;
    using Fsel.Core.Extensions;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchReferralCodeQuery : BaseQueryModel, IRequest<MethodResult<PagingItemsModel<SearchReferralCodeModel>>>
    {
    }

    public class SearchReferralCodeQueryHandler : IRequestHandler<SearchReferralCodeQuery, MethodResult<PagingItemsModel<SearchReferralCodeModel>>>
    {
        private readonly IUserReferralRepository _userReferralRepository;
        private readonly UserManager<User> _userManager;

        public SearchReferralCodeQueryHandler(IUserReferralRepository userReferralRepository, UserManager<User> userManager)
        {
            _userReferralRepository = userReferralRepository;
            _userManager = userManager;
        }

        public async Task<MethodResult<PagingItemsModel<SearchReferralCodeModel>>> Handle(SearchReferralCodeQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<SearchReferralCodeModel>>();

            var userReferrals = await _userReferralRepository.Queryable.ToListAsync(cancellationToken);
            var senderIds = userReferrals.Select(x => x.SenderId).Distinct().ToList();

            var users = _userManager.Users.Include(p => p.Human).Where(p => senderIds != null && senderIds.Contains(p.Id)).Select(x => new SearchReferralCodeModel
            {
                SenderId = x.Id,
                FullName = x.FullName,
                Email = x.Email,
                UserName = x.UserName,
                Code = x.Human != null ? x.Human.Code : null,
            });

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                users = users?.Where(m => (!string.IsNullOrEmpty(m.Email) && m.Email.Contains(request.Keyword)) || (!string.IsNullOrEmpty(m.Code) && m.Code.Contains(request.Keyword)) || (!string.IsNullOrEmpty(m.FullName) && m.FullName.Contains(request.Keyword)));
            }

            int totalItem = users != null ? await users.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false) : default;
            var lists = users != null ? await users
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false) : default;

            if (lists != null && lists.Count > 0)
            {
                foreach (var item in lists)
                {
                    item.NumberUser = userReferrals.Where(p => p.SenderId == item.SenderId).Count();
                    item.TotalToken = userReferrals.Where(p => p.SenderId == item.SenderId).Where(p => p.FeatureMissions != null && p.FeatureMissions.Count > 0).SelectMany(p => p.FeatureMissions!).Sum(x => x.Token);
                }
            }

            methodResult.Result = new PagingItemsModel<SearchReferralCodeModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
