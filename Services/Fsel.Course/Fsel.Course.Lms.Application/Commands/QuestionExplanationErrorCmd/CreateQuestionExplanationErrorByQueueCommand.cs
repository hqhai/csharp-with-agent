// Copyright (c) Atlantic. All rights reserved.

using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Lms.Application.Commands.QuestionExplanationErrorCmd
{
    using System.Linq;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.QuestionExplanationErrors;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;

    public class CreateQuestionExplanationErrorByQueueCommand : CreateQuestionExplanationErrorCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class CreateQuestionExplanationErrorByQueueCommandHandler : IRequestHandler<CreateQuestionExplanationErrorByQueueCommand, MethodResult<bool>>
    {
        private readonly IQuestionRepository _questionRepository;
        private readonly ISystemService _systemService;
        private readonly IExerciseQuestionRepository _exerciseQuestionRepository;
        private readonly IExerciseRepository _exerciseRepository;
        private readonly ITimeCodeExerciseRepository _timeCodeExerciseRepository;
        private readonly IVideoTimeCodeRepository _videoTimeCodeRepository;
        private readonly IVideoRepository _videoRepository;
        private readonly IQuestionExplanationLogRepository _questionExplanationLogRepository;
        private readonly IHomeWorkQuestionRepository _homeWorkQuestionRepository;
        private readonly IHomeWorkRepository _homeWorkRepository;

        public CreateQuestionExplanationErrorByQueueCommandHandler(
            IQuestionRepository questionRepository,
            ISystemService systemService,
            IExerciseQuestionRepository exerciseQuestionRepository,
            IExerciseRepository exerciseRepository,
            ITimeCodeExerciseRepository timeCodeExerciseRepository,
            IVideoTimeCodeRepository videoTimeCodeRepository,
            IVideoRepository videoRepository,
            IQuestionExplanationLogRepository questionExplanationLogRepository,
            IHomeWorkQuestionRepository homeWorkQuestionRepository,
            IHomeWorkRepository homeWorkRepository)
        {
            _questionRepository = questionRepository;
            _systemService = systemService;
            _exerciseQuestionRepository = exerciseQuestionRepository;
            _exerciseRepository = exerciseRepository;
            _timeCodeExerciseRepository = timeCodeExerciseRepository;
            _videoTimeCodeRepository = videoTimeCodeRepository;
            _videoRepository = videoRepository;
            _questionExplanationLogRepository = questionExplanationLogRepository;
            _homeWorkQuestionRepository = homeWorkQuestionRepository;
            _homeWorkRepository = homeWorkRepository;
        }

        public async Task<MethodResult<bool>> Handle(CreateQuestionExplanationErrorByQueueCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();
            await AddGoogleSheetErrorReportAsync(request).ConfigureAwait(false);
            return methodResult;
        }

        private async Task<AddErrorReportExplanationQuestionModel?> GetReportExplanationQuestionModel(CreateQuestionExplanationErrorByQueueCommand request)
        {
            var question = new AddErrorReportExplanationQuestionModel();
            if (request.ExplanationType == EnumFeatureExplanationType.Video)
            {
                question = await (from baseQ in _questionRepository.Queryable
                                  join eq in _exerciseQuestionRepository.Queryable on baseQ.Id equals eq.QuestionId
                                  join e in _exerciseRepository.Queryable on eq.ExerciseId equals e.Id
                                  join te in _timeCodeExerciseRepository.Queryable on e.Id equals te.ExerciseId
                                  join vtc in _videoTimeCodeRepository.Queryable on te.VideoTimeCodeId equals vtc.Id
                                  join v in _videoRepository.Queryable on vtc.VideoId equals v.Id
                                  where baseQ.Id == request.QuestionId
                                  select new AddErrorReportExplanationQuestionModel
                                  {
                                      Id = v.Id,
                                      ExplanationType = request.ExplanationType,
                                      DisplayTime = vtc.DisplayTime,
                                      QuestionId = request.QuestionId,
                                      CourseLevel = v.CourseLevel,
                                      Config = baseQ.Config,
                                      Explanation = baseQ.Explanation,
                                      QuestionType = baseQ.QuestionType,
                                      Feedback = request.Feedback ?? request.FeedbackExplanation.ToString(),
                                  }).FirstOrDefaultAsync();
            }
            else if (request.ExplanationType == EnumFeatureExplanationType.HomeWork)
            {
                question = await (from baseQ in _questionRepository.Queryable
                                  join hq in _homeWorkQuestionRepository.Queryable on baseQ.Id equals hq.QuestionId
                                  join h in _homeWorkRepository.Queryable on hq.HomeWorkId equals h.Id
                                  where baseQ.Id == request.QuestionId
                                  select new AddErrorReportExplanationQuestionModel
                                  {
                                      Id = h.Id,
                                      ExplanationType = request.ExplanationType,
                                      DisplayTime = default,
                                      QuestionId = request.QuestionId,
                                      CourseLevel = h.CourseLevel,
                                      Config = baseQ.Config,
                                      Explanation = baseQ.Explanation,
                                      QuestionType = baseQ.QuestionType,
                                      Feedback = request.Feedback ?? request.FeedbackExplanation.ToString(),
                                  }).FirstOrDefaultAsync();
            }
            return question;
        }

        private async Task AddGoogleSheetErrorReportAsync(CreateQuestionExplanationErrorByQueueCommand request)
        {
            var question = await GetReportExplanationQuestionModel(request);
            if (question == null)
            {
                return;
            }
            var logExplanations = await _questionExplanationLogRepository.Queryable.Where(x => x.QuestionId == question.QuestionId).OrderBy(x => x.CreatedDate).ToListAsync();
            question.PromptRequest = string.Join("\n", logExplanations.Select(x => x.PromptRequest).ToList());
            question.PromptResponse = string.Join("\n", logExplanations.Select(x => x.PromptResponse).ToList());
            await _systemService.AddErrorReportExplanationQuestionToGoogleSheet(question).ConfigureAwait(false);
        }
    }
}
