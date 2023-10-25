// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Enums;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.EntityModels;
using Fsel.Shared.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Application.Queries.CourseQuery
{
    public class GetCourseQuery : IRequest<MethodResult<List<CourseUnitMockTestModel>>>
    {
        public Guid Id { get; set; }
    }

    public class GetCourseQueryHandler : IRequestHandler<GetCourseQuery, MethodResult<List<CourseUnitMockTestModel>>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly ICourseUnitMockTestRepository _courseUnitMockTestRepository;
        private readonly IUnitResultRepository _unitResultRepository;
        private readonly IFinalTestResultRepository _finalTestResultRepository;
        private readonly IMockTestResultRepository _finalTestResultRepositoryMock;
        private readonly IMapper _mapper;

        public GetCourseQueryHandler(ICourseRepository courseRepository, ICourseUnitMockTestRepository courseUnitMockTestRepository, IUnitResultRepository unitResultRepository, IFinalTestResultRepository finalTestResultRepository, IMockTestResultRepository finalTestResultRepositoryMock, IMapper mapper)
        {
            _courseRepository = courseRepository;
            _courseUnitMockTestRepository = courseUnitMockTestRepository;
            _unitResultRepository = unitResultRepository;
            _finalTestResultRepository = finalTestResultRepository;
            _finalTestResultRepositoryMock = finalTestResultRepositoryMock;
            _mapper = mapper;
        }

        public async Task<MethodResult<List<CourseUnitMockTestModel>>> Handle(GetCourseQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<List<CourseUnitMockTestModel>> methodResult = new MethodResult<List<CourseUnitMockTestModel>>();

            var course = await _courseRepository.GetIncludeByIdAsync(request.Id);

            if (course == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(course));
                return methodResult;
            }
            var courseUnitMockTests = course.CourseUnitMockTests.OrderBy(x=>x.DisplayOrder).ToList();
            var courseModel = _mapper.Map<CourseModel>(course);


            var unitIds = courseUnitMockTests.Where(x => x.UnitId.HasValue).Select(x => x.UnitId!.Value).ToList();


            var unitResults = await _unitResultRepository.Queryable.Where(x => unitIds.Any(y=> y ==x.UnitId)).GroupBy(x => x.UnitId).Select(x => new
            {
                Id = x.Key,
                Status = x.Select(x => x).Any(x => x.Status != EnumResultStatus.Unfinished)
            }).ToListAsync(cancellationToken);

            foreach(var courseUnitMockTest in courseUnitMockTests)
            {

            }
            if (course.CourseType == EnumCourseType.Academic)
            {
                var finalTestIds = courseUnitMockTests.Where(x => x.FinalTestId.HasValue).Select(x => x.FinalTestId!.Value).ToList();
                var finalTestResults = await _finalTestResultRepository.Queryable.Where(x => finalTestIds.Contains(x.FinalTestId)).GroupBy(x => x.FinalTestId).Select(x => new
                {
                    Id = x.Key,
                    Status = x.Select(x => x).Any(x => x.Status != EnumResultStatus.Unfinished)
                }).ToListAsync(cancellationToken);
            }
            else
            {
                var mockTestIds = courseUnitMockTests.Where(x => x.MockTestId.HasValue).Select(x => x.MockTestId!.Value).ToList();
                var mockTestResults = await _finalTestResultRepositoryMock.Queryable.Where(x => mockTestIds.Contains(x.MockTestId)).GroupBy(x => x.MockTestId).Select(x => new
                {
                    Id = x.Key,
                    Status = x.Select(x => x).Any(x => x.Status != EnumResultStatus.Unfinished)
                }).ToListAsync(cancellationToken);
            }

            methodResult.Result = courseModel;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

    }
}
