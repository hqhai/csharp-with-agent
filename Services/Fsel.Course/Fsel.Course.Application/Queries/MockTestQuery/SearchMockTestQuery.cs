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
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.QueryModels.MockTests;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchMockTestQuery : SearchMockTestQueryModel, IRequest<MethodResult<PagingItemsModel<MockTestSearchModel>>>
    {
    }

    public class SearchMockTestQueryHandler : IRequestHandler<SearchMockTestQuery, MethodResult<PagingItemsModel<MockTestSearchModel>>>
    {
        private readonly IMapper _mapper;
        private readonly IMockTestRepository _mockTestRepository;

        public SearchMockTestQueryHandler(IMapper mapper, IMockTestRepository mockTestRepository)
        {
            _mapper = mapper;
            _mockTestRepository = mockTestRepository;
        }

        public async Task<MethodResult<PagingItemsModel<MockTestSearchModel>>> Handle(SearchMockTestQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<PagingItemsModel<MockTestSearchModel>> methodResult = new MethodResult<PagingItemsModel<MockTestSearchModel>>();

            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }
            var mockTestQuery = _mockTestRepository.Queryable
                                      .Include(x => x.MockTestSections.Where(y => !y.IsDeleted))
                                      .ThenInclude(x => x.SectionGroup)
                                      .Include(x => x.CourseUnitMockTests)
                                      .Include(x => x.UnitSkillMockTests)
                                      .Select(x => new MockTestSearchModel
                                      {
                                          Id = x.Id,
                                          Name = x.Name,
                                          CourseType = x.CourseType,
                                          CreatedDate = x.CreatedDate,
                                          IsActive = x.UnitSkillMockTests.Any() || x.CourseUnitMockTests.Any(),
                                          MockTestType = x.MockTestType,
                                          CreatedFullName = x.CreatedFullName,
                                          UpdatedFullName = x.UpdatedFullName,
                                          Skills = x.MockTestSections.Select(x => x.SectionGroup).Select(n => n!.CourseSkill).ToList(),
                                      });
            if (!string.IsNullOrEmpty(request.Keyword))
            {
                mockTestQuery = mockTestQuery.Where(m => m.Id.ToString() == request.Keyword || (m.Name ?? string.Empty).Contains(request.Keyword));
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
