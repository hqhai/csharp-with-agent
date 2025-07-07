// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.TestConfigQuery
{
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels.TestConfig;
    using Fsel.Course.Domain.Models.QueryModels.MockTests;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchTestConfigQuery : SearchTestConfigQueryModel, IRequest<MethodResult<PagingItemsModel<TestConfigModel>>>
    {
    }

    public class SearchTestConfigQueryHandler : IRequestHandler<SearchTestConfigQuery, MethodResult<PagingItemsModel<TestConfigModel>>>
    {
        private readonly ITestConfigRepository _testConfigRepository;
        private readonly IMapper _mapper;

        public SearchTestConfigQueryHandler(ITestConfigRepository testConfigRepository, IMapper mapper)
        {
            _testConfigRepository = testConfigRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<PagingItemsModel<TestConfigModel>>> Handle(SearchTestConfigQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<PagingItemsModel<TestConfigModel>> methodResult = new MethodResult<PagingItemsModel<TestConfigModel>>();

            var testConfigQuery = _testConfigRepository.Queryable
                                    .Include(x => x.Program)
                                    .Include(x => x.Level)
                                    .Select(x => _mapper.Map<TestConfigModel>(x));
            request.Keyword = request.Keyword?.Trim().ToLower(CultureInfo.CurrentCulture);
            if (!string.IsNullOrEmpty(request.Keyword))
            {
                if (Guid.TryParse(request.Keyword, out var guid))
                {
                    testConfigQuery = testConfigQuery.Where(m => m.Id == guid);
                }
                else
                {
                    testConfigQuery = testConfigQuery.Where(m => m.Name != null && m.Name.Contains(request.Keyword)
                    || m.Code != null && m.Code.Contains(request.Keyword));
                }
            }

            if (request.LevelId != null)
            {
                testConfigQuery = testConfigQuery
                    .Where(m => m.LevelId == request.LevelId);
            }
            if (request.ProgramId != null)
            {
                testConfigQuery = testConfigQuery
                    .Where(m => m.ProgramId == request.ProgramId);
            }
            if (request.LayoutType != null)
            {
                testConfigQuery = testConfigQuery
                    .Where(m => m.LayoutType == request.LayoutType);
            }

            int totalItem = await testConfigQuery.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var testConfigModels = await testConfigQuery
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false);

            methodResult.Result = new PagingItemsModel<TestConfigModel>(testConfigModels, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
