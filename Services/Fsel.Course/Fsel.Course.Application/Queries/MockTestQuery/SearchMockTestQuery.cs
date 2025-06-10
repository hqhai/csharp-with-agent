// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.MockTestQuery
{
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.QueryModels.MockTests;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchMockTestQuery : SearchMockTestQueryModel, IRequest<MethodResult<PagingItemsModel<MockTestSearchModel>>>
    {
    }

    public class SearchMockTestQueryHandler : IRequestHandler<SearchMockTestQuery, MethodResult<PagingItemsModel<MockTestSearchModel>>>
    {
        private readonly IMockTestRepository _mockTestRepository;

        public SearchMockTestQueryHandler(IMockTestRepository mockTestRepository)
        {
            _mockTestRepository = mockTestRepository;
        }

        public async Task<MethodResult<PagingItemsModel<MockTestSearchModel>>> Handle(SearchMockTestQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<PagingItemsModel<MockTestSearchModel>> methodResult = new MethodResult<PagingItemsModel<MockTestSearchModel>>();

            var mockTestQuery = _mockTestRepository.Queryable.Where(p => !p.IsArchive)
                                      .Select(x => new MockTestSearchModel
                                      {
                                          Id = x.Id,
                                          Name = x.Name,
                                          CreatedDate = x.CreatedDate,
                                          IsActive = x.UnitSkillMockTests.Any() || x.CourseUnitMockTests.Any(),
                                          MockTestType = x.MockTestType,
                                          CreatedFullName = x.CreatedFullName,
                                          UpdatedFullName = x.UpdatedFullName,
                                          Version = x.Version,
                                          Skills = x.MockTestSections.Select(x => x.SectionGroup).Select(n => n!.CourseSkill).ToList(),
                                          SkillNames = x.MockTestSections.Select(x => x.SectionGroup).Where(x => x != null && x.Skill != null).Select(x => x!.Skill!.Name).ToList(),
                                          SkillIds = x.MockTestSections.Select(x => x.SectionGroup).Where(x => x != null && x.Skill != null).Select(x => x!.Skill!.Id).ToList(),
                                      });

            request.Keyword = request.Keyword?.Trim().ToLower(CultureInfo.CurrentCulture);
            if (!string.IsNullOrEmpty(request.Keyword))
            {
                if (Guid.TryParse(request.Keyword, out var guid))
                {
                    mockTestQuery = mockTestQuery.Where(m => m.Id == guid);
                }
                else
                {
                    mockTestQuery = mockTestQuery.Where(m => m.Name != null && m.Name.Contains(request.Keyword));
                }
            }

            if (request.MockTestType != null)
            {
                mockTestQuery = mockTestQuery.Where(m => m.MockTestType == request.MockTestType);
            }

            if (request.CourseSkill != null)
            {
                mockTestQuery = mockTestQuery.Where(x => x.Skills!.Contains(request.CourseSkill ?? default));
            }
            if (request.SkillId.HasValue)
            {
                mockTestQuery = mockTestQuery.Where(x => x.SkillIds != null && x.SkillIds.Contains(request.SkillId.Value));
            }
            int totalItem = await mockTestQuery.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await mockTestQuery
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            lists.ForEach(x =>
            {
                x.Skills = x.Skills?.OrderBy(x => x).ToList();
            });
            methodResult.Result = new PagingItemsModel<MockTestSearchModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
