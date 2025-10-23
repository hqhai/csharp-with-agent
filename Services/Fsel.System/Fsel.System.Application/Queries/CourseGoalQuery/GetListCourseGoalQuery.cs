// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.CourseGoalQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.System.Domain.IRepositories.CourseGoals;
    using Fsel.System.Domain.Models.EntityModels;
    using global::System;
    using global::System.Threading.Tasks;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetListCourseGoalQuery : IRequest<MethodResult<IList<CourseGoalModel>>>
    {
    }

    public class GetListCourseGoalQueryHandler : IRequestHandler<GetListCourseGoalQuery, MethodResult<IList<CourseGoalModel>>>
    {
        private readonly ICourseGoalRepository _courseGoalRepository;
        private readonly IMapper _mapper;

        public GetListCourseGoalQueryHandler(ICourseGoalRepository courseGoalRepository,
            IMapper mapper)
        {
            _courseGoalRepository = courseGoalRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<CourseGoalModel>>> Handle(GetListCourseGoalQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<CourseGoalModel>> methodResult = new MethodResult<IList<CourseGoalModel>>();

            var courseGoals = await _courseGoalRepository.Queryable.Include(x => x.CourseGoalConfigs)
                                                         .ToListAsync(cancellationToken);
            methodResult.Result = _mapper.Map<IList<CourseGoalModel>>(courseGoals);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
