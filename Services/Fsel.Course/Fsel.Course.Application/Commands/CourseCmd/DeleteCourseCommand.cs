using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Course.Domain.Enums;
using Fsel.Course.Domain.Enums.ErrorCodes;
using Fsel.Course.Domain.IRepositories;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Application.Commands.CourseCmd
{
    public class DeleteCourseCommand : IRequest<MethodResult<bool>>
    {
        public Guid Id { get; set; }
    }

    public class DeleteCourseCommandHandler : IRequestHandler<DeleteCourseCommand, MethodResult<bool>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IMapper _mapper;
        private readonly ICourseUnitMockTestRepository _courseUnitRepository;

        public DeleteCourseCommandHandler(ICourseRepository courseRepository, ICourseUnitMockTestRepository courseUnitRepository,
            IMapper mapper)
        {
            _courseUnitRepository = courseUnitRepository;
            _courseRepository = courseRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<bool>> Handle(DeleteCourseCommand request, CancellationToken cancellationToken)
        {
            MethodResult<bool> methodResult = new MethodResult<bool>();

            #region Validation

            var course = _courseRepository.Queryable.Where(e => e.Id == request.Id).Include(e => e.CourseUnitMockTests).Include(e => e.CourseTeachers).FirstOrDefault();
            if (course == null)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddErrorMessage(
                    nameof(EnumCourseErrorCode.C01V));
                return methodResult;
            }

            if (course.Status != EnumCourseStatus.New)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddErrorMessage(
                nameof(EnumCourseErrorCode.C03V));
                return methodResult;
            }

            #endregion Validation

            await _courseRepository.ExecuteTransactionAsync(async () =>
            {
                var result = await _courseRepository.DeleteAsync(course);
                await _courseRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = result;
                return methodResult;
            });

            return methodResult;
        }
    }
}
