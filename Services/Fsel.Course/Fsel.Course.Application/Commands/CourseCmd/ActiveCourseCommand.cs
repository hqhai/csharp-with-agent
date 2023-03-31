// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.CourseCmd
{
    using System.Linq;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class ActiveCourseCommand : IRequest<MethodResult<bool>>
    {
        public Guid Id { get; set; }
    }

    public class UpdateActiveStatusCommandHandler : IRequestHandler<ActiveCourseCommand, MethodResult<bool>>
    {
        private readonly ICourseRepository _courseRepository;

        public UpdateActiveStatusCommandHandler(ICourseRepository courseRepository)
        {
            _courseRepository = courseRepository;
        }

        public async Task<MethodResult<bool>> Handle(ActiveCourseCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            MethodResult<bool> methodResult = new MethodResult<bool>();

            #region Validation

            var course = await _courseRepository.Queryable
                            .Include(e => e.CourseTeachers)
                            .Where(e => e.Id == request.Id)
                            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

            if (course == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumCourseErrorCode.CourseNotExist), nameof(request.Id), request.Id);
                return methodResult;
            }

            if (course.Status != EnumCourseStatus.New)
            {
                methodResult.AddErrorBadRequest(nameof(EnumCourseErrorCode.CourseNotInNewState), nameof(course.Status), course.Status);
                return methodResult;
            }
            var teacherIds = course.CourseTeachers.Select(x => x.TeacherId).ToList();
            var courses = await _courseRepository.Queryable
                                .Include(e => e.CourseTeachers)
                                .Where(e => e.CourseLevel == course.CourseLevel &&
                                            e.Status == EnumCourseStatus.Active &&
                                            e.CourseTeachers.Count == teacherIds.Count &&
                                            e.CourseTeachers.All(x => teacherIds.Contains(x.TeacherId)))
                                .ToListAsync(cancellationToken: cancellationToken);

            #endregion Validation

            await _courseRepository.ExecuteTransactionAsync(async () =>
            {
                course.Status = EnumCourseStatus.Active;
                course = _courseRepository.Update(course);

                foreach (var item in courses)
                {
                    item.Status = EnumCourseStatus.InActive;
                    _courseRepository.Update(item);
                }

                await _courseRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = true;
                return methodResult;
            });
            return methodResult;
        }
    }
}
