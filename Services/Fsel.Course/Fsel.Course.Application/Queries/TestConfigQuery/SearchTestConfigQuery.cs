// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.TestConfigQuery
{
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.QueryModels.MockTests;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchTestConfigQuery : SearchTestConfigQueryModel, IRequest<MethodResult<PagingItemsModel<TestConfigSearchModel>>>
    {
    }

    public class SearchTestConfigQueryHandler : IRequestHandler<SearchTestConfigQuery, MethodResult<PagingItemsModel<TestConfigSearchModel>>>
    {
        private readonly ITestConfigRepository _testConfigRepository;

        public SearchTestConfigQueryHandler(ITestConfigRepository testConfigRepository)
        {
            _testConfigRepository = testConfigRepository;
        }

        public async Task<MethodResult<PagingItemsModel<TestConfigSearchModel>>> Handle(SearchTestConfigQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<PagingItemsModel<TestConfigSearchModel>> methodResult = new MethodResult<PagingItemsModel<TestConfigSearchModel>>();

            var testConfigQuery = _testConfigRepository.Queryable
                                .Include(tc => tc.Program)
                                .Select(x => new TestConfigSearchModel
                                {
                                    Id = x.Id,
                                    Name = x.Name,
                                    Code = x.Code,
                                    IsActive = x.IsActive,
                                    Program = x.Program,
                                    CreatedFullName = x.CreatedFullName,
                                    UpdatedFullName = x.UpdatedFullName,
                                    CreatedDate = x.CreatedDate,
                                    CreatedUserId = x.CreatedUserId,
                                    UpdatedDate = x.UpdatedDate,
                                    UpdatedUserId = x.UpdatedUserId,
                                });

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
                    .Where(m => m.Program != null
                                && m.Program.Levels.Any(z => z.Id == request.LevelId));
            }
            if (request.ProgramId != null)
            {
                testConfigQuery = testConfigQuery
                    .Where(m => m.Program != null && m.Program.Id == request.ProgramId);
            }

            int totalItem = await testConfigQuery.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await testConfigQuery
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            methodResult.Result = new PagingItemsModel<TestConfigSearchModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
