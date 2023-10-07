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

            var mockTestQuery = _mockTestRepository.Queryable
                                      .Include(x => x.MockTestSections.Where(y => !y.IsDeleted))
                                      .ThenInclude(x => x.SectionGroup)
                                      .Include(x => x.CourseUnitMockTests)
                                      .Include(x => x.UnitSkillMockTests)
                                      .Select(x => new MockTestSearchModel
                                      {
                                          Id = x.Id,
                                          Name = x.Name,
                                          CreatedDate = x.CreatedDate,
                                          IsActive = x.UnitSkillMockTests.Any() || x.CourseUnitMockTests.Any(),
                                          MockTestType = x.MockTestType,
                                          CreatedFullName = x.CreatedFullName,
                                          UpdatedFullName = x.UpdatedFullName,
                                          Skills = x.MockTestSections.Select(x => x.SectionGroup).Select(n => n!.CourseSkill).ToList(),
                                      });
            if (!string.IsNullOrEmpty(request.Keyword))
            {
                mockTestQuery = mockTestQuery.Where(m => m.Id.ToString() == request.Keyword || (m.Name ?? string.Empty).ToLower().Trim().Contains(request.Keyword.ToLower().Trim()));
            }

            if (request.MockTestType != null)
            {
                mockTestQuery = mockTestQuery.Where(m => m.MockTestType == request.MockTestType);
            }

            if (request.CourseSkill != null)
            {
                mockTestQuery = mockTestQuery.Where(x => x.Skills!.Contains(request.CourseSkill ?? default));
            }

            int totalItem = await mockTestQuery.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await mockTestQuery
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            methodResult.Result = new PagingItemsModel<MockTestSearchModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
