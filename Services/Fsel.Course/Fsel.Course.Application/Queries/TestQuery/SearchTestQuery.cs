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
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchTestQuery : SearchTestConfigQueryModel, IRequest<MethodResult<PagingItemsModel<TestSearchModel>>>
    {
    }

    public class SearchTestConfigQueryHandler : IRequestHandler<SearchTestQuery, MethodResult<PagingItemsModel<TestSearchModel>>>
    {
        private readonly ITestRepository _testRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILevelRepository _levelRepository;
        private readonly ITestSectionRepository _testSectionRepository;

        public SearchTestConfigQueryHandler(ITestRepository testRepository,
            ICategoryRepository categoryRepository,
            ILevelRepository levelRepository,
            ITestSectionRepository testSectionRepository)
        {
            _testRepository = testRepository;
            _categoryRepository = categoryRepository;
            _levelRepository = levelRepository;
            _testSectionRepository = testSectionRepository;
        }

        public async Task<MethodResult<PagingItemsModel<TestSearchModel>>> Handle(SearchTestQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<PagingItemsModel<TestSearchModel>> methodResult = new MethodResult<PagingItemsModel<TestSearchModel>>();

            var query = _testRepository.Queryable.Where(x => x.VersionStatus == EnumVersionStatus.LastVersion).Where(p => !p.IsArchive);
            if (!string.IsNullOrEmpty(request.Keyword))
            {
                if (Guid.TryParse(request.Keyword, out var guid))
                {
                    query = query.Where(m => m.Id == guid);
                }
                else
                {
                    var queryCode = query.Where(m => m.Code != null && m.Code.Contains(request.Keyword));
                    var queryName = query.Where(m => m.Name != null && m.Name.Contains(request.Keyword));
                    query = queryCode.Union(queryName);
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

            if (request.LayoutType.HasValue)
            {
                query = query.Where(x => _testSectionRepository.Queryable.Any(y => y.TestId == x.Id && y.LayoutType == request.LayoutType && !y.ParentId.HasValue));
            }
            var queryTest = from baseQ in query
                            join program in _categoryRepository.Queryable.Where(x => x.Type == EnumTypeCategory.Program) on baseQ.ProgramId equals program.Id
                            join level in _levelRepository.Queryable on baseQ.LevelId equals level.Id
                            select new TestSearchModel
                            {
                                Id = baseQ.Id,
                                CreatedDate = baseQ.CreatedDate,
                                CreatedFullName = baseQ.CreatedFullName,
                                CreatedUserId = baseQ.CreatedUserId,
                                UpdatedDate = baseQ.UpdatedDate,
                                UpdatedFullName = baseQ.UpdatedFullName,
                                UpdatedUserId = baseQ.UpdatedUserId,
                                LevelName = program.Name,
                                ProgramName = level.Name,
                                Name = baseQ.Name,
                                Code = baseQ.Code,
                                OriginalId = baseQ.OriginalId,
                                LevelId = baseQ.LevelId,
                                ProgramId = baseQ.ProgramId,
                                LayoutTypes = baseQ.TestSections.Select(x => x.LayoutType).Where(x => x.HasValue).Distinct().ToList(),
                            };

            int totalItem = await queryTest.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var list = await queryTest
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false);
            var originalIds = await _testRepository.GetUsedOriginalIdsAsync(list.Select(x => x.OriginalId).ToList());
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
