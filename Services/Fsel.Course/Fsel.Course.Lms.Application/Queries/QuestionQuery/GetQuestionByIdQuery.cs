// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.MockTestQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetQuestionByIdQuery : IRequest<MethodResult<QuestionModel>>
    {
        public Guid QuestionId { get; set; }
        public EnumTestResult Result { get; set; }
    }

    public class GetQuestionByIdQueryHandler : IRequestHandler<GetQuestionByIdQuery, MethodResult<QuestionModel>>
    {
        private readonly IQuestionRepository _questionRepository;
        private readonly QuestionTypeConverter _questionTypeConverter;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly AuthContext _authContext;

        public GetQuestionByIdQueryHandler(IQuestionRepository questionRepository, QuestionTypeConverter questionTypeConverter, IMapper mapper, IUserService userService, AuthContext authContext)
        {
            _questionRepository = questionRepository;
            _questionTypeConverter = questionTypeConverter;
            _mapper = mapper;
            _userService = userService;
            _authContext = authContext;
        }

        public async Task<MethodResult<QuestionModel>> Handle(GetQuestionByIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<QuestionModel>();
            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentResult));
                return methodResult;
            }
            var student = studentResult?.Content?.Result;
            var question = await _questionRepository.Queryable.FirstOrDefaultAsync(x => x.Id == request.QuestionId, cancellationToken);

            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = await GetQuestionAsync(request);
            return methodResult;
        }

        private async Task<QuestionModel?> GetQuestionAsync(GetQuestionByIdQuery request)
        {
            switch (request.Result)
            {
                case EnumTestResult.MockTest:
                    return await GetQuestionByMockTest(request.QuestionId);

                case EnumTestResult.FinalTest:
                    return await GetQuestionByFinalTest(request.QuestionId);

                case EnumTestResult.ExtraPratice:
                    return await GetQuestionByExtraPratice(request.QuestionId);

                default:
                    throw new InvalidOperationException();
            }
        }

        private async Task<QuestionModel?> GetQuestionByMockTest(Guid questionId)
        {
            var question = await _questionRepository.Queryable.Include(x => x.SectionQuestions).ThenInclude(x => x.MockTestAnswers).FirstOrDefaultAsync(x => x.Id == questionId);
            if (question == null)
            {
                return default;
            }
            var mockTestAnswer = question.SectionQuestions.SelectMany(x => x.MockTestAnswers).FirstOrDefault();
            var questionModel = GetQuestion(question, mockTestAnswer != null);
            questionModel.ResultAnswer = _mapper.Map<AnswerModel>(mockTestAnswer);
            return questionModel;
        }

        private async Task<QuestionModel?> GetQuestionByExtraPratice(Guid questionId)
        {
            var question = await _questionRepository.Queryable.Include(x => x.ExtraPracticeAnswers).FirstOrDefaultAsync(x => x.Id == questionId);
            if (question == null)
            {
                return default;
            }
            var extraPracticeAnswer = question.ExtraPracticeAnswers.FirstOrDefault();
            var questionModel = GetQuestion(question, extraPracticeAnswer != null);
            questionModel.ResultAnswer = _mapper.Map<AnswerModel>(extraPracticeAnswer);
            return questionModel;
        }

        private async Task<QuestionModel?> GetQuestionByFinalTest(Guid questionId)
        {
            var question = await _questionRepository.Queryable.Include(x => x.SectionQuestions).ThenInclude(x => x.FinalTestAnswers).FirstOrDefaultAsync(x => x.Id == questionId);
            if (question == null)
            {
                return default;
            }
            var finalTestAnswer = question.SectionQuestions.SelectMany(x => x.FinalTestAnswers).FirstOrDefault();
            var questionModel = GetQuestion(question, finalTestAnswer != null);
            questionModel.ResultAnswer = _mapper.Map<AnswerModel>(finalTestAnswer);
            return questionModel;
        }

        private QuestionModel GetQuestion(Question question, bool isShowAnswer = false)
        {
            return new QuestionModel()
            {
                Id = question.Id,
                QuestionType = question.QuestionType,
                CorrectTotal = question.CorrectTotal,
                Explanation = question.Explanation,
                Ungraded = question.Ungraded,
                Config = _questionTypeConverter.QuestionTypeConverterObject(question.Config, question.QuestionType, isDisableAnswers: !(isShowAnswer)).Item1,
            };
        }
    }
}
