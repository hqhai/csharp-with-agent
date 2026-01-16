// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.CourseCmd
{
    using AutoMapper;
    using Common.Enums;
    using Core.Base;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Org.BouncyCastle.Ocsp;
    using Services.UserServices;
    using Shared.Enums.ErrorCodes;

    public class ChooseCourseByLevelCommand : IRequest<MethodResult<CourseModel>>
    {
        public Guid LevelId { get; set; }

        public Guid ProgramId { get; set; }
    }

    public class ChooseCourseByLevelCommandHandler : IRequestHandler<ChooseCourseByLevelCommand, MethodResult<CourseModel>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly ILogger<CloneCourseCommand> _logger;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly AuthContext _authContext;
        private readonly ICourseResultRepository _courseResultRepository;

        public ChooseCourseByLevelCommandHandler(ICourseRepository courseRepository,
            ICourseResultRepository courseResultRepository,
            AuthContext authContext,
            ILogger<CloneCourseCommand> logger,
            IUserService userService,
            IMapper mapper)
        {
            _courseRepository = courseRepository;
            _courseResultRepository = courseResultRepository;
            _logger = logger;
            _userService = userService;
            _authContext = authContext;
            _mapper = mapper;
        }

        public async Task<MethodResult<CourseModel>> Handle(ChooseCourseByLevelCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<CourseModel>();
            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResult));
                return methodResult;
            }

            var student = studentResult?.Content?.Result;
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }

            var courseResult = await _courseResultRepository.ReadQueryable
                .FirstOrDefaultAsync(x => x.StudentId == student.Id, cancellationToken);
            if (courseResult != null)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddErrorBadRequest($"StudentId: {student.Id} already has a course assigned.");
                _logger.LogWarning("StudentId: {StudentId} already has a course assigned.", student.Id);
                return methodResult;
            }

            var courses = await _courseRepository.ReadQueryable
                .Where(x => x.ProgramId == request.ProgramId
                            && x.LevelId == request.LevelId
                            && !x.IsArchive
                            && x.VersionStatus == EnumVersionStatus.LastVersion
                            && x.Status == EnumCourseStatus.Active)
                .ToListAsync(cancellationToken);

            var random = new Random();
            var course = courses.OrderBy(x => random.Next()).FirstOrDefault();

            if (course == null)
            {
                methodResult.StatusCode = StatusCodes.Status404NotFound;
                methodResult.AddErrorBadRequest($"No course found for ProgramId: {request.ProgramId} and LevelId: {request.LevelId}");
                _logger.LogWarning("No course found for ProgramId: {ProgramId} and LevelId: {LevelId}", request.ProgramId, request.LevelId);
                return methodResult;
            }

            await _userService.UpdateCourseToStudentAsync(course.Id);
            methodResult.Result = new CourseModel { Id = course.Id, Name = course.Name, LevelId = course.LevelId, ProgramId = course.ProgramId };
            return methodResult;
        }
    }
}
