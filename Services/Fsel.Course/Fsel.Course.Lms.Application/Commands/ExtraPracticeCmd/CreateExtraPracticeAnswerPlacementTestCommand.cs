// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.ExtraPracticeCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.ExtraPracticeAnswers;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using MediatR;
    using Fsel.Course.Lms.Application.Services.UserServices;

    public class CreateExtraPracticeAnswerPlacementTestCommand : CreateExtraPracticeAnswerPlacementTestCommandModel, IRequest<MethodResult<ExtraPracticeModel>>
    {
    }

    public class CreateExtraPracticeAnswerPlacementTestCommandHandler : IRequestHandler<CreateExtraPracticeAnswerPlacementTestCommand, MethodResult<ExtraPracticeModel>>
    {
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly ISectionTimeCodeRepository _sectionTimeCodeRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly ISectionRepository _sectionRepository;
        private readonly IExtraPracticeAnswerRepository _extraPracticeAnswerRepository;
        private readonly AnswerTypeConverter _answerTypeConverter;
        private readonly IQuestionRepository _questionRepository;
        private readonly IExtraPracticeResultRepository _extraPracticeResultRepository;
        private readonly IExtraPracticeExerciseResultRepository _extraPracticeExerciseResultRepository;

        public CreateExtraPracticeAnswerPlacementTestCommandHandler(AuthContext authContext
            , IUserService userService
            , ISectionTimeCodeRepository sectionTimeCodeRepository
            , IMapper mapper
            , IMediator mediator
            , ISectionRepository sectionRepository
            , IExtraPracticeAnswerRepository extraPracticeAnswerRepository
            , AnswerTypeConverter answerTypeConverter
            , IQuestionRepository questionRepository
            , IExtraPracticeResultRepository extraPracticeResultRepository
            , IExtraPracticeExerciseResultRepository extraPracticeExerciseResultRepository)
        {
            _authContext = authContext;
            _userService = userService;
            _sectionTimeCodeRepository = sectionTimeCodeRepository;
            _mapper = mapper;
            _mediator = mediator;
            _sectionRepository = sectionRepository;
            _extraPracticeAnswerRepository = extraPracticeAnswerRepository;
            _answerTypeConverter = answerTypeConverter;
            _questionRepository = questionRepository;
            _extraPracticeResultRepository = extraPracticeResultRepository;
            _extraPracticeExerciseResultRepository = extraPracticeExerciseResultRepository;
        }

        public async Task<MethodResult<ExtraPracticeModel>> Handle(CreateExtraPracticeAnswerPlacementTestCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<ExtraPracticeModel> methodResult = new MethodResult<ExtraPracticeModel>();

            #region Validate

            var student = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!student.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumMockTestErrorCode.UserNotExist), nameof(student), _authContext.CurrentUserId.ToString());
                return methodResult;
            }
            var studentId = student?.Content?.Result?.Id;
            var extraPracticeResult = await _extraPracticeResultRepository.Queryable.Include(x => x.ExtraPracticeAnswers).FirstOrDefaultAsync(x => x.Id == request.ExtraPracticeResultId && x.StudentId == studentId, cancellationToken);
            if (extraPracticeResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumExtraPracticeErrorCode.ExtraPracticeResultNotExist));
                return methodResult;
            }

            #endregion Validate

            ExtraPracticeExerciseResult? extraPracticeExerciseResult = null;
            if (request.Type == EnumExtraPracticeType.Book || request.Type == EnumExtraPracticeType.VideoEmbed)
            {
                extraPracticeExerciseResult = await _extraPracticeExerciseResultRepository.Queryable.Include(x => x.ExtraPracticeAnswers)
                    .FirstOrDefaultAsync(x => x.ExtraPracticeExerciseId == request.ExtraPracticeExerciseId && x.StudentId == studentId && x.ExtraPracticeResultId == request.ExtraPracticeResultId, cancellationToken);
                if (extraPracticeExerciseResult == null)
                {
                    extraPracticeExerciseResult = new ExtraPracticeExerciseResult
                    {
                        ExtraPracticeExerciseId = request.ExtraPracticeExerciseId ?? default,
                        ExtraPracticeResultId = request.ExtraPracticeResultId ?? default,
                        StudentId = studentId ?? default,
                        Status = EnumResultStatus.Process
                    };
                    extraPracticeExerciseResult = _extraPracticeExerciseResultRepository.Add(extraPracticeExerciseResult);
                    await _extraPracticeExerciseResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                }
            }

            #region xoa cau tra loi

            if (extraPracticeResult.Status == EnumResultStatus.Done)
            {
                foreach (var item in extraPracticeResult.ExtraPracticeAnswers)
                {
                    await _extraPracticeAnswerRepository.DeleteAsync(item);
                    await _extraPracticeAnswerRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                }
            }
            if (extraPracticeExerciseResult != null && extraPracticeExerciseResult.Status == EnumResultStatus.Done)
            {
                foreach (var item in extraPracticeExerciseResult.ExtraPracticeAnswers)
                {
                    await _extraPracticeAnswerRepository.DeleteAsync(item);
                    await _extraPracticeAnswerRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                }
            }

            #endregion xoa cau tra loi

            var extraPracticeAnswers = new List<ExtraPracticeAnswer>();
            if (request.Answers != null && request.Answers.Count != 0)
            {
                var questionIds = request.Answers.Where(x => x.QuestionId != null).Select(x => x.QuestionId ?? default).ToList();
                var questions = await GetQuestionsAsync(questionIds, request.Type);
                if ((request.Type == EnumExtraPracticeType.Book || request.Type == EnumExtraPracticeType.VideoEmbed) && extraPracticeExerciseResult != null)
                {
                    var method = await AddExtraPracticeExerciseResult(extraPracticeExerciseResult, questions, request, cancellationToken);
                    if (!method.IsOK)
                    {
                        methodResult.AddErrorBadRequest(method.ErrorMessages);
                        return methodResult;
                    }

                    if (extraPracticeExerciseResult.Status == EnumResultStatus.Done)
                    {
                        extraPracticeExerciseResult.CorrectCount = 0;
                        extraPracticeExerciseResult.Status = EnumResultStatus.Process;
                        extraPracticeExerciseResult.Percent = 0;
                    }
                    extraPracticeExerciseResult.CorrectCount += extraPracticeExerciseResult.ExtraPracticeAnswers.Sum(x => x.CorrectCount);
                    if (request.IsActive)
                    {
                        extraPracticeExerciseResult.Status = EnumResultStatus.Done;
                        extraPracticeExerciseResult.ExecuteCount += 1;
                        extraPracticeExerciseResult.Percent = 100;
                    }
                    else
                    {
                        extraPracticeExerciseResult.Status = EnumResultStatus.Process;
                    }
                }
                else if (request.Type != EnumExtraPracticeType.Articles)
                {
                    foreach (var item in request.Answers)
                    {
                        var method = await AddExtraPracticeResult(extraPracticeResult, questions, request, cancellationToken);
                        if (!method.IsOK)
                        {
                            methodResult.AddErrorBadRequest(method.ErrorMessages);
                            return methodResult;
                        }
                    }
                    if (extraPracticeResult.Status == EnumResultStatus.Done)
                    {
                        extraPracticeResult.CorrectCount = 0;
                        extraPracticeResult.Status = EnumResultStatus.Process;
                        extraPracticeResult.Percent = 0;
                    }
                    extraPracticeResult.CorrectCount = extraPracticeResult.ExtraPracticeAnswers.Sum(x => x.CorrectCount);
                    if (request.IsActive)
                    {
                        extraPracticeResult.Status = EnumResultStatus.Done;
                        extraPracticeResult.Percent = 100;
                    }
                    else
                    {
                        extraPracticeResult.Status = EnumResultStatus.Process;
                    }
                }
            }
            await _extraPracticeResultRepository.ExecuteTransactionAsync(async () =>
            {
                if (extraPracticeExerciseResult != null)
                {
                    _extraPracticeExerciseResultRepository.Update(extraPracticeExerciseResult);
                    await _extraPracticeExerciseResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    _extraPracticeResultRepository.Update(extraPracticeResult);
                    await _extraPracticeResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                }
                methodResult.Result = _mapper.Map<ExtraPracticeExerciseResultModel>(extraPracticeExerciseResult);
                methodResult.StatusCode = StatusCodes.Status201Created;
                return methodResult;
            });
            return methodResult;
        }
    }
}
