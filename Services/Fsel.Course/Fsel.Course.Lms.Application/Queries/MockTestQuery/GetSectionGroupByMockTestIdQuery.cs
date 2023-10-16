// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.MockTestQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetSectionGroupByMockTestIdQuery : IRequest<MethodResult<IList<SectionGroupModel>>>
    {
        public Guid MockTestId { get; set; }
    }

    public class GetSectionBySectionGroupIdQueryHandler : IRequestHandler<GetSectionGroupByMockTestIdQuery, MethodResult<IList<SectionGroupModel>>>
    {
        private readonly ISectionGroupRepository _sectionGroupRepository;

        public GetSectionBySectionGroupIdQueryHandler(ISectionGroupRepository sectionGroupRepository, AuthContext authContext)
        {
            _sectionGroupRepository = sectionGroupRepository;
        }

        public async Task<MethodResult<IList<SectionGroupModel>>> Handle(GetSectionGroupByMockTestIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<SectionGroupModel>>();
            var sectionGroups = await _sectionGroupRepository.Queryable.Include(x => x.MockTestSections).Include(x => x.SectionGroupResults.)

            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
