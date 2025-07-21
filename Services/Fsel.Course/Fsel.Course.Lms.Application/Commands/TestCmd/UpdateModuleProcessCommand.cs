// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.TestCmd
{
    using System.Threading;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Lms.Application.Commands.LessonCmd;
    using Fsel.Course.Lms.Application.Queries.CourseQuery;
    using Fsel.Course.Lms.Application.Queries.LessonQuery.V1i1;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.Models;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Net.Http.Headers;
    using Microsoft.Extensions.Hosting;

    public class UpdateModuleProcessCommand : IRequest<MethodResult<bool>>
    {
        public string? Email { get; set; }
        public string? Type { get; set; }
        public Guid CourseId { get; set; }
        public Guid? UnitId { get; set; }
        public Guid? LessonId { get; set; }
        public Guid? ObjectId { get; set; }
    }

    public class UpdateModuleProcessCommandHandler : IRequestHandler<UpdateModuleProcessCommand, MethodResult<bool>>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AuthContext _authContext;
        private readonly IMediator _mediator;
        private readonly IVideoTimeCodeRepository _videoTimeCodeRepository;
        private readonly IVideoTimeCodeResultRepository _videoTimeCodeResultRepository;
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly IFinalTestResultRepository _finalTestResultRepository;
        private readonly IUserService _userService;
        private readonly IUnitResultRepository _unitResultRepository;
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly IHostEnvironment _environment;

        public UpdateModuleProcessCommandHandler(IHttpContextAccessor httpContextAccessor, AuthContext authContext, IMediator mediator, IVideoTimeCodeRepository videoTimeCodeRepository, IVideoTimeCodeResultRepository videoTimeCodeResultRepository, IVideoResultRepository videoResultRepository, ILessonResultRepository lessonResultRepository, IFinalTestResultRepository finalTestResultRepository, IUserService userService, IUnitResultRepository unitResultRepository, IMockTestResultRepository mockTestResultRepository, IHostEnvironment environment = null)
        {
            _httpContextAccessor = httpContextAccessor;
            _authContext = authContext;
            _mediator = mediator;
            _videoTimeCodeRepository = videoTimeCodeRepository;
            _videoTimeCodeResultRepository = videoTimeCodeResultRepository;
            _videoResultRepository = videoResultRepository;
            _lessonResultRepository = lessonResultRepository;
            _finalTestResultRepository = finalTestResultRepository;
            _userService = userService;
            _unitResultRepository = unitResultRepository;
            _mockTestResultRepository = mockTestResultRepository;
            _environment = environment;
        }

        public async Task<MethodResult<bool>> Handle(UpdateModuleProcessCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();
            //if (_environment.IsProduction())
            //{
            //    methodResult.AddError(StatusCodes.Status401Unauthorized, "Not Have Access Production");
            //    return methodResult;
            //}

            if (string.IsNullOrEmpty(request.Type))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Type));
                return methodResult;
            }
            if (string.IsNullOrEmpty(request.Email))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Type));
                return methodResult;
            }
            var studentResult = await _userService.GetStudentByEmailAsync(request.Email);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError));
                return methodResult;
            }
            var student = studentResult.Content?.Result;
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }
            var userId = student?.UserId ?? default;

            var tokenResult = await _userService.GetJWTAsync(userId);
            var authToken = tokenResult.Content?.Result;
            if (authToken != null && _httpContextAccessor.HttpContext != null)
            {
                _httpContextAccessor.HttpContext.SetHeader(HeaderNames.Authorization, "Bearer " + authToken.AccessToken);
                _authContext.CurrentUsername = authToken.FullName;
                _authContext.CurrentUserId = userId;
                _authContext.CurrentFullName = authToken.FullName;
                _authContext.Roles = authToken.Roles;
            }
            var courseResult = await _mediator.Send(new GetCourseQuery { UserId = userId }, cancellationToken);
            if (!courseResult.IsOK)
            {
                methodResult.AddErrorBadRequest(courseResult.ErrorMessages);
                return methodResult;
            }
            if (courseResult.Result?.Id != request.CourseId)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }

            switch (request.Type)
            {
                case nameof(Domain.Entities.Unit):
                    if (request.UnitId.HasValue)
                    {
                        await UpdateUnitResultAsync(request, student);
                    }
                    break;

                case nameof(Lesson):
                    await UpdateLessonResultAsync(request, student, cancellationToken);
                    break;

                case nameof(VideoTimeCode):
                    var lessonVideoResult = await UpdateStartLessonAsync(request, student, cancellationToken);
                    if (lessonVideoResult != null)
                    {
                        await UpdateVideoTimeCodeResultAsync(request, lessonVideoResult, cancellationToken);
                    }
                    break;

                case nameof(ClassForum):
                    var lessonResult = await UpdateStartLessonAsync(request, student, cancellationToken);
                    if (lessonResult != null)
                    {
                        await UpdateVideoDoneAsync(lessonResult, cancellationToken);
                    }
                    break;

                case nameof(FinalTest):
                    await UpdateFinalTestResultAsync(request, student);
                    break;

                case nameof(EnumMockTestType.SkillMockTest):
                    await UpdateMockTestResultAsync(request, student, cancellationToken);
                    break;

                case nameof(EnumMockTestType.FullMockTest):
                    await UpdateMockTestResultAsync(request, student, cancellationToken);
                    break;
            }
            methodResult.Result = true;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task CreateModuleResultForUnitAsync(UpdateModuleProcessCommand request, StudentModel student, CancellationToken cancellationToken)
        {
            await UpdateUnitResultAsync(request, student);
            if (request.UnitId.HasValue)
            {
                await _mediator.Send(new GetLessonsQuery { CourseId = request.CourseId, UnitId = request.UnitId.Value, UserId = student?.UserId ?? default }, cancellationToken);
            }
        }

        private async Task UpdateVideoTimeCodeResultAsync(UpdateModuleProcessCommand request, LessonResult lessonResult, CancellationToken cancellationToken)
        {
            if (!request.ObjectId.HasValue)
            {
                return;
            }
            var videoResult = await _videoResultRepository.Queryable.FirstOrDefaultAsync(x => x.LessonResultId == lessonResult.Id, cancellationToken);
            var videoTimeCode = await _videoTimeCodeRepository.GetByIdAsync(request.ObjectId.Value);

            if (videoResult == null || videoTimeCode == null)
            {
                return;
            }
            videoResult.CurrentVideoTimeCodeId = videoTimeCode.Id;
            await _videoResultRepository.BulkUpdateList(new List<VideoResult>() { videoResult }, bulk =>
            {
                bulk.ColumnInputExpression = entity => new { entity.CurrentVideoTimeCodeId };
            });
        }

        private async Task UpdateVideoDoneAsync(LessonResult lessonResult, CancellationToken cancellationToken)
        {
            var videoResult = await _videoResultRepository.Queryable.FirstOrDefaultAsync(x => x.LessonResultId == lessonResult.Id, cancellationToken);
            if (videoResult != null)
            {
                videoResult.Status = EnumResultStatus.Done;
                await _videoResultRepository.BulkUpdateList(new List<VideoResult>() { videoResult }, bulk =>
                {
                    bulk.ColumnInputExpression = entity => new { entity.Status };
                });
            }
        }

        private async Task<LessonResult?> UpdateStartLessonAsync(UpdateModuleProcessCommand request, StudentModel student, CancellationToken cancellationToken)
        {
            LessonResult? lessonResult = default;
            if (request.UnitId.HasValue)
            {
                lessonResult = await UpdateLessonResultAsync(request, student, cancellationToken);
                if (lessonResult != null && lessonResult.Status == EnumResultStatus.New)
                {
                    await _mediator.Send(new StartLessonCommand
                    {
                        CourseId = request.CourseId,
                        UnitId = request.UnitId.Value,
                        LessonId = lessonResult.LessonId,
                        LessonResultId = lessonResult.Id,
                        UserId = student?.UserId ?? default
                    }, cancellationToken);
                }
            }
            return lessonResult;
        }

        private async Task<LessonResult?> UpdateLessonResultAsync(UpdateModuleProcessCommand request, StudentModel student, CancellationToken cancellationToken)
        {
            await CreateModuleResultForUnitAsync(request, student, cancellationToken);
            var lessonResult = await _lessonResultRepository.Queryable.FirstOrDefaultAsync(x => x.CourseId == request.CourseId && x.UnitId == request.UnitId && x.LessonId == request.LessonId && x.StudentId == student.Id, cancellationToken);
            if (lessonResult != null && lessonResult.Status == EnumResultStatus.Unfinished)
            {
                lessonResult.Status = EnumResultStatus.New;
                await _lessonResultRepository.BulkUpdateList(new List<LessonResult>() { lessonResult }, bulk =>
                {
                    bulk.ColumnInputExpression = entity => new { entity.Status };
                });
            }
            return lessonResult;
        }

        private async Task UpdateUnitResultAsync(UpdateModuleProcessCommand request, StudentModel student)
        {
            var unitResult = await _unitResultRepository.Queryable.FirstOrDefaultAsync(x => x.CourseId == request.CourseId && x.UnitId == request.UnitId && x.StudentId == student.Id);
            if (unitResult != null && unitResult.Status == EnumResultStatus.Unfinished)
            {
                unitResult.Status = EnumResultStatus.New;
                await _unitResultRepository.BulkUpdateList(new List<UnitResult>() { unitResult }, bulk =>
                {
                    bulk.ColumnInputExpression = entity => new { entity.Status };
                });
            }
        }

        private async Task UpdateFinalTestResultAsync(UpdateModuleProcessCommand request, StudentModel student)
        {
            var finalTestResult = await _finalTestResultRepository.Queryable.FirstOrDefaultAsync(x => x.CourseId == request.CourseId && x.FinalTestId == request.ObjectId && x.StudentId == student.Id);
            if (finalTestResult != null && finalTestResult.Status == EnumResultStatus.Unfinished)
            {
                finalTestResult.Status = EnumResultStatus.New;
                await _finalTestResultRepository.BulkUpdateList(new List<FinalTestResult>() { finalTestResult }, bulk =>
                {
                    bulk.ColumnInputExpression = entity => new { entity.Status };
                });
            }
        }

        private async Task UpdateMockTestResultAsync(UpdateModuleProcessCommand request, StudentModel student, CancellationToken cancellationToken)
        {
            if (request.UnitId.HasValue)
            {
                await CreateModuleResultForUnitAsync(request, student, cancellationToken);
            }
            var mockTestResult = await _mockTestResultRepository.Queryable.Where(x => !request.UnitId.HasValue || x.UnitId == request.UnitId)
                .FirstOrDefaultAsync(x => x.CourseId == request.CourseId && x.MockTestId == request.ObjectId && x.StudentId == student.Id, cancellationToken);
            if (mockTestResult != null && mockTestResult.Status == EnumResultStatus.Unfinished)
            {
                mockTestResult.Status = EnumResultStatus.New;
                await _mockTestResultRepository.BulkUpdateList(new List<MockTestResult>() { mockTestResult }, bulk =>
                {
                    bulk.ColumnInputExpression = entity => new { entity.Status };
                });
            }
        }
    }
}
