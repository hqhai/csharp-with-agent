// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Course.Domain.Enums;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.EntityModels;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using EntityCourse = Fsel.Course.Domain.Entities.Course;

namespace Fsel.Course.Application.Queries.CourseQuery
{
    public class GetCourseQuery : IRequest<MethodResult<CourseModel>>
    {
        public Guid Id { get; set; }
    }

    public class GetCourseQueryHandler : IRequestHandler<GetCourseQuery, MethodResult<CourseModel>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly ICourseUnitMockTestRepository _courseUnitMockTestRepository;
        private readonly IMapper _mapper;

        public GetCourseQueryHandler(IMapper mapper, ICourseRepository courseRepository, ICourseUnitMockTestRepository courseUnitMockTestRepository)
        {
            _courseRepository = courseRepository;
            _courseUnitMockTestRepository = courseUnitMockTestRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<CourseModel>> Handle(GetCourseQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<CourseModel> methodResult = new MethodResult<CourseModel>();

            var course = await _courseRepository.Queryable.Include(x => x.CourseTeachers).FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (course == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(course));
                return methodResult;
            }
            var courseModel = _mapper.Map<CourseModel>(course);
            courseModel.CourseUnitMockTests = await GetCourseUnitMockTestsAsync(course, cancellationToken);
            methodResult.Result = courseModel;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<IList<CourseUnitMockTestModel>> GetCourseUnitMockTestsAsync(EntityCourse course, CancellationToken cancellationToken)
        {
            var courseUnitMockTests = await _courseUnitMockTestRepository.Queryable.Include(x => x.FinalTest)
                                                                        .ThenInclude(x => x.FinalTestResults)
                                                                        .Include(x => x.Unit)
                                                                        .ThenInclude(x => x.UnitResults)
                                                                        .Include(x => x.MockTest)
                                                                        .ThenInclude(x => x.MockTestResults)
                                                                        .Where(x => x.CourseId == course.Id)
                                                                        .ToListAsync(cancellationToken);
            return courseUnitMockTests.OrderBy(x => x.DisplayOrder).Select(x =>
            {
                var courseUnitMockTest = _mapper.Map<CourseUnitMockTestModel>(x);
                if (courseUnitMockTest.UnitId.HasValue)
                {
                    courseUnitMockTest.IsUsed = (x.Unit!.UnitResults.Any() && x.Unit!.UnitResults.Any(x => x.Status != EnumResultStatus.Unfinished));
                }
                else if (courseUnitMockTest.MockTestId.HasValue)
                {
                    courseUnitMockTest.IsUsed = (x.MockTest!.MockTestResults.Any() && x.MockTest!.MockTestResults.Any(x => x.Status != EnumResultStatus.Unfinished));
                }
                else if (courseUnitMockTest.FinalTestId.HasValue)
                {
                    courseUnitMockTest.IsUsed = (x.FinalTest!.FinalTestResults.Any() && x.FinalTest!.FinalTestResults.Any(x => x.Status != EnumResultStatus.Unfinished));
                }
                return courseUnitMockTest;
            }).ToList();
        }
    }
}
