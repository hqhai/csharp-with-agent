// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.CourseCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using EntityCourse = Domain.Entities.Course;

    public class CloneCourseCommand : IRequest<MethodResult<CourseModel>>
    {
        public Guid CourseId { get; set; }
    }

    public class CloneCourseCommandHandler : IRequestHandler<CloneCourseCommand, MethodResult<CourseModel>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IMapper _mapper;

        public CloneCourseCommandHandler(ICourseRepository courseRepository
            , IMapper mapper
            )
        {
            _courseRepository = courseRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<CourseModel>> Handle(CloneCourseCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<CourseModel>();

            var course = await _courseRepository.Queryable.Include(x => x.CourseUnitMockTests).Include(x => x.CourseTeachers).FirstOrDefaultAsync(x => x.Id == request.CourseId, cancellationToken);
            if (course == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var courseClone = _mapper.Map<EntityCourse>(course);
            if (courseClone.IsValid())
            {
                methodResult.AddErrorBadRequest(courseClone.ErrorMessages);
                return methodResult;
            }
            var priority = await _courseRepository.Queryable.Where(x => x.ParentCourseId.HasValue && x.ParentCourseId == course.Id).CountAsync(cancellationToken);
            courseClone.Code = course.Code + "_" + priority;
            courseClone.ParentCourseId = course.Id;
            courseClone.Priority = priority;
            await _courseRepository.ExecuteTransactionAsync(async () =>
            {
                courseClone = _courseRepository.Add(courseClone);
                await _courseRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<CourseModel>(courseClone);
                return methodResult;
            });

            return methodResult;
        }
    }
}
