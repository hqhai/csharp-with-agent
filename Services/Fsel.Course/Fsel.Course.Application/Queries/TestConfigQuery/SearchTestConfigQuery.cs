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
        private readonly ISkillRepository _skillRepository;

        public SearchTestConfigQueryHandler(ITestConfigRepository testConfigRepository, ISkillRepository skillLevelRepository)
        {
            _testConfigRepository = testConfigRepository;
            _skillRepository = skillLevelRepository;
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
                                    Program = x.Program != null ? new CategoryModel
                                    {
                                        Name = x.Program.Name,
                                        Code = x.Program.Code,
                                        Levels = x.Program.Levels.Any() ?
                                            x.Program.Levels.Select(z => new LevelModel
                                            {
                                                Name = z.Name,
                                                Code = z.Code,
                                                LevelOrder = z.LevelOrder,
                                                Description = z.Description,
                                            }).ToList()
                                        : new List<LevelModel>(),
                                    } : new CategoryModel(),
                                    CreatedFullName = x.CreatedFullName,
                                    UpdatedFullName = x.UpdatedFullName,
                                    CreatedDate = x.CreatedDate,
                                    CreatedUserId = x.CreatedUserId,
                                    UpdatedDate = x.UpdatedDate,
                                    UpdatedUserId = x.UpdatedUserId,
                                    Skills = _skillRepository.Queryable.Where(z => x.Skills.Any(y => y.Id == z.Id))
                                    .Select(s => new Domain.Models.EntityModels.SkillModels.SkillModel
                                    {
                                        Id = s.Id,
                                        Name = s.Name,
                                        Code = s.Code,
                                    }).ToList(),
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
