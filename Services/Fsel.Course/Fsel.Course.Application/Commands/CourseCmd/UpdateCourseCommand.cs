using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Common.Helpers;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Enums;
using Fsel.Course.Domain.Enums.ErrorCodes;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.CommandModels.Courses;
using Fsel.Course.Domain.Models.EntiyModels;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Fsel.Course.Application.Commands.CourseCmd
{
    public class UpdateCourseCommand : UpdateCourseCommandModel, IRequest<MethodResult<CourseModel>>
    {
    }

    public class UpdateCourseTestCommandHandler : IRequestHandler<UpdateCourseCommand, MethodResult<CourseModel>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IMapper _mapper;
        private readonly IUnitRepository _unitRepository;
        private readonly ICourseUnitMockTestRepository _unitUnitMockTestRepository;
        private readonly ICourseMockTestRepository _mockTestRepository;
        private readonly ICourseUnitMockTestRepository _courseUnitRepository;

        public UpdateCourseTestCommandHandler(ICourseRepository courseRepository, ICourseUnitMockTestRepository courseUnitRepository,
            IMapper mapper,
            IUnitRepository unitRepository
            , ICourseUnitMockTestRepository unitUnitMockTestRepository
            , ICourseMockTestRepository mockTestRepository)
        {
            _courseUnitRepository = courseUnitRepository;
            _courseRepository = courseRepository;
            _mapper = mapper;
            _unitRepository = unitRepository;
            _unitUnitMockTestRepository = unitUnitMockTestRepository;
            _mockTestRepository = mockTestRepository;
        }

        public async Task<MethodResult<CourseModel>> Handle(UpdateCourseCommand request, CancellationToken cancellationToken)
        {
            MethodResult<CourseModel> methodResult = new MethodResult<CourseModel>();

            #region Validation

            var course = await _courseRepository.GetIncludeByIdAsync(request.Id);
            if (course == null)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddErrorMessage(
                    nameof(EnumCourseErrorCode.C01V),
                    new[] { MethodHelper.GenerateErrorResult(nameof(request.Id), request.Id) });
                return methodResult;
            }
            _mapper.Map(request, course);

            if (!course.IsValid())
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddResultFromErrorList(course.ErrorMessages);
                return methodResult;
            }

            if (request.CourseUnitMockTests == null)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddErrorMessage(
                    nameof(EnumCourseErrorCode.C03V));
                return methodResult;
            }

            var units = request.CourseUnitMockTests.Where(e => e.UnitId != null).Select(x => x.UnitId).ToList();
            if (_unitRepository.IsIdsInValid(request.CourseUnitMockTests.Select(x => x.UnitId)))
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddErrorMessage(
                    nameof(EnumLessonErrorCode.L03V));

                return methodResult;
            }

            var mocktestIds = request.CourseUnitMockTests.Where(e => e.MockTestId != null).Select(x => x.MockTestId).ToList();
            if (_mockTestRepository.IsIdsInValid(mocktestIds))
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddErrorMessage(
                    nameof(EnumLessonErrorCode.L03V));

                return methodResult;
            }

            List<MockTest> mocktests = new List<MockTest>();
            mocktestIds.ForEach(id =>
            {
                var mocktest = _mockTestRepository.Queryable.FirstOrDefault(x => x.Id == id);
                if (mocktest == null)
                {
                    methodResult.StatusCode = StatusCodes.Status400BadRequest;
                    methodResult.AddErrorMessage(nameof(EnumLessonErrorCode.L03V));
                }
                else
                {
                    mocktests.Add(mocktest);
                }
            });
            var checkMockTest = mocktests.Any(x => x.MockTestType == EnumMockTestType.CourseMockTest);
            if (!checkMockTest)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddErrorMessage(
                nameof(EnumLessonErrorCode.L03V));
                return methodResult;
            }

            #endregion Validation

            await _courseRepository.ExecuteTransactionAsync(async () =>
            {
                course.CourseUnitMockTests = request.CourseUnitMockTests.Select(x => new CourseUnitMockTest
                {
                    UnitId = x.UnitId,
                    MockTestId = x.MockTestId
                }).ToList();

                course = _courseRepository.Update(course);

                await _courseRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<CourseModel>(course);
                return methodResult;
            });

            return methodResult;
        }
    }
}
