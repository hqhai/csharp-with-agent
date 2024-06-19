// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.CourseCmd
{
    using System.Linq;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class ActiveCourseCommand : IRequest<MethodResult<bool>>
    {
        public Guid Id { get; set; }
    }

    public class ActiveCourseCommandHandler : IRequestHandler<ActiveCourseCommand, MethodResult<bool>>
    {
        private readonly ICourseRepository _courseRepository;

        public ActiveCourseCommandHandler(ICourseRepository courseRepository)
        {
            _courseRepository = courseRepository;
        }

        public async Task<MethodResult<bool>> Handle(ActiveCourseCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            MethodResult<bool> methodResult = new MethodResult<bool>();

            #region Validation

            var course = await _courseRepository.Queryable
                            .Include(e => e.CourseTeachers.Where(n => !n.IsDeleted))
                            .Where(e => e.Id == request.Id)
                            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

            if (course == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(course));
                return methodResult;
            }

            if (course.Status == EnumCourseStatus.Active)
            {
                methodResult.AddErrorBadRequest(nameof(EnumCourseErrorCode.CourseIsActiveState), nameof(course.Status), course.Status);
                return methodResult;
            }
            var teacherIds = course.CourseTeachers.Select(x => x.TeacherId).ToList();
            var courses = await _courseRepository.Queryable
                                .Include(e => e.CourseTeachers.Where(n => !n.IsDeleted))
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
