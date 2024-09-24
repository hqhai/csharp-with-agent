// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.CourseQuery.V1i1
{
    using System;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetCourseStudiedQuery : IRequest<MethodResult<CourseModel>>
    {
    }

    public class GetCourseStudiedQueryHandler : IRequestHandler<GetCourseStudiedQuery, MethodResult<CourseModel>>
    {
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly AuthContext _authContext;
        private readonly IMapper _mapper;

        public GetCourseStudiedQueryHandler(ICourseResultRepository courseResultRepository, AuthContext authContext, IMapper mapper)
        {
            _courseResultRepository = courseResultRepository;
            _authContext = authContext;
            _mapper = mapper;
        }

        public async Task<MethodResult<CourseModel>> Handle(GetCourseStudiedQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<CourseModel>();
            var courseResult = await _courseResultRepository.Queryable.Include(x => x.Course)
                                        .Where(p => p.CreatedUserId == _authContext.CurrentUserId && p.Status == EnumResultStatus.Process && p.WorkingStatus == EnumWorkingStatus.Active)
                                        .FirstOrDefaultAsync(cancellationToken);
            methodResult.Result = _mapper.Map<CourseModel>(courseResult?.Course);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
