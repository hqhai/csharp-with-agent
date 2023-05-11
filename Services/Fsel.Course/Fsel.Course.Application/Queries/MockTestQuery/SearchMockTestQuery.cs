// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.MockTestQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Course.Application.Queries.PlacementTestQuery;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.QueryModels.MockTests;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchMockTestQuery : SearchMockTestQueryModel, IRequest<MethodResult<PagingItemsModel<MockTestModel>>>
    {
    }

    public class SearchMockTestQueryHandler : IRequestHandler<SearchMockTestQuery, MethodResult<PagingItemsModel<MockTestModel>>>
    {
        private readonly IMapper _mapper;
        private readonly IMockTestRepository _mockTestRepository;

        public SearchMockTestQueryHandler(IMapper mapper, IMockTestRepository mockTestRepository)
        {
            _mapper = mapper;
            _mockTestRepository = mockTestRepository;
        }

        public async Task<MethodResult<PagingItemsModel<MockTestModel>>> Handle(SearchMockTestQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<PagingItemsModel<MockTestModel>> methodResult = new MethodResult<PagingItemsModel<MockTestModel>>();

            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }
            var mockTestQuery = _mockTestRepository.SearchAsync(request.MockTestType);
            if (!string.IsNullOrEmpty(request.Keyword))
            {
                mockTestQuery = mockTestQuery.Where(m => m.Id.ToString() == request.Keyword || (m.Name ?? string.Empty).Contains(request.Keyword));
            }
            int totalItem = await mockTestQuery.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await mockTestQuery
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            methodResult.Result = new PagingItemsModel<MockTestModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
