// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ExtraPracticeQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetQuestionByExtraPracticeQuery : IRequest<MethodResult<ExerciseModel>>
    {
        public Guid ExerciseId { get; set; }
        public Guid ExtraPracticeResultId { get; set; }
    }

    public class GetQuestionByExtraPracticeQueryHandler : IRequestHandler<GetQuestionByExtraPracticeQuery, MethodResult<ExerciseModel>>
    {
        private readonly IExerciseRepository _exerciseRepository;
        private readonly QuestionTypeConverter _questionTypeConverter;
        private readonly IMapper _mapper;
        private readonly IExtraPracticeResultRepository _extraPracticeResultRepository;

        public GetQuestionByExtraPracticeQueryHandler(IExerciseRepository exerciseRepository, QuestionTypeConverter questionTypeConverter, IMapper mapper, IExtraPracticeResultRepository extraPracticeResultRepository)
        {
            _exerciseRepository = exerciseRepository;
            _questionTypeConverter = questionTypeConverter;
            _mapper = mapper;
            _extraPracticeResultRepository = extraPracticeResultRepository;
        }

        public async Task<MethodResult<ExerciseModel>> Handle(GetQuestionByExtraPracticeQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var exerciseModel = new ExerciseModel();
            MethodResult<ExerciseModel> methodResult = new MethodResult<ExerciseModel>();

            var extraPracticeResult = await _extraPracticeResultRepository.GetByIdAsync(request.ExtraPracticeResultId);
            if (extraPracticeResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumExtraPracticeErrorCode.ExtraPracticeResultNotExist));
                return methodResult;
            }

            var exercise = await _exerciseRepository.Queryable.Include(x => x.ExtraPracticeExercises)
                                                            .Include(x => x.ExerciseQuestions)
                                                            .ThenInclude(x => x.Question)
                                                            .ThenInclude(x => x!.ExtraPracticeAnswers)
                                                            .FirstOrDefaultAsync(x => x.Id == request.ExerciseId, cancellationToken);
            if (exercise != null)
            {
                exerciseModel = new ExerciseModel
                {
                    Id = exercise!.Id,
                    MediaPost = exercise.MediaPost,
                    CourseSkill = exercise.CourseSkill,
                    Questions = exercise.ExerciseQuestions.Select(x => x.Question).OrderBy(x => x!.CreatedDate).Select(m => new QuestionModel()
                    {
                        Id = m!.Id,
                        QuestionType = m!.QuestionType,
                        CorrectTotal = m!.CorrectTotal,
                        Explanation = m!.Explanation,
                        Ungraded = m!.Ungraded,
                        Config = _questionTypeConverter.QuestionTypeConverterObject(m!.Config, m!.QuestionType, isDisableAnswers: true).Item1,
                        ResultAnswer = _mapper.Map<AnswerModel>(m.ExtraPracticeAnswers.FirstOrDefault(x => x.ExtraPracticeResultId == extraPracticeResult.Id))
                    }).ToList(),
                };
            }

            methodResult.Result = exerciseModel;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
