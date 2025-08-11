// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.CourseCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using EntityCourse = Domain.Entities.Course;

    public class CloneCourseCommand : IRequest<MethodResult<CourseModel>>
    {
        public Guid CourseId { get; set; }
    }

    public class CloneCourseCommandHandler : IRequestHandler<CloneCourseCommand, MethodResult<CourseModel>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly ILogger<CloneCourseCommand> _logger;
        private readonly IMapper _mapper;

        public CloneCourseCommandHandler(ICourseRepository courseRepository
            , ILogger<CloneCourseCommand> logger
            , IMapper mapper)
        {
            _courseRepository = courseRepository;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<MethodResult<CourseModel>> Handle(CloneCourseCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<CourseModel>();

            var course = await _courseRepository.Queryable.Include(x => x.CourseUnitMockTests).Include(x => x.CourseTeachers).FirstOrDefaultAsync(x => x.Id == request.CourseId, cancellationToken);
            if (course != null && course.Status != EnumCourseStatus.Active)
            {
                course = await _courseRepository.Queryable.Include(x => x.CourseUnitMockTests).Include(x => x.CourseTeachers).Where(x => x.CourseLevel == course.CourseLevel && x.Status == EnumCourseStatus.Active && !x.ParentCourseId.HasValue)
                    .OrderByDescending(x => x.CreatedDate)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            if (course == null || course.ParentCourseId.HasValue)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }
            var courseClone = _mapper.Map<EntityCourse>(course);
            if (!courseClone.IsValid())
            {
                methodResult.AddErrorBadRequest(courseClone.ErrorMessages);
                return methodResult;
            }
            courseClone.CourseTeachers = course.CourseTeachers.Select(x =>
            {
                var courseTeacher = _mapper.Map<CourseTeacher>(x);
                courseTeacher.CourseId = courseClone.Id;
                return courseTeacher;
            }).ToList();

            courseClone.CourseUnitMockTests = course.CourseUnitMockTests.OrderBy(x => x.DisplayOrder).Select(x =>
            {
                var courseUnitMockTest = _mapper.Map<CourseUnitMockTest>(x);
                courseUnitMockTest.CourseId = courseClone.Id;
                return courseUnitMockTest;
            }).ToList();

            var priority = await _courseRepository.Queryable.Where(x => x.ParentCourseId.HasValue && x.ParentCourseId == course.Id).CountAsync(cancellationToken);
            courseClone.Status = EnumCourseStatus.Clone;
            courseClone.Code = course.Code + "_" + priority;
            courseClone.ParentCourseId = course.Id;
            courseClone.Priority = priority;
            await _courseRepository.ExecuteTransactionAsync(async () =>
            {
                try
                {
                    await _courseRepository.BulkMergeAsync(new List<Course> { courseClone }, bulk =>
                    {
                        bulk.ColumnPrimaryKeyExpression = entity => new { entity.ParentCourseId, entity.Priority, entity.IsDeleted };
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Log Duplicate Course : {ex.Message}");
                    courseClone = await _courseRepository.Queryable.Where(x => x.ParentCourseId.HasValue && x.ParentCourseId == course.Id && x.Priority == priority)
                                                                   .FirstOrDefaultAsync(cancellationToken);
                }

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<CourseModel>(courseClone);
                return methodResult;
            });

            return methodResult;
        }
    }
}
