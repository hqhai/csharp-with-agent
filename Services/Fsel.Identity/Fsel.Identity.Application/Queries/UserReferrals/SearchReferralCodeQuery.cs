// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.UserReferrals
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
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

            var users = _userManager.Users.Include(p => p.Senders).Where(p => senderIds != null && senderIds.Contains(p.Id));

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                if (request.Keyword.IsValidEmail())
                {
                    users = users.Where(m => m.Email!.Contains(request.Keyword));
                }
                else
                {
                    var codeQuery = users.Where(m => m.Code!.Contains(request.Keyword));
                    var fullNameQuery = users.Where(m => m.FullName!.Contains(request.Keyword));
                    users = codeQuery.Union(fullNameQuery);
                }
            }

            var queryData = users.Select(x => new SearchReferralCodeModel
            {
                SenderId = x.Id,
                FullName = x.FullName,
                Email = x.Email,
                UserName = x.UserName,
                Code = x.Code,
                NumberUser = x.Senders.Count(),
            });
            queryData = queryData?.OrderByDescending(p => p.NumberUser);

            int totalItem = queryData != null ? await queryData.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false) : default;
            var lists = queryData != null ? await queryData
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
