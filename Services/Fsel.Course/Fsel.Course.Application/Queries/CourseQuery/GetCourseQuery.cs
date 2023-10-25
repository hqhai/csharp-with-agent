// Copyright (c) Atlantic. All rights reserved.

using System.Threading;
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
using static MassTransit.Logging.OperationName;

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

        public async Task<MethodResult<CourseModel>> Handle(GetCourseQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<CourseModel> methodResult = new MethodResult<CourseModel>();
            var course = await _courseRepository.GetIncludeByIdAsync(request.Id);
            if (course is null)
            {
                 methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(course));
                return methodResult;
            }

            var courseModel = _mapper.Map<CourseModel>(course);
            var courseUnitMockTestModels = _mapper.Map<IList<CourseUnitMockTestModel>>(course.CourseUnitMockTests.OrderBy(x => x.DisplayOrder).ToList());

            foreach (var courseUnitMockTest in courseUnitMockTestModels)
            {
                courseUnitMockTest.Status = await GetStatus(courseUnitMockTest);
            }

            courseModel.CourseUnitMockTests = courseUnitMockTestModels;

            methodResult.Result = courseModel;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<bool> GetStatus(CourseUnitMockTestModel courseUnitMockTest)
        {
            if (courseUnitMockTest.UnitId.HasValue)
            {
                return await _unitResultRepository.Queryable.AnyAsync(x => x.UnitId == courseUnitMockTest.UnitId!.Value && x.Status != EnumResultStatus.Unfinished);
            }
            else if ( courseUnitMockTest.FinalTestId.HasValue)
            {
                return await _finalTestResultRepository.Queryable.AnyAsync(x => x.FinalTestId == courseUnitMockTest.FinalTestId!.Value && x.Status != EnumResultStatus.Unfinished);
            }
            else if (courseUnitMockTest.MockTestId.HasValue)
            {
                return await _finalTestResultRepositoryMock.Queryable.AnyAsync(x => x.MockTestId == courseUnitMockTest.MockTestId!.Value && x.Status != EnumResultStatus.Unfinished);
            }

            return false;
        }

    }
}
