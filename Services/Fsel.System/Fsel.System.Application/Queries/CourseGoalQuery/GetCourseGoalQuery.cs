// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.CourseGoalQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.System.Application.Services.CourseServices;
    using Fsel.System.Domain.IRepositories.CourseGoals;
    using Fsel.System.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetCourseGoalQuery : IRequest<MethodResult<CourseGoalModel>>
    {
        public Guid Id { get; set; }
    }

    public class GetCourseGoalQueryHandler : IRequestHandler<GetCourseGoalQuery, MethodResult<CourseGoalModel>>
    {
        private readonly ICourseGoalRepository _courseGoalRepository;
        private readonly IMapper _mapper;
        private readonly ICourseService _courseService;

        public GetCourseGoalQueryHandler(ICourseGoalRepository courseGoalRepository,
            IMapper mapper,
            ICourseService courseService)
        {
            _courseGoalRepository = courseGoalRepository;
            _mapper = mapper;
            _courseService = courseService;
        }

        public async Task<MethodResult<CourseGoalModel>> Handle(GetCourseGoalQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<CourseGoalModel> methodResult = new MethodResult<CourseGoalModel>();

            var courseGoal = await _courseGoalRepository.Queryable.Include(x => x.CourseGoalConfigs)
                                                        .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (courseGoal == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Id), nameof(request.Id));
                return methodResult;
            }

            var courseIds = courseGoal.CourseGoalConfigs.Select(x => x.CourseId).ToList();
            var courseResults = await _courseService.GetCoursesByIdsAsync(courseIds);
            var courses = courseResults.Content?.Result;

            var courseGoalModel = _mapper.Map<CourseGoalModel>(courseGoal);
            courseGoalModel.CourseGoalConfigs.ForEach(x =>
            {
                var course = courses?.FirstOrDefault(y => y.Id == x.CourseId);
                if (course != null)
                {
                    x.CourseName = course.Name;
                }
            });
            courseGoalModel.CourseGoalConfigs = courseGoalModel.CourseGoalConfigs.OrderBy(x => x.DisplayOrder).ToList();
            methodResult.Result = courseGoalModel;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
