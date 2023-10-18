// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Queries.SupportTicketQuery
{
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using Fsel.Interaction.Domain.Models.QueryModels.SupportTickets;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchSupportTicketQuery : SearchSupportTicketQueryModel, IRequest<MethodResult<PagingItemsModel<SupportTicketModel>>>
    {
    }

    public class SearchSupportTicketQueryHandler : IRequestHandler<SearchSupportTicketQuery, MethodResult<PagingItemsModel<SupportTicketModel>>>
    {
        private readonly ISupportTicketRepository _supportTicketRepository;

        public SearchSupportTicketQueryHandler(ISupportTicketRepository supportTicketRepository)
        {
            _supportTicketRepository = supportTicketRepository;
        }

        public async Task<MethodResult<PagingItemsModel<SupportTicketModel>>> Handle(SearchSupportTicketQuery request, CancellationToken cancellationToken)
        {
            MethodResult<PagingItemsModel<SupportTicketModel>> methodResult = new MethodResult<PagingItemsModel<SupportTicketModel>>();
            ArgumentNullException.ThrowIfNull(request);
            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }
            var supportTicketQuery = _supportTicketRepository.Queryable
                                .Select(x => new SupportTicketModel
                                {
                                    Id = x.Id,
                                    Code = x.Code,
                                    CreatedDate = x.CreatedDate,
                                    Content = x.Content,
                                    FullName = x.FullName,
                                    PhoneNumber = x.PhoneNumber,
                                    UserCode = x.UserCode,
                                    Email = x.Email,
                                    FilePaths = x.FilePaths,
                                    OtherProblem = x.OtherProblem,
                                    Status = x.Status,
                                    SupportQuestionId = x.SupportQuestionId,
                                    SupportCategoryId = x.SupportCategoryId,
                                    CreatedFullName = x.CreatedFullName,
                                });
            if (!string.IsNullOrEmpty(request.Keyword))
            {
                supportTicketQuery = supportTicketQuery.Where(m => m.Id.ToString() == request.Keyword || (m.Code ?? string.Empty).ToLower().Trim().Contains(request.Keyword.ToLower().Trim()));
            }
            if (request.SupportCategoryId != null)
            {
                supportTicketQuery = supportTicketQuery.Where(m => m.SupportCategoryId == request.SupportCategoryId);
            }
            if (request.SupportQuestionId != null)
            {
                supportTicketQuery = supportTicketQuery.Where(m => m.SupportQuestionId == request.SupportQuestionId);
            }
            if (request.Status != null)
            {
                supportTicketQuery = supportTicketQuery.Where(m => m.Status == request.Status);
            }
            int totalItem = await supportTicketQuery.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await supportTicketQuery
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            methodResult.Result = new PagingItemsModel<SupportTicketModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
