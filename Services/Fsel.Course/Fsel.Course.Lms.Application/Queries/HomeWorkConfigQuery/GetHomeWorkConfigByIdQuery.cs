// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.HomeWorkConfigQuery
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetHomeWorkConfigByIdQuery : IRequest<MethodResult<HomeWorkConfigModel>>
    {
        public Guid Id { get; set; }
    }

    public class GetHomeWorkConfigByIdQueryHandler : IRequestHandler<GetHomeWorkConfigByIdQuery, MethodResult<HomeWorkConfigModel>>
    {
        private readonly IHomeWorkConfigRepository _homeWorkConfigRepository;
        private readonly IHomeWorkRepository _homeWorkRepository;

        public GetHomeWorkConfigByIdQueryHandler(IHomeWorkConfigRepository homeWorkConfigRepository, IHomeWorkRepository homeWorkRepository)
        {
            _homeWorkConfigRepository = homeWorkConfigRepository;
            _homeWorkRepository = homeWorkRepository;
        }

        public async Task<MethodResult<HomeWorkConfigModel>> Handle(GetHomeWorkConfigByIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<HomeWorkConfigModel>();

            var query = await (from hc in _homeWorkConfigRepository.Queryable
                               join h in _homeWorkRepository.Queryable on hc.HomeWorkId equals h.Id
                               where hc.Id == request.Id
                               select new HomeWorkConfigModel()
                               {
                                   Id = hc.Id,
                                   CreatedUserId = hc.CreatedUserId,
                                   CreatedFullName = hc.CreatedFullName,
                                   HomeWorkName = h.Name,
                                   CreatedDate = hc.CreatedDate,
                                   CurriculumId = hc.CurriculumId,
                                   EndDate = hc.EndDate,
                                   NumberRetry = hc.NumberRetry,
                                   StartDate = hc.StartDate,
                                   HomeWorkId = hc.HomeWorkId,
                                   CourseLevel = h.CourseLevel,
                                   CourseSkill = h.CourseSkill
                               }).FirstOrDefaultAsync(cancellationToken);

            methodResult.Result = query;
            return methodResult;
        }
    }
}
