// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices.TestRequestHandler
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Shared.Enums;
    using Microsoft.EntityFrameworkCore;

    public interface ITestRequestCreateAnswerHandler : IBaseTestRequestHandler
    {
    }

    public class TestRequestCreateAnswerHandler : BaseTestRequestHandler, ITestRequestCreateAnswerHandler
    {
        private readonly IQuestionRepository _questionRepository;
        private readonly IRepository<TestAnswer> _testAnswerRepository;
        private readonly QuestionConverter _questionConverter;

        public TestRequestCreateAnswerHandler(IQuestionRepository questionRepository,
            IRepository<TestAnswer> placementTestAnswerRepository,
            QuestionConverter questionConverter)
        {
            _questionRepository = questionRepository;
            _testAnswerRepository = placementTestAnswerRepository;
            _questionConverter = questionConverter;
        }

        public override async Task Handle(TestRequestContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            var request = context.TestRequestCommand;
            if (request.Answers != null && request.Answers.Any())
            {
                var questionIds = request.Answers.Select(x => x.QuestionId).ToList();
                var questions = await _questionRepository.GetIncludeSectionByIdAsync(questionIds);
                if (questions != null && questions.Any())
                {
                    var testAnswers = await _testAnswerRepository.Queryable.Where(x => x.TestSectionResultId == request.SectionResultId).ToListAsync();
                    foreach (var item in request.Answers)
                    {
                        var question = questions.FirstOrDefault(x => x.Id == item.QuestionId);
                        var questionResult = _questionConverter.HandleAnswerTest(question, item.Answer, request.IsSubmit);
                        if (!questionResult.IsOK)
                        {
                            context.MethodResult.AddErrorBadRequest(questionResult.ErrorMessages);
                            return;
                        }
                        var (questionItem, answerConfig, correctCount, isAnswered) = questionResult.Result;

                        var questionId = questionItem.TestSectionQuestions.FirstOrDefault()?.QuestionId ?? default;
                        var testAnswer = testAnswers.FirstOrDefault(x => x.QuestionId == questionId);
                        if (testAnswer == null)
                        {
                            testAnswer = new TestAnswer
                            {
                                TestResultId = context.TestSectionResult.TestResultId,
                                TestSectionResultId = request.SectionResultId,
                                QuestionId = questionId,
                                StudentId = context.Student.Id
                            };

                            _testAnswerRepository.Add(testAnswer);
                        }

                        testAnswer.Answer = answerConfig;
                        testAnswer.CorrectCount = correctCount;
                        testAnswer.IsCorrect = isAnswered ? correctCount == questionItem.CorrectTotal : null;
                        testAnswer.Status = questionItem.CorrectTotal == correctCount ? EnumAnswerStatus.Done : EnumAnswerStatus.Process;

                        if (!testAnswer.IsValid())
                        {
                            context.MethodResult.AddErrorBadRequest(testAnswer.ErrorMessages);
                            return;
                        }
                    }

                    await _testAnswerRepository.DbContext.BulkSaveChangesAsync();
                }
            }
        }
    }
}
