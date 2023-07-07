// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.ExtraPracticeQuery
{
    using System.Collections.Generic;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetMockTestsByExtraPracticeQuery : IRequest<MethodResult<IList<MockTestSearchModel>>>
    {
        public EnumMockTestType? Type { get; set; }
    }

    public class GetMockTestsByExtraPracticeQueryHandler : IRequestHandler<GetMockTestsByExtraPracticeQuery, MethodResult<IList<MockTestSearchModel>>>
    {
        private readonly IMockTestRepository _mockTestRepository;
        private readonly IMapper _mapper;

        public GetMockTestsByExtraPracticeQueryHandler(IMockTestRepository mockTestRepository, IMapper mapper)
        {
            _mockTestRepository = mockTestRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<MockTestSearchModel>>> Handle(GetMockTestsByExtraPracticeQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<MockTestSearchModel>> methodResult = new MethodResult<IList<MockTestSearchModel>>();

            var mockTests = await _mockTestRepository.Queryable.Include(x => x.ExtraPractice)
                                      .Include(x => x.MockTestSections.Where(y => !y.IsDeleted))
                                      .ThenInclude(x => x.SectionGroup)
                                      .Include(x => x.CourseUnitMockTests)
                                      .Include(x => x.UnitSkillMockTests)
                                      .Where(y => y.ExtraPractice == null && !request.Type.HasValue || y.MockTestType == request.Type)
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
                                      }).ToListAsync(cancellationToken);

            methodResult.Result = _mapper.Map<IList<MockTestSearchModel>>(mockTests);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
