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
        private readonly IMapper _mapper;

        public UpdateActiveStatusCommandHandler(
            ICourseRepository courseRepository,
            IMapper mapper)
        {
            _courseRepository = courseRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<bool>> Handle(ActiveCourseCommand request, CancellationToken cancellationToken)
        {
            MethodResult<bool> methodResult = new MethodResult<bool>();

            #region Validation

            var course = _courseRepository.Queryable.Where(e => e.Id == request.Id).Include(e => e.CourseTeachers).FirstOrDefault();
            if (course == null)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddErrorMessage(
                    nameof(EnumCourseErrorCode.C01V));
                methodResult.Result = false;
                return methodResult;
            }

            var courses = _courseRepository.Queryable
                                .Where(e => e.CourseLevel == course.CourseLevel && e.Status == EnumCourseStatus.Active)
                                .Include(e => e.CourseTeachers).ToList();
            if (courses == null)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddErrorMessage(
                    nameof(EnumCourseErrorCode.C01V));
                methodResult.Result = false;
                return methodResult;
            }

            List<Guid> courseIds = new List<Guid>();
            course.CourseTeachers.ForEach(z => courseIds.Add(z.TeacherId));

            courses.ForEach(x =>
            {
                if (x.CourseTeachers.Count != course.CourseTeachers.Count)
                {
                    courses.Remove(x);
                }
                else if (courseIds.Any(id => !x.CourseTeachers.Any(f => f.TeacherId == id)))
                {
                    courses.Remove(x);
                }
            });

            if (courses.Count > 0)
            {
                course.Status = EnumCourseStatus.Active;
                courses.ForEach(x => x.Status = EnumCourseStatus.InActive);
            }
            else
            {
                course.Status = EnumCourseStatus.Active;
            }

            #endregion Validation

            await _courseRepository.ExecuteTransactionAsync(async () =>
            {
                courses.ForEach(x => _courseRepository.Update(x));
                course = _courseRepository.Update(course);
                await _courseRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = true;
                return methodResult;
            });
            return methodResult;
        }
    }
}
