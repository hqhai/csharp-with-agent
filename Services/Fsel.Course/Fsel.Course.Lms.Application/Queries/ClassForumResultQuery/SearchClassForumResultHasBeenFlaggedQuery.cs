// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ClassForumResultQuery
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.QueryModels.ClassForumResults;
    using Fsel.Course.Lms.Application.Services.InteractionService;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchClassForumResultHasBeenFlaggedQuery : SearchClassForumResultHasBeenFlaggedQueryModel, IRequest<MethodResult<PagingItemsModel<ClassForumResultModel>>>
    {
    }

    public class SearchClassForumResultHasBeenFlaggedQueryHandler : IRequestHandler<SearchClassForumResultHasBeenFlaggedQuery, MethodResult<PagingItemsModel<ClassForumResultModel>>>
    {
        private readonly IClassForumResultRepository _classForumResultRepository;
        private readonly IInteractionService _interactionService;

        public SearchClassForumResultHasBeenFlaggedQueryHandler(IClassForumResultRepository classForumResultRepository, IInteractionService interactionService)
        {
            _classForumResultRepository = classForumResultRepository;
            _interactionService = interactionService;
        }

        public async Task<MethodResult<PagingItemsModel<ClassForumResultModel>>> Handle(SearchClassForumResultHasBeenFlaggedQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<PagingItemsModel<ClassForumResultModel>> methodResult = new MethodResult<PagingItemsModel<ClassForumResultModel>>();
            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }

            var classForumResultQuery = _classForumResultRepository.Queryable
                                    .Include(x => x.ClassForum)
                                    .Select(x => new ClassForumResultModel
                                    {
                                        Id = x.Id,
                                        CreatedDate = x.CreatedDate,
                                        CreatedUserId = x.CreatedUserId,
                                        CreatedFullName = x.CreatedFullName,
                                        Content = x.Content,
                                        Status = x.Status,
                                    });

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                classForumResultQuery = classForumResultQuery.Where(m => m.Id.ToString() == request.Keyword || (m.CreatedFullName ?? string.Empty).ToLower().Trim().Contains(request.Keyword.ToLower().Trim()));
            }
            int totalItem = await classForumResultQuery.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await classForumResultQuery
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            methodResult.Result = new PagingItemsModel<ClassForumResultModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
