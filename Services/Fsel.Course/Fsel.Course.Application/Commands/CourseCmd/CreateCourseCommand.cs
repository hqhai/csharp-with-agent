using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Common.Helpers;
using Fsel.Course.Domain.Models.CommandModels.Courses;
using Fsel.Course.Domain.Models.EntiyModels;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Enums.ErrorCodes;
using Fsel.Course.Domain.IRepositories;
using MediatR;
using Microsoft.AspNetCore.Http;
using EntityCourse = Fsel.Course.Domain.Entities.Course;

namespace Fsel.Course.Application.Commands.CourseCmd
{
    public class CreateCourseCommand : CreateCourseCommandModel, IRequest<MethodResult<CourseModel>>
    {
    }

    public class CreateCourseCommandHandler : IRequestHandler<CreateCourseCommand, MethodResult<CourseModel>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly ICourseUnitMockTestRepository _courseUnitRepository;
        private readonly IUnitRepository _unitRepository;
        private readonly IMapper _mapper;

        public CreateCourseCommandHandler(ICourseRepository courseRepository
            , ICourseUnitMockTestRepository courseUnitRepository
            , IUnitRepository unitRepository
            , IMapper mapper)
        {
            _unitRepository = unitRepository;
            _courseUnitRepository = courseUnitRepository;
            _courseRepository = courseRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<CourseModel>> Handle(CreateCourseCommand request, CancellationToken cancellationToken)
        {
            MethodResult<CourseModel> methodResult = new MethodResult<CourseModel>();

            #region Validation

            EntityCourse course = _mapper.Map<EntityCourse>(request);

            if (!course.IsValid())
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddResultFromErrorList(course.ErrorMessages);
                return methodResult;
            }
            if (request.UnitIds == null)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddErrorMessage(
                    nameof(EnumCourseErrorCode.C03V),
                    new[] { MethodHelper.GenerateErrorResult(nameof(request.UnitIds), request.UnitIds) });
                return methodResult;
            }

            if (_unitRepository.IsIdsInValid(request.UnitIds))
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddErrorMessage(
                    nameof(EnumUnitErrorCode.U03V));

                return methodResult;
            }

            #endregion Validation

            await _courseRepository.ExecuteTransactionAsync(async () =>
            {
                course.CourseUnitMockTests = request.UnitIds.Select(x => new CourseUnitMockTest
                {
                    UnitId = x
                }).ToList();

                course = _courseRepository.Add(course);

                await _courseRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<CourseModel>(course);
                return methodResult;
            });

            return methodResult;
        }
    }
}
