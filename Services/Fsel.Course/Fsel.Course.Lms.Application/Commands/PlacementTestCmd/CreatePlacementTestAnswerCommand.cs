// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.PlacementTestCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.PlacementTestAnswers;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreatePlacementTestAnswerCommand : CreatePlacementTestAnswerCommandModel, IRequest<MethodResult<PlacementTestResultModel>>
    {
    }

    public class CreatePlacementTestAnswerCommandHandler : IRequestHandler<CreatePlacementTestAnswerCommand, MethodResult<PlacementTestResultModel>>
    {
        private readonly IPlacementTestAnswerRepository _placementTestAnswerRepository;
        private readonly IPlacementTestResultRepository _placementTestResultRepository;
        private readonly IUserService _userService;
        private readonly IQuestionRepository _questionRepository;
        private readonly IMapper _mapper;
        private readonly ISectionGroupRepository _sectionGroupRepository;
        private readonly ISectionPartRepository _sectionPartRepository;
        private readonly ISectionQuestionRepository _sectionQuestionRepository;
        private readonly ISectionRepository _sectionRepository;
        private readonly AuthContext _authContext;
        private readonly AnswerTypeConverter _answerTypeConverter;

        public CreatePlacementTestAnswerCommandHandler(
             IPlacementTestAnswerRepository placementTestAnswerRepository
            , IPlacementTestResultRepository placementTestResultRepository
            , IUserService userService
            , IQuestionRepository questionRepository
            , IMapper mapper
            , ISectionGroupRepository sectionGroupRepository
            , ISectionPartRepository sectionPartRepository
            , ISectionQuestionRepository sectionQuestionRepository
            , ISectionRepository sectionRepository
            , AuthContext authContext
            , AnswerTypeConverter answerTypeConverter)
        {
            _placementTestAnswerRepository = placementTestAnswerRepository;
            _placementTestResultRepository = placementTestResultRepository;
            _userService = userService;
            _questionRepository = questionRepository;
            _mapper = mapper;
            _sectionGroupRepository = sectionGroupRepository;
            _sectionPartRepository = sectionPartRepository;
            _sectionQuestionRepository = sectionQuestionRepository;
            _sectionRepository = sectionRepository;
            _authContext = authContext;
            _answerTypeConverter = answerTypeConverter;
        }

        public async Task<MethodResult<PlacementTestResultModel>> Handle(CreatePlacementTestAnswerCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<PlacementTestResultModel> methodResult = new MethodResult<PlacementTestResultModel>();

            #region Validation

            if (request.Answers == null || request.Answers.Any(x => x.Answer == null) || request.Answers.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumPlacementTestAnswerErrorCode.AnswersNull), nameof(request.Answers), request.Answers);
                return methodResult;
            }
            var student = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!student.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumLessonErrorCode.UserNotExist), nameof(student), _authContext.CurrentUserId.ToString());
                return methodResult;
            }

            var studentId = student?.Content?.Result?.Id;

            var placementTestResult = await _placementTestResultRepository.Queryable.FirstOrDefaultAsync(x => x.Level == request.Level && x.Status == EnumResultStatus.Process, cancellationToken);
            if (placementTestResult == null)
            {
                placementTestResult = new PlacementTestResult
                {
                    Status = EnumResultStatus.Process,
                    Level = request.Level,
                    StudentId = studentId ?? default
                };
            }

            var questionIds = request.Answers.Select(x => x.QuestionId).ToList();
            var questions = await _questionRepository.GetIncludeSectionByIdAsync(questionIds);
            if (questions == null || questions.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumQuestionErrorCode.QuestionsNotExist), nameof(questionIds), questionIds);
                return methodResult;
            }

            var placementTestAnswers = new List<PlacementTestAnswer>();
            int correctCountStudent = 0;
            foreach (var item in request.Answers)
            {
                var question = questions.FirstOrDefault(x => x.Id == item.QuestionId);
                if (question == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumQuestionErrorCode.QuestionNull), nameof(item.QuestionId), item.QuestionId);
                    return methodResult;
                }
                else if (question.Config == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumQuestionErrorCode.QuestionConfigNull), nameof(question), question);
                    return methodResult;
                }
                else if (question.SectionQuestions == null || question.SectionQuestions.Count == 0)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSectionQuestionErrorCode.SectionQuestionsNotExist), nameof(question.SectionQuestions));
                    return methodResult;
                }
                var sectionQuestionId = question.SectionQuestions.FirstOrDefault()!.Id;
                var placementTestAnswer = await _placementTestAnswerRepository.Queryable.FirstOrDefaultAsync(x => x.PlacementTestResultId == placementTestResult.Id && x.SectionQuestionId == sectionQuestionId, cancellationToken);

                if (placementTestAnswer == null)
                {
                    var (answerConfig, correctCount) = _answerTypeConverter.GetTotalCorrectByAsnwerType(item.Answer, question.Config, question.QuestionType);
                    if (answerConfig == null)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumPlacementTestAnswerErrorCode.AnswerIsInTheWrongFormat), nameof(item.Answer), item.Answer);
                        return methodResult;
                    }
                    correctCountStudent += correctCount;
                    placementTestAnswer = new PlacementTestAnswer
                    {
                        CorrectCount = correctCount,
                        Answer = answerConfig,
                        PlacementTestResultId = placementTestResult.Id,
                        SectionQuestionId = sectionQuestionId
                    };
                    placementTestAnswers.Add(placementTestAnswer);
                }
            }

            #endregion Validation

            #region Update placementTestResult

            IQueryable<SkillScores>? skillScoresQuery = null;
            if (placementTestResult.Level != EnumPlacementTestLevel.IELTS)
            {
                skillScoresQuery = from pr in _placementTestResultRepository.Queryable
                                   join pa in _placementTestAnswerRepository.Queryable on pr.Id equals pa.PlacementTestResultId
                                   join sq in _sectionQuestionRepository.Queryable on pa.SectionQuestionId equals sq.Id
                                   join s in _sectionRepository.Queryable on sq.SectionId equals s.Id
                                   join sg in _sectionGroupRepository.Queryable on s.SectionGroupId equals sg.Id
                                   join q in _questionRepository.Queryable on sq.QuestionId equals q.Id
                                   where pr.Id == placementTestResult.Id
                                   group q by sg.CourseSkill into g
                                   select new SkillScores
                                   {
                                       Skill = g.Key,
                                       Total = g.Sum(x => x.CorrectTotal),
                                   };
            }
            else
            {
                skillScoresQuery = from pr in _placementTestResultRepository.Queryable
                                   join pa in _placementTestAnswerRepository.Queryable on pr.Id equals pa.PlacementTestResultId
                                   join sq in _sectionQuestionRepository.Queryable on pa.SectionQuestionId equals sq.Id
                                   join sp in _sectionPartRepository.Queryable on sq.SectionPartId equals sp.Id
                                   join s in _sectionRepository.Queryable on sp.SectionId equals s.Id
                                   join sg in _sectionGroupRepository.Queryable on s.SectionGroupId equals sg.Id
                                   join q in _questionRepository.Queryable on sq.QuestionId equals q.Id
                                   where pr.Id == placementTestResult.Id
                                   group q by sg.CourseSkill into g
                                   select new SkillScores
                                   {
                                       Skill = g.Key,
                                       Total = g.Sum(x => x.CorrectTotal),
                                   };
            }

            var skillScoreTotals = await skillScoresQuery.ToListAsync(cancellationToken);
            //var skillScoreScores = await answerQuery.ToListAsync(cancellationToken);
            //foreach (var skillScore in skillScoreTotals)
            //{
            //    var number = skillScoreScores.FirstOrDefault(x => x.Skill == skillScore.Skill)!.Scores;
            //    skillScore.Scores = number;
            //    if (placementTestResult.Level == EnumPlacementTestLevel.IELTS)
            //    {
            //        if (skillScore.Skill == EnumCourseSkill.Reading)
            //        {
            //            skillScore.Number = number.GetReadingCountIelts();
            //        }
            //        else if (skillScore.Skill == EnumCourseSkill.Listening)
            //        {
            //            skillScore.Number = number.GetListeningCountIelts();
            //        }
            //    }
            //}
            placementTestResult.CorrectCount = correctCountStudent;
            placementTestResult.CorrectTotal = Convert.ToInt32(skillScoreTotals.Sum(x => x.Total));
            placementTestResult.Status = EnumResultStatus.Done;
            placementTestResult.SkillScores = skillScoreTotals;
            placementTestResult.Percent = (double)placementTestResult.CorrectCount / placementTestResult.CorrectTotal * 100;
            placementTestResult = _placementTestResultRepository.Update(placementTestResult);
            await _placementTestResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            placementTestResult = _placementTestResultRepository.Add(placementTestResult);
            await _placementTestResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

            #endregion Update placementTestResult

            await _placementTestAnswerRepository.ExecuteTransactionAsync(async () =>
            {
                if (placementTestAnswers.Count > 0)
                {
                    await _placementTestAnswerRepository.AddList(placementTestAnswers);
                    await _placementTestAnswerRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                }

                _placementTestResultRepository.Update(placementTestResult);
                await _placementTestResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<PlacementTestResultModel>(placementTestResult);
                return methodResult;
            });

            return methodResult;
        }
    }
}
