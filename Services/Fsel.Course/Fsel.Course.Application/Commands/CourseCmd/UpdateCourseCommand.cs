using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Course.Domain.Enums;
using Fsel.Course.Domain.Enums.ErrorCodes;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.CommandModels.Courses;
using Fsel.Course.Domain.Models.EntityModels;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

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
        private readonly IMockTestRepository _mockTestRepository;
        private readonly ICourseUnitMockTestRepository _courseUnitRepository;

        public UpdateCourseTestCommandHandler(ICourseRepository courseRepository
            , ICourseUnitMockTestRepository courseUnitRepository
            , IMapper mapper
            , IUnitRepository unitRepository
            , IMockTestRepository mockTestRepository)
        {
            _courseUnitRepository = courseUnitRepository;
            _courseRepository = courseRepository;
            _mapper = mapper;
            _unitRepository = unitRepository;
            _mockTestRepository = mockTestRepository;
        }

        public async Task<MethodResult<CourseModel>> Handle(UpdateCourseCommand request, CancellationToken cancellationToken)
        {
            MethodResult<CourseModel> methodResult = new MethodResult<CourseModel>();

            #region Validation

            var course = _courseRepository.Queryable.Where(e => e.Id == request.Id).Include(e => e.CourseUnitMockTests).Include(e => e.CourseTeachers).FirstOrDefault();
            if (course == null)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddError(
                    nameof(EnumCourseErrorCode.C01V));
                return methodResult;
            }
            _mapper.Map(request, course);

            if (!course.IsValid())
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddResultFromErrorList(course.ErrorMessages);
                return methodResult;
            }

            if (course.Status != EnumCourseStatus.New)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddError(
                nameof(EnumCourseErrorCode.C03V));
                return methodResult;
            }

            if (request?.CourseUnitMockTests == null)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddError(
                    nameof(EnumCourseUnitMockTestErrorCode.CUM01V));
                return methodResult;
            }

            if (request.CourseTeachers == null)
            {
                methodResult.AddErrorBadRequest(
                    nameof(EnumCourseTeacherErrorCode.CT04V));
                return methodResult;
            }

            var units = request.CourseUnitMockTests.Where(e => e.UnitId != null).Select(x => x.UnitId).ToList();
            if (_unitRepository.IsIdsInValid(units.Where(e => e.HasValue).Select(e => e!.Value)))
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddError(
                    nameof(EnumUnitErrorCode.U03V));

                return methodResult;
            }

            var mocktestIds = request.CourseUnitMockTests.Where(e => e.MockTestId != null).Select(x => x.MockTestId);
            if (_mockTestRepository.IsIdsInValid(mocktestIds.Where(e => e.HasValue).Select(e => e!.Value)))
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddError(
                nameof(EnumMockTestErrorCode.MT03V));
                return methodResult;
            }

            var mocktests = await _mockTestRepository.GetByIdsAsync(mocktestIds.Where(e => e.HasValue).Select(e => e!.Value));

            var checkMockTest = mocktests.Any(x => x.MockTestType == EnumMockTestType.CourseMockTest);
            if (!checkMockTest)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddError(
                nameof(EnumMockTestErrorCode.MT05V));
                return methodResult;
            }

            #endregion Validation

            await _courseRepository.ExecuteTransactionAsync(async () =>
            {
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
