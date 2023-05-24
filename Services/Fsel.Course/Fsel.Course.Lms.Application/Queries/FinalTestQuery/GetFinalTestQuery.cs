// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.FinalTestQuery
{
    using System;
    using System.Threading;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetFinalTestQuery : IRequest<MethodResult<FinalTestModel>>
    {
        public Guid FinalTestId { get; set; }
    }

    public class GetFinalTestQueryHandler : IRequestHandler<GetFinalTestQuery, MethodResult<FinalTestModel>>
    {
        private readonly QuestionTypeConverter _questionTypeConverter;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly AuthContext _authContext;
        private readonly IFinalTestResultRepository _finalTestResultRepository;
        private readonly IFinalTestRepository _finalTestRepository;

        public GetFinalTestQueryHandler(
            QuestionTypeConverter questionTypeConverter
            , IUserService userService
            , IMapper mapper
            , AuthContext authContext
            , IFinalTestResultRepository finalTestResultRepository
            , IFinalTestRepository finalTestRepository)
        {
            _questionTypeConverter = questionTypeConverter;
            _userService = userService;
            _mapper = mapper;
            _authContext = authContext;
            _finalTestResultRepository = finalTestResultRepository;
            _finalTestRepository = finalTestRepository;
        }

        public async Task<MethodResult<FinalTestModel>> Handle(GetFinalTestQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<FinalTestModel> methodResult = new MethodResult<FinalTestModel>();

            var studentsResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (studentsResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumVideoErrorCode.UserNotExist));
                return methodResult;
            }
            var studentId = studentsResult.Content!.Result!.Id;

            var finalTestResult = await _finalTestResultRepository.Queryable.FirstOrDefaultAsync(x => x.FinalTestId == request.FinalTestId && x.StudentId == studentId, cancellationToken);

            var finalTest = await _finalTestRepository.Queryable.Include(x => x.FinalTestExercises)
                                                        .ThenInclude(x => x.Exercise)
                                                        .ThenInclude(x => x!.ExerciseQuestions)
                                                        .ThenInclude(x => x!.Question)
                                                        .ThenInclude(x => x!.ExerciseQuestions)
                                                        .ThenInclude(x => x!.FinalTestExerciseAnswers)
                                                        .FirstOrDefaultAsync(x => x.Id == request.FinalTestId, cancellationToken);

            if (finalTest == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumFinalTestErrorCode.FinalTestsNotExist));
                return methodResult;
            }
            var checkDone = finalTestResult != null && finalTestResult.Status == EnumResultStatus.Done;
            var finalTestModel = new FinalTestModel
            {
                Id = finalTest.Id,
                Name = finalTest.Name,
                IsActive = finalTest.IsActive,
                ExecutionTime = finalTest.ExecutionTime,
                FinalTestLevel = finalTest.FinalTestLevel,
                Exercises = finalTest.FinalTestExercises.Where(n => n.Exercise != null).Select(n => n.Exercise).Select(n => new ExerciseModel
                {
                    Id = n!.Id,
                    MediaPost = n.MediaPost,
                    CourseSkill = n.CourseSkill,
                    Questions = n.ExerciseQuestions.Where(m => m.Question != null).Select(m => m.Question).Select(m => new QuestionModel()
                    {
                        Id = m!.Id,
                        QuestionType = m.QuestionType,
                        CorrectTotal = m.CorrectTotal,
                        Explanation = m.Explanation,
                        Ungraded = m.Ungraded,
                        Config = _questionTypeConverter.QuestionTypeConverterObject(m.Config, m.QuestionType, false, true).Item1,
                        ResultAnswer = finalTestResult == null ? null : _mapper.Map<FinalTestExerciseAnswerModel>(m.ExerciseQuestions.SelectMany(x => x.FinalTestExerciseAnswers).FirstOrDefault(x => x.FinalTestResultId == finalTestResult.Id))
                    }).ToList()
                }).ToList(),
                FinalTestResult = finalTestResult == null ? null : new FinalTestResultModel
                {
                    Id = finalTestResult.Id,
                    CorrectCount = finalTestResult.CorrectCount,
                    CorrectTotal = finalTestResult.CorrectTotal,
                    Percent = finalTestResult.Percent,
                    Status = finalTestResult.Status,
                    SkillScores = finalTestResult.SkillScores,
                    FinalTestId = finalTestResult.FinalTestId,
                    StudentId = finalTestResult.StudentId,
                    CourseId = finalTestResult.CourseId,
                }
            };
            methodResult.Result = finalTestModel;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
