// Copyright (c) Atlantic. All rights reserved.

using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Lms.Application.Commands.QuestionExplanationErrorCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.QuestionExplanationErrors;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class CreateQuestionExplanationErrorCommand : CreateQuestionExplanationErrorCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class CreateQuestionExplanationErrorCommandHandler : IRequestHandler<CreateQuestionExplanationErrorCommand, MethodResult<bool>>
    {
        private readonly IMapper _mapper;
        private readonly IQuestionRepository _questionRepository;
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly IQuestionExplanationErrorRepository _questionExplanationErrorRepository;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly ISystemService _systemService;
        private readonly IExerciseQuestionRepository _exerciseQuestionRepository;
        private readonly IExerciseRepository _exerciseRepository;
        private readonly ITimeCodeExerciseRepository _timeCodeExerciseRepository;
        private readonly IVideoTimeCodeRepository _videoTimeCodeRepository;
        private readonly IVideoRepository _videoRepository;

        public CreateQuestionExplanationErrorCommandHandler(IMapper mapper,
            IQuestionRepository questionRepository,
            IVideoResultRepository videoResultRepository,
            IQuestionExplanationErrorRepository questionExplanationErrorRepository,
            AuthContext authContext,
            IUserService userService,
            ISystemService systemService,
            IExerciseQuestionRepository exerciseQuestionRepository,
            IExerciseRepository exerciseRepository,
            ITimeCodeExerciseRepository timeCodeExerciseRepository,
            IVideoTimeCodeRepository videoTimeCodeRepository,
            IVideoRepository videoRepository)
        {
            _mapper = mapper;
            _questionRepository = questionRepository;
            _videoResultRepository = videoResultRepository;
            _questionExplanationErrorRepository = questionExplanationErrorRepository;
            _authContext = authContext;
            _userService = userService;
            _systemService = systemService;
            _exerciseQuestionRepository = exerciseQuestionRepository;
            _exerciseRepository = exerciseRepository;
            _timeCodeExerciseRepository = timeCodeExerciseRepository;
            _videoTimeCodeRepository = videoTimeCodeRepository;
            _videoRepository = videoRepository;
        }

        public async Task<MethodResult<bool>> Handle(CreateQuestionExplanationErrorCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();
            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResult));
                return methodResult;
            }
            var student = studentResult.Content?.Result;
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }

            var question = await _questionRepository.GetByIdAsync(request.QuestionId);
            if (question == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(question));
                return methodResult;
            }

            var videoResult = await _videoResultRepository.GetByIdAsync(request.VideoResultId);
            if (videoResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(videoResult));
                return methodResult;
            }
            var questionExplanationError = _mapper.Map<QuestionExplanationError>(request);
            questionExplanationError.StudentId = student.Id;
            if (!questionExplanationError.IsValid())
            {
                methodResult.AddErrorBadRequest(questionExplanationError.ErrorMessages);
                return methodResult;
            }
            await _questionExplanationErrorRepository.ExecuteTransactionAsync(async () =>
            {
                questionExplanationError = _questionExplanationErrorRepository.Add(questionExplanationError);
                await _questionExplanationErrorRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = true;
                return methodResult;
            });
            await AddGoogleSheetErrorReportAsync(request).ConfigureAwait(false);
            return methodResult;
        }

        private async Task AddGoogleSheetErrorReportAsync(CreateQuestionExplanationErrorCommand request)
        {
            var question = await (from baseQ in _questionRepository.Queryable
                                  join eq in _exerciseQuestionRepository.Queryable on baseQ.Id equals eq.QuestionId
                                  join e in _exerciseRepository.Queryable on eq.ExerciseId equals e.Id
                                  join te in _timeCodeExerciseRepository.Queryable on e.Id equals te.ExerciseId
                                  join vtc in _videoTimeCodeRepository.Queryable on te.VideoTimeCodeId equals vtc.Id
                                  join v in _videoRepository.Queryable on vtc.VideoId equals v.Id
                                  where baseQ.Id == request.QuestionId
                                  select new AddErrorReportExplanationQuestionModel
                                  {
                                      VideoId = v.Id,
                                      DisplayTime = vtc.DisplayTime,
                                      QuestionId = request.QuestionId,
                                      CourseLevel = v.CourseLevel,
                                      Config = baseQ.Config,
                                      Explanation = baseQ.Explanation,
                                      QuestionType = baseQ.QuestionType,
                                      Feedback = request.Feedback,
                                  }).FirstOrDefaultAsync();
            if (question != null)
            {
                await _systemService.AddErrorReportExplanationQuestionToGoogleSheet(question).ConfigureAwait(false);
            }
        }
    }
}
