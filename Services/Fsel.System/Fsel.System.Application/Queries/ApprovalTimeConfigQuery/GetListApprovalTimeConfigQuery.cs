// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.ApprovalTimeConfigQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetListApprovalTimeConfigQuery : BaseQueryModel, IRequest<MethodResult<PagingItemsModel<ApprovalTimeConfigModel>>>
    {
    }

    public class GetListCourseTimeConfigQueryHandler : IRequestHandler<GetListApprovalTimeConfigQuery, MethodResult<PagingItemsModel<ApprovalTimeConfigModel>>>
    {
        private readonly IApprovalTimeConfigRepository _approvalTimeConfigRepository;

        public GetListCourseTimeConfigQueryHandler(IApprovalTimeConfigRepository approvalTimeConfigRepository)
        {
            _approvalTimeConfigRepository = approvalTimeConfigRepository;
        }

        public async Task<MethodResult<PagingItemsModel<ApprovalTimeConfigModel>>> Handle(GetListApprovalTimeConfigQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var methodResult = new MethodResult<PagingItemsModel<ApprovalTimeConfigModel>>();
            methodResult.Result = new PagingItemsModel<ApprovalTimeConfigModel>();

            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }
            var approvalTimeConfigQuery = _approvalTimeConfigRepository.Queryable
                                    .Select(x => new ApprovalTimeConfigModel
                                    {
                                        Id = x.Id,
                                        ExpiredTime = x.ExpiredTime,
                                        ApprovalTimeType = x.ApprovalTimeType,
                                        CreatedDate = x.CreatedDate,
                                        CreatedFullName = x.CreatedFullName,
                                        CreatedUserId = x.CreatedUserId,
                                    });

            int totalItem = await approvalTimeConfigQuery.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await approvalTimeConfigQuery
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);


            methodResult.Result = new PagingItemsModel<ApprovalTimeConfigModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
