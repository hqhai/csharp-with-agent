// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Commands.StudentReviewCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Interaction.Application.Queues.Publishers;
    using Fsel.Interaction.Application.Services.CourseServices;
    using Fsel.Interaction.Application.Services.TrainingServices;
    using Fsel.Interaction.Application.Services.UserServices;
    using Fsel.Interaction.Domain.Entities;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.CommandModels.StudentReviews;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SaveStudentReviewCommand : SaveStudentReviewCommandModel, IRequest<MethodResult<StudentReviewModel>>
    {
    }

    public class SaveStudentReviewCommandHandler : IRequestHandler<SaveStudentReviewCommand, MethodResult<StudentReviewModel>>
    {
        private readonly IStudentReviewRepository _studentReviewRepository;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly ITrainingService _trainingService;
        private readonly ICourseService _courseService;
        private readonly IMapper _mapper;
        private readonly QuestBoardPublisher _questBoardPublisher;

        public SaveStudentReviewCommandHandler(IStudentReviewRepository studentReviewRepository, AuthContext authContext
            , IUserService userService
            , ITrainingService trainingService
            , ICourseService courseService
            , IMapper mapper
            , QuestBoardPublisher questBoardPublisher)
        {
            _studentReviewRepository = studentReviewRepository;
            _authContext = authContext;
            _userService = userService;
            _trainingService = trainingService;
            _courseService = courseService;
            _mapper = mapper;
            _questBoardPublisher = questBoardPublisher;
        }

        public async Task<MethodResult<StudentReviewModel>> Handle(SaveStudentReviewCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<StudentReviewModel> methodResult = new MethodResult<StudentReviewModel>();
            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError));
                return methodResult;
            }
            var studentId = studentResult.Content?.Result?.Id;
            Guid? courseId = null;
            if (request.ReviewType == EnumReviewType.Course)
            {
                var classResult = await _trainingService.GetClassByStudentId(studentId ?? default);
                if (!classResult.IsSuccessStatusCode)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallTrainingServiceError));
                    return methodResult;
                }
                courseId = classResult.Content?.Result?.CourseId;

                var courseResult = await _courseService.GetCourseByIdAsync(courseId ?? default);
                if (!courseResult.IsSuccessStatusCode)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallCourseServiceError));
                    return methodResult;
                }

                if (courseResult == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(courseResult));
                    return methodResult;
                }
            }

            await _studentReviewRepository.ExecuteTransactionAsync(async () =>
            {
                var studentReview = await _studentReviewRepository.Queryable.Include(x => x.StudentReviewDetails).FirstOrDefaultAsync(x => x.StudentId == studentId && x.ReviewType == request.ReviewType, cancellationToken);

                var studentReviewDetails = _mapper.Map<List<StudentReviewDetail>>(request.StudentReviewDetails);
                foreach (var studentReviewDetail in studentReviewDetails)
                {
                    if (!studentReviewDetail.IsValid())
                    {
                        methodResult.AddErrorBadRequest(studentReviewDetail.ErrorMessages);
                        return methodResult;
                    }
                }
                if (studentReview == null)
                {
                    studentReview = _mapper.Map<StudentReview>(request);
                    studentReview.StudentId = studentId ?? default;
                    studentReview.CourseId = courseId ?? null;
                    if (!studentReview.IsValid())
                    {
                        methodResult.AddErrorBadRequest(studentReview.ErrorMessages);
                        return methodResult;
                    }
                    _studentReviewRepository.Add(studentReview);

                    await DoQuestBoard(studentId, cancellationToken);
                    methodResult.StatusCode = StatusCodes.Status201Created;
                }
                else
                {
                    if (request.ReviewType == EnumReviewType.Platform)
                    {
                        var isCheckPlatform = request.Id.HasValue && studentReview?.Id == request.Id;
                        if (!isCheckPlatform && request.Id.HasValue)
                        {
                            methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(isCheckPlatform));
                            return methodResult;
                        }
                        else if (studentReview != null && !request.Id.HasValue)
                        {
                            methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentReview));
                            return methodResult;
                        }
                    }
                    else
                    {
                        var isCheckCourse = courseId.HasValue && studentReview?.CourseId == courseId && request.Id.HasValue && studentReview?.Id == request.Id;
                        if (!isCheckCourse && request.Id.HasValue)
                        {
                            methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(isCheckCourse));
                            return methodResult;
                        }
                        else if (studentReview != null && !request.Id.HasValue)
                        {
                            methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentReview));
                            return methodResult;
                        }

                    }
                    _mapper.Map(request, studentReview);
                    if (!studentReview!.IsValid())
                    {
                        methodResult.AddErrorBadRequest(studentReview.ErrorMessages);
                        return methodResult;
                    }
                    _studentReviewRepository.Update(studentReview);
                    methodResult.StatusCode = StatusCodes.Status200OK;
                }
                await _studentReviewRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.Result = _mapper.Map<StudentReviewModel>(studentReview);
                return methodResult;
            });
            return methodResult;
        }
        public async Task DoQuestBoard(Guid? studentId, CancellationToken cancellationToken)
        {
            var classResult = await _trainingService.GetClassByStudentId(studentId ?? default);
            var courseId = classResult.Content?.Result?.CourseId;
            if (studentId != null && courseId != null)
            {
                IList<EnumQuestBoardCategory> categories = new List<EnumQuestBoardCategory>() { EnumQuestBoardCategory.RateAndComment };
                await _questBoardPublisher.Publish(new QuestBoardQueueModel
                {
                    StudentId = (Guid)studentId,
                    Categories = categories,
                    AchievedPoint = ValueSettings.QuestBoardPoint.Achieved_Point,
                    CourseId = (Guid)courseId
                }, cancellationToken);
            }

        }
    }
}
