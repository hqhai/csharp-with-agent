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
        private readonly IUnitResultRepository _unitResultRepository;
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly IFinalTestResultRepository _finalTestResultRepository;
        private readonly IMapper _mapper;

        public GetCourseQueryHandler(IMapper mapper, ICourseRepository courseRepository,
            ICourseUnitMockTestRepository courseUnitMockTestRepository,
            IUnitResultRepository unitResultRepository,
            IMockTestResultRepository mockTestResultRepository,
            IFinalTestResultRepository finalTestResultRepository)
        {
            _courseRepository = courseRepository;
            _courseUnitMockTestRepository = courseUnitMockTestRepository;
            _unitResultRepository = unitResultRepository;
            _mockTestResultRepository = mockTestResultRepository;
            _finalTestResultRepository = finalTestResultRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<CourseModel>> Handle(GetCourseQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<CourseModel> methodResult = new MethodResult<CourseModel>();

            var course = await _courseRepository.Queryable.Include(x => x.CourseTeachers).Where(x => !x.ParentCourseId.HasValue).FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
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
            var query = from baseQ in _courseUnitMockTestRepository.Queryable.Include(x => x.Unit).Include(x => x.MockTest).Include(x => x.FinalTest)
                        where baseQ.CourseId == course.Id
                        let hasUnitResult = _unitResultRepository.Queryable
                            .Any(ur => ur.CourseId == baseQ.CourseId && ur.UnitId == baseQ.UnitId && ur.Status != EnumResultStatus.Unfinished)
                        let hasMockTestResult = _mockTestResultRepository.Queryable
                            .Any(mr => mr.CourseId == baseQ.CourseId && mr.MockTestId == baseQ.MockTestId && mr.Status != EnumResultStatus.Unfinished)
                        let hasFinalTestResult = _finalTestResultRepository.Queryable
                            .Any(fr => fr.CourseId == baseQ.CourseId && fr.FinalTestId == baseQ.FinalTestId && fr.Status != EnumResultStatus.Unfinished)
                        select new
                        {
                            CourseUnitMockTest = baseQ,
                            HasUnitResult = hasUnitResult,
                            HasMockTestResult = hasMockTestResult,
                            HasFinalTestResult = hasFinalTestResult
                        };

            var courseUnitMockTests = await query.ToListAsync(cancellationToken);

            return courseUnitMockTests.OrderBy(x => x.CourseUnitMockTest.DisplayOrder)
                .Select(x =>
                {
                    var courseUnitMockTest = _mapper.Map<CourseUnitMockTestModel>(x.CourseUnitMockTest);
                    courseUnitMockTest.IsUsed = x.HasUnitResult || x.HasMockTestResult || x.HasFinalTestResult;
                    return courseUnitMockTest;
                })
                .ToList();
        }
    }
}
