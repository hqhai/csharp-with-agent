// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Application.Queries.RandomQuestionsQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Cms.PlanetDefender.Application.Services.LmsCourseServices;
    using Fsel.Cms.PlanetDefender.Application.Services.SystemServices;
    using Fsel.Cms.PlanetDefender.Application.Services.SystemServices.Models;
    using Fsel.Cms.PlanetDefender.Application.Services.UserServices;
    using Fsel.Cms.PlanetDefender.Domain.Entities;
    using Fsel.Cms.PlanetDefender.Domain.IRepositories;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class RandomQuestionsQuery : IRequest<MethodResult<IList<GameVocabularyTypeModel>>>
    {
        public int RoundNumber { get; set; }
    }

    public class RandomQuestionsQueryHandler : IRequestHandler<RandomQuestionsQuery, MethodResult<IList<GameVocabularyTypeModel>>>
    {
        private readonly ISystemService _systemService;
        private readonly IUserService _userService;
        private readonly AuthContext _authContext;
        private readonly IGameAnswerRepository _gameAnswerRepository;
        private readonly IGameplayRuleConfigRepository _gameplayRuleConfigRepository;
        private readonly IGameplayTimeConfigRepository _gameplayTimeConfigRepository;
        private readonly ICourseService _courseService;
        private readonly IStudentGameInfoRepository _studentGameInfoRepository;

        public RandomQuestionsQueryHandler(ISystemService systemService, IUserService userService, AuthContext authContext, IGameAnswerRepository gameAnswerRepository, IGameplayRuleConfigRepository gameplayRuleConfigRepository, IGameplayTimeConfigRepository gameplayTimeConfigRepository, ICourseService courseService, IStudentGameInfoRepository studentGameInfoRepository)
        {
            _systemService = systemService;
            _userService = userService;
            _authContext = authContext;
            _gameAnswerRepository = gameAnswerRepository;
            _gameplayRuleConfigRepository = gameplayRuleConfigRepository;
            _gameplayTimeConfigRepository = gameplayTimeConfigRepository;
            _courseService = courseService;
            _studentGameInfoRepository = studentGameInfoRepository;
        }

        public async Task<MethodResult<IList<GameVocabularyTypeModel>>> Handle(RandomQuestionsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<GameVocabularyTypeModel>>();
            EnumGameCefrLevel courseLevel = default;
            int unitNumber = default;
            var role = _authContext.Roles?.FirstOrDefault();
            if (role == EnumRole.Student.ToString())
            {
                var courseUnitResult = await _courseService.GetCourseUnitByUserId(_authContext.CurrentUserId);
                if (!courseUnitResult.IsSuccessStatusCode || courseUnitResult.Content?.Result == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                    return methodResult;
                }
                var courseUnit = courseUnitResult.Content.Result;
                courseLevel = (EnumGameCefrLevel)courseUnit.CourseLevel;
                unitNumber = courseUnit.UnitNumber;
            }
            else if (role == EnumRole.Guest.ToString())
            {
                var studentGameInfo = await _studentGameInfoRepository.Queryable.FirstOrDefaultAsync(p => p.CreatedUserId == _authContext.CurrentUserId, cancellationToken);
                if (studentGameInfo == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                    return methodResult;
                }
                courseLevel = (EnumGameCefrLevel)studentGameInfo.CourseLevel;
                unitNumber = courseLevel <= (EnumGameCefrLevel)6 ? 12 : 8;
            }
            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode || studentResult.Content?.Result == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }
            var student = studentResult.Content.Result;

            var gameVocabulariesResult = await _systemService.ExecuteListGameVocabularyQueryAsync(new BaseQueryModel { IncludePaths = new List<string> { "GameVocabularyTypes" } });
            if (!gameVocabulariesResult.IsSuccessStatusCode || gameVocabulariesResult.Content?.Result == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }
            var gameVocabularies = gameVocabulariesResult.Content.Result;

            var questionRule = await _gameplayRuleConfigRepository.Queryable.FirstOrDefaultAsync(p => p.StartRoundNumber <= request.RoundNumber && request.RoundNumber <= p.EndRoundNumber, cancellationToken);
            if (questionRule == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var gameVocabularyCorrectIds = await _gameAnswerRepository.Queryable.Where(p => p.StudentId == student.Id && p.IsCorrect).Select(p => p.GameVocabularyId).ToListAsync(cancellationToken);
            var gameVocabularyTypeInCorrectIds = await _gameAnswerRepository.Queryable.Where(p => p.StudentId == student.Id && !p.IsCorrect).Select(p => p.GameVocabularyTypeId).ToListAsync(cancellationToken);

            var gameVocabulariesModel = new List<GameVocabularyModel>();

            GetCurrentQuestions(gameVocabularies.ToList(), courseLevel, unitNumber, null, questionRule, 0, gameVocabulariesModel, false, courseLevel, _gameAnswerRepository, student.Id, unitNumber);

            foreach (var item in gameVocabulariesModel.SelectMany(p => p.GameVocabularyTypes!))
            {
                if (gameVocabularyTypeInCorrectIds.Contains(item.Id))
                {
                    gameVocabulariesModel.SelectMany(p => p.GameVocabularyTypes!).ToList().Remove(item);
                }
            }

            var questionTime = await _gameplayTimeConfigRepository.Queryable.Where(p => p.RoundNumber == request.RoundNumber).ToListAsync(cancellationToken);

            var gameVocabularyTypeModel = new List<GameVocabularyTypeModel>();
            int count = 0;
            while (count < 100)
            {
                var gameVocabularyType = gameVocabulariesModel.SelectMany(p => p.GameVocabularyTypes!).OrderBy(p => Guid.NewGuid()).ToList();

                gameVocabularyTypeModel = new List<GameVocabularyTypeModel>();

                GetQuestion(questionTime, request.RoundNumber, questionRule, gameVocabularyTypeModel, gameVocabularyType, EnumGameVocabType.Hint, EnumGameVocabPDType.Hint);
                GetQuestion(questionTime, request.RoundNumber, questionRule, gameVocabularyTypeModel, gameVocabularyType, EnumGameVocabType.Audio, EnumGameVocabPDType.Audio);
                GetQuestion(questionTime, request.RoundNumber, questionRule, gameVocabularyTypeModel, gameVocabularyType, EnumGameVocabType.Image, EnumGameVocabPDType.Image);
                GetQuestion(questionTime, request.RoundNumber, questionRule, gameVocabularyTypeModel, gameVocabularyType, EnumGameVocabType.Definition, EnumGameVocabPDType.Definition);
                GetQuestion(questionTime, request.RoundNumber, questionRule, gameVocabularyTypeModel, gameVocabularyType, EnumGameVocabType.JumbledSpelling, EnumGameVocabPDType.JumbledSpelling);

                if (gameVocabularyTypeModel.Count == (questionRule.CurrentUnitOutside + questionRule.CurrentUnit + questionRule.PreviousUnit + questionRule.PreviousUnitOutside))
                {
                    break;
                }
                count++;
            }

            foreach (var item in gameVocabularyTypeModel)
            {
                if (item.GameVocabType == EnumGameVocabType.JumbledSpelling)
                {
                    item.QuestionContent = StringHelper.RandomCharacters(item.QuestionContent ?? string.Empty);
                }
            }
            methodResult.Result = gameVocabularyTypeModel;
            return methodResult;
        }

        private static void GetQuestion(List<GameplayTimeConfig> questionTime, int roundNumber, GameplayRuleConfig questionRule, List<GameVocabularyTypeModel> gameVocabularyTypeModel, List<GameVocabularyTypeModel> gameVocabularyType, EnumGameVocabType gameVocabType, EnumGameVocabPDType gameVocabPDType)
        {
            var type = questionTime.First(p => p.GameVocabPDType == gameVocabPDType && p.RoundNumber == roundNumber).Percent;

            var countQuestionType = (int)Math.Round((type * (questionRule.CurrentUnit + questionRule.CurrentUnitOutside + questionRule.PreviousUnit + questionRule.PreviousUnitOutside)) / 100, MidpointRounding.AwayFromZero);

            gameVocabularyTypeModel.AddRange(gameVocabularyType.Where(p => p.GameVocabType == gameVocabType).Take(countQuestionType));

            gameVocabularyType.RemoveAll(p => gameVocabularyTypeModel.Select(x => x.GameVocabularyId).Contains(p.GameVocabularyId));
        }

        private static void GetCurrentQuestions(List<GameVocabularyModel> gameVocabularies, EnumGameCefrLevel level, int unitNumber, IList<Guid>? gameVocabularyCorrectIds, GameplayRuleConfig questionRule, int surplus, List<GameVocabularyModel> gameVocabulariesModel, bool isSecond, EnumGameCefrLevel startLevel, IGameAnswerRepository gameAnswerRepository, Guid studentId, int unitStart)
        {
            var currentQuestions = gameVocabularies.Where(p => p.CefrLevel == level && p.UnitOrder == (EnumUnitNumber)unitNumber && (gameVocabularyCorrectIds == null || !gameVocabularyCorrectIds.Contains(p.Id)) && p.CourseLevel != EnumGameCourseLevel.OutsideCurriculum).ToList();

            currentQuestions = currentQuestions.Take(!isSecond ? questionRule.CurrentUnit : surplus).ToList();

            gameVocabulariesModel.AddRange(currentQuestions);

            gameVocabularies.RemoveAll(x => currentQuestions.Contains(x));

            if (gameVocabulariesModel.Count >= (questionRule.CurrentUnitOutside + questionRule.CurrentUnit + questionRule.PreviousUnit + questionRule.PreviousUnitOutside))
            {
                return;
            }

            surplus = !isSecond ? (questionRule.CurrentUnit - (currentQuestions == null ? 0 : currentQuestions.Count)) : surplus - currentQuestions.Count;

            GetCurrentOutSideQuestions(gameVocabularies, level, unitNumber, gameVocabularyCorrectIds, questionRule, surplus, gameVocabulariesModel, isSecond, startLevel, gameAnswerRepository, studentId, unitStart);
        }

        private static void GetCurrentOutSideQuestions(List<GameVocabularyModel> gameVocabularies, EnumGameCefrLevel level, int unitNumber, IList<Guid>? gameVocabularyCorrectIds, GameplayRuleConfig questionRule, int surplus, List<GameVocabularyModel> gameVocabulariesModel, bool isSecond, EnumGameCefrLevel startLevel, IGameAnswerRepository gameAnswerRepository, Guid studentId, int unitStart)
        {
            var currentOutSideQuestions = gameVocabularies.Where(p => p.CefrLevel == level && p.UnitOrder == (EnumUnitNumber)unitNumber && (gameVocabularyCorrectIds == null || !gameVocabularyCorrectIds.Contains(p.Id)) && p.CourseLevel == EnumGameCourseLevel.OutsideCurriculum).ToList();

            currentOutSideQuestions = currentOutSideQuestions.Take(!isSecond ? (questionRule.CurrentUnitOutside + surplus) : surplus).ToList();

            gameVocabulariesModel.AddRange(currentOutSideQuestions);

            gameVocabularies.RemoveAll(x => currentOutSideQuestions.Contains(x));

            if (gameVocabulariesModel.Count >= (questionRule.CurrentUnitOutside + questionRule.CurrentUnit + questionRule.PreviousUnit + questionRule.PreviousUnitOutside))
            {
                return;
            }

            surplus = !isSecond ? (currentOutSideQuestions == null ? surplus + questionRule.CurrentUnitOutside : (questionRule.CurrentUnitOutside + surplus - currentOutSideQuestions.Count)) : surplus - currentOutSideQuestions.Count;

            if (level == EnumGameCefrLevel.A1 && unitNumber == 1 && gameVocabulariesModel.Count < (questionRule.CurrentUnit + questionRule.CurrentUnitOutside))
            {
                var deleteGameAnswers = gameAnswerRepository.Queryable.Where(p => p.StudentId == studentId).ToList();
                gameAnswerRepository.DeleteListAsync(deleteGameAnswers);
                gameAnswerRepository.UnitOfWork.SaveChangesAsync();

                GetPreviousQuestions(gameVocabularies, startLevel, unitStart, gameVocabularyCorrectIds, questionRule, surplus, gameVocabulariesModel, isSecond, startLevel, gameAnswerRepository, studentId, unitNumber);
            }
            else if (level == EnumGameCefrLevel.A1 && unitNumber == 1 && gameVocabulariesModel.Count >= (questionRule.CurrentUnit + questionRule.CurrentUnitOutside) && currentOutSideQuestions?.Count > 0)
            {
                GetPreviousQuestions(gameVocabularies, level, unitNumber, gameVocabularyCorrectIds, questionRule, surplus, gameVocabulariesModel, isSecond, startLevel, gameAnswerRepository, studentId, unitStart);
            }
            else if (level == EnumGameCefrLevel.A1 && unitNumber == 1 && gameVocabulariesModel.Count >= (questionRule.CurrentUnit + questionRule.CurrentUnitOutside) && currentOutSideQuestions?.Count == 0)
            {
                var deleteGameAnswers = gameAnswerRepository.Queryable.Where(p => p.StudentId == studentId).ToList();
                gameAnswerRepository.DeleteListAsync(deleteGameAnswers);
                gameAnswerRepository.UnitOfWork.SaveChangesAsync();

                GetPreviousQuestions(gameVocabularies, startLevel, unitStart, gameVocabularyCorrectIds, questionRule, surplus, gameVocabulariesModel, isSecond, startLevel, gameAnswerRepository, studentId, unitNumber);
            }
            else
            {
                GetPreviousQuestions(gameVocabularies, unitNumber == 1 ? (level - 1 >= 0 ? level - 1 : startLevel) : level, unitNumber == 1 ? (level - 1 >= 0 ? (level - 1 <= (EnumGameCefrLevel)6 ? 12 : 8) : unitStart) : unitNumber - 1, gameVocabularyCorrectIds, questionRule, surplus, gameVocabulariesModel, isSecond, startLevel, gameAnswerRepository, studentId, unitNumber);
            }
        }

        private static void GetPreviousQuestions(List<GameVocabularyModel> gameVocabularies, EnumGameCefrLevel level, int unitNumber, IList<Guid>? gameVocabularyCorrectIds, GameplayRuleConfig questionRule, int surplus, List<GameVocabularyModel> gameVocabulariesModel, bool isSecond, EnumGameCefrLevel startLevel, IGameAnswerRepository gameAnswerRepository, Guid studentId, int unitStart)
        {
            var previousQuestions = gameVocabularies.Where(p => p.CefrLevel == level && p.UnitOrder == (EnumUnitNumber)unitNumber && (gameVocabularyCorrectIds == null || !gameVocabularyCorrectIds.Contains(p.Id)) && p.CourseLevel != EnumGameCourseLevel.OutsideCurriculum).ToList();

            previousQuestions = previousQuestions.Take(!isSecond ? questionRule.PreviousUnit + surplus : surplus).ToList();

            gameVocabulariesModel.AddRange(previousQuestions);

            gameVocabularies.RemoveAll(x => previousQuestions.Contains(x));

            if (gameVocabulariesModel.Count >= (questionRule.CurrentUnitOutside + questionRule.CurrentUnit + questionRule.PreviousUnit + questionRule.PreviousUnitOutside))
            {
                return;
            }

            surplus = !isSecond ? (previousQuestions == null ? surplus + questionRule.PreviousUnit : (questionRule.PreviousUnit + surplus - previousQuestions.Count)) : surplus - previousQuestions.Count;

            GetPreviousOutSideQuestions(gameVocabularies, level, unitNumber, gameVocabularyCorrectIds, questionRule, surplus, gameVocabulariesModel, isSecond, startLevel, gameAnswerRepository, studentId, unitStart);
        }

        private static void GetPreviousOutSideQuestions(List<GameVocabularyModel> gameVocabularies, EnumGameCefrLevel level, int unitNumber, IList<Guid>? gameVocabularyCorrectIds, GameplayRuleConfig questionRule, int surplus, List<GameVocabularyModel> gameVocabulariesModel, bool isSecond, EnumGameCefrLevel startLevel, IGameAnswerRepository gameAnswerRepository, Guid studentId, int startUnit)
        {
            var previousOutSideQuestions = gameVocabularies.Where(p => p.CefrLevel == level && p.UnitOrder == (EnumUnitNumber)unitNumber && (gameVocabularyCorrectIds == null || !gameVocabularyCorrectIds.Contains(p.Id)) && p.CourseLevel == EnumGameCourseLevel.OutsideCurriculum).ToList();

            previousOutSideQuestions = previousOutSideQuestions.Take(!isSecond ? questionRule.PreviousUnitOutside + surplus : surplus).ToList();
            gameVocabulariesModel.AddRange(previousOutSideQuestions);

            gameVocabularies.RemoveAll(x => previousOutSideQuestions.Contains(x));

            if (gameVocabulariesModel.Count >= (questionRule.CurrentUnitOutside + questionRule.CurrentUnit + questionRule.PreviousUnit + questionRule.PreviousUnitOutside))
            {
                return;
            }

            surplus = !isSecond ? previousOutSideQuestions == null ? surplus + questionRule.PreviousUnitOutside : (questionRule.PreviousUnit + surplus - previousOutSideQuestions.Count) : surplus - previousOutSideQuestions.Count;

            GetCurrentQuestions(gameVocabularies, unitNumber == 1 ? (level - 1 >= 0 ? level - 1 : startLevel) : level, unitNumber == 1 ? (level - 1 >= 0 ? (level - 1 <= (EnumGameCefrLevel)6 ? 12 : 8) : startUnit) : unitNumber - 1, gameVocabularyCorrectIds, questionRule, surplus, gameVocabulariesModel, true, startLevel, gameAnswerRepository, studentId, unitNumber);
        }
    }
}
