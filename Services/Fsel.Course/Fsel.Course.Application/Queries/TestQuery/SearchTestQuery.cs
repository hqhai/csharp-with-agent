// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.TestQuery
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels.TestModels;
    using Fsel.Course.Domain.Models.QueryModels.Test;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchTestQuery : SearchTestConfigQueryModel, IRequest<MethodResult<PagingItemsModel<TestSearchModel>>>
    {
    }

    public class SearchTestConfigQueryHandler : IRequestHandler<SearchTestQuery, MethodResult<PagingItemsModel<TestSearchModel>>>
    {
        private readonly ITestRepository _testRepository;
        private readonly ITestSectionRepository _testSectionRepository;

        public SearchTestConfigQueryHandler(ITestRepository testRepository,
            ITestSectionRepository testSectionRepository)
        {
            _testRepository = testRepository;
            _testSectionRepository = testSectionRepository;
        }

        public async Task<MethodResult<PagingItemsModel<TestSearchModel>>> Handle(SearchTestQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<PagingItemsModel<TestSearchModel>> methodResult = new MethodResult<PagingItemsModel<TestSearchModel>>();

            var queryTestSection = _testSectionRepository.Queryable.Where(x => !x.ParentId.HasValue);
            if (request.LayoutType.HasValue)
            {
                queryTestSection = queryTestSection.Where(m => m.LayoutType == request.LayoutType);
            }

            var query = _testRepository.Queryable.Where(x => x.VersionStatus == EnumVersionStatus.LastVersion).Where(p => !p.IsArchive);

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                if (Guid.TryParse(request.Keyword, out var guid))
                {
                    query = query.Where(m => m.Id == guid);
                }
                else
                {
                    query = query.Where(m => (m.Name != null && m.Name.Contains(request.Keyword)) || (m.Code != null && m.Code.Contains(request.Keyword)));
                }
            }

            if (request.LevelId.HasValue)
            {
                query = query.Where(m => m.LevelId == request.LevelId);
            }
            if (request.ProgramId.HasValue)
            {
                query = query.Where(m => m.ProgramId == request.ProgramId);
            }
            var queryTest = from baseQ in query
                            join testSection in queryTestSection on baseQ.Id equals testSection.TestId into testSections
                            where !request.LayoutType.HasValue || testSections.Any()
                            select new TestSearchModel
                            {
                                Id = baseQ.Id,
                                CreatedDate = baseQ.CreatedDate,
                                CreatedFullName = baseQ.CreatedFullName,
                                CreatedUserId = baseQ.CreatedUserId,
                                UpdatedDate = baseQ.UpdatedDate,
                                UpdatedFullName = baseQ.UpdatedFullName,
                                UpdatedUserId = baseQ.UpdatedUserId,
                                LevelName = baseQ.Level != null ? baseQ.Level.Name : string.Empty,
                                ProgramName = baseQ.Program != null ? baseQ.Program.Name : string.Empty,
                                Name = baseQ.Name,
                                Code = baseQ.Code,
                                OriginalId = baseQ.OriginalId,
                                LevelId = baseQ.LevelId,
                                ProgramId = baseQ.ProgramId,
                                LayoutTypes = testSections.Select(ts => ts.LayoutType).Distinct().ToList(),
                            };

            int totalItem = await queryTest.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var list = await queryTest
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false);
            var originalIds = await _testRepository.GetUsedOriginalIdsAsync(list.Select(x => x.Id).ToList());
            foreach (var item in list)
            {
                item.IsActive = originalIds.Any(x => x == item.OriginalId);
            }
            methodResult.Result = new PagingItemsModel<TestSearchModel>(list, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
