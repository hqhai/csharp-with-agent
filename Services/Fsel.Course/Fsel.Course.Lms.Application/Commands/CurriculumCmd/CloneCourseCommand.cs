// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.CurriculumCmd
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.Courses.V1i1;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common.CourseHelpers;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Courses = Domain.Entities.Course;

    public class CloneCourseCommand : IRequest<MethodResult<CourseModel>>
    {
        public Guid Id { get; set; }
    }

    public class CloneCourseCommandHandler : IRequestHandler<CloneCourseCommand, MethodResult<CourseModel>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly IVersionEntityUpdater<Courses> _versionEntityUpdater;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILevelRepository _levelRepository;

        public CloneCourseCommandHandler(ICourseRepository courseRepository, IMapper mapper, IUserService userService, IVersionEntityUpdater<Courses> versionEntityUpdater, ICategoryRepository categoryRepository, ILevelRepository levelRepository)
        {
            _courseRepository = courseRepository;
            _mapper = mapper;
            _userService = userService;
            _versionEntityUpdater = versionEntityUpdater;
            _categoryRepository = categoryRepository;
            _levelRepository = levelRepository;
        }

        public async Task<MethodResult<CourseModel>> Handle(CloneCourseCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<CourseModel> methodResult = new MethodResult<CourseModel>();

            var course = await _courseRepository.Queryable
                                               .Where(x => x.Id == request.Id)
                                               .Include(e => e.CourseModules)
                                               .Include(e => e.CourseTeachers)
                                               .FirstOrDefaultAsync(cancellationToken: cancellationToken);
            if (course == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var requestModel = _mapper.Map<UpdateCourseCommandModel>(course);

            var newVersionCourse = CourseFactory.Create(requestModel).Build();

            var priority = await _courseRepository.ReadQueryable.Where(x => x.ParentCourseId.HasValue && x.ParentCourseId == course.Id).CountAsync(cancellationToken);

            newVersionCourse.Code = course.Code + "_" + priority;
            newVersionCourse.ParentCourseId = course.Id;
            newVersionCourse.VersionStatus = EnumVersionStatus.OldVersion;
            newVersionCourse.Version = course.Version;
            newVersionCourse.Priority = priority;
            newVersionCourse.OriginalId = course.OriginalId;

            if (!newVersionCourse.IsValid())
            {
                methodResult.AddErrorBadRequest(newVersionCourse.ErrorMessages);
                return methodResult;
            }

            await _courseRepository.ExecuteTransactionAsync(async () =>
            {
                newVersionCourse = _courseRepository.Add(newVersionCourse);
                await _courseRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<CourseModel>(newVersionCourse);
                return methodResult;
            });

            return methodResult;
        }
    }
}
