// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.TestQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels.TestModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetTestHistoryQuery : BaseQueryModel, IRequest<MethodResult<PagingItemsModel<TestModel>>>
    {
        public Guid Id { get; set; }
    }

    public class GetTestHistoryQueryHandler : IRequestHandler<GetTestHistoryQuery, MethodResult<PagingItemsModel<TestModel>>>
    {
        private readonly ITestRepository _testRepository;
        private readonly IMapper _mapper;

        public GetTestHistoryQueryHandler(ITestRepository testRepository, IMapper mapper)
        {
            _testRepository = testRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<PagingItemsModel<TestModel>>> Handle(GetTestHistoryQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<PagingItemsModel<TestModel>> methodResult = new MethodResult<PagingItemsModel<TestModel>>();

            var test = await _testRepository.GetByIdAsync(request.Id);
            if (test == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(test), request.Id);
                return methodResult;
            }
            var query = _testRepository.Queryable.Where(x => x.OriginalId == test.OriginalId);
            int totalItem = await query.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await query.ApplySortAndPaging(request)
                        .AsNoTracking()
                        .ToListAsync(cancellationToken: cancellationToken)
                        .ConfigureAwait(false);
            methodResult.Result = new PagingItemsModel<TestModel>(_mapper.Map<List<TestModel>>(lists), request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
