// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.CourseCmd
{
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
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
            MethodResult<bool> methodResult = new MethodResult<bool>();

            #region Validation

            var course = await _courseRepository.Queryable
                            .Include(e => e.CourseTeachers)
                            .Where(e => e.Id == request.Id)
                            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

            if (course == null)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddErrorMessage(
                    nameof(EnumCourseErrorCode.C01V));
                methodResult.Result = false;
                return methodResult;
            }

            if (course.Status != EnumCourseStatus.New)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddErrorMessage(
                    nameof(EnumCourseErrorCode.C03V));
                methodResult.Result = false;
                return methodResult;
            }

            #endregion Validation

            var teacherIds = course.CourseTeachers.Select(x => x.TeacherId).ToList();
            var courses = await _courseRepository.Queryable
                                .Include(e => e.CourseTeachers)
                                .Where(e => e.CourseLevel == course.CourseLevel &&
                                            e.Status == EnumCourseStatus.Active &&
                                            e.CourseTeachers.Count == teacherIds.Count &&
                                            e.CourseTeachers.All(x => teacherIds.Contains(x.TeacherId)))
                                .ToListAsync(cancellationToken: cancellationToken);

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
