// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Application.Commands.GameHistoryCmd
{
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Cms.PlanetDefender.Application.Services.SystemServices;
    using Fsel.Cms.PlanetDefender.Application.Services.UserServices;
    using Fsel.Cms.PlanetDefender.Domain.Entities;
    using Fsel.Cms.PlanetDefender.Domain.IRepositories;
    using Fsel.Cms.PlanetDefender.Domain.Models.CommandModel.GameAnswers;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Models;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class AnswerTheQuestionCommand : AnswerTheQuestionCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class AnswerTheQuestionCommandHandler : IRequestHandler<AnswerTheQuestionCommand, MethodResult<bool>>
    {
        private readonly ISystemService _systemService;
        private readonly IGameAnswerRepository _gameAnswerRepository;
        private readonly IUserService _userService;
        private readonly AuthContext _authContext;
        private readonly IGameHistoryRepository _gameHistoryRepository;
        private readonly IMapper _mapper;

        public AnswerTheQuestionCommandHandler(ISystemService systemService, IGameAnswerRepository gameAnswerRepository, IUserService userService, AuthContext authContext, IGameHistoryRepository gameHistoryRepository, IMapper mapper)
        {
            _systemService = systemService;
            _gameAnswerRepository = gameAnswerRepository;
            _userService = userService;
            _authContext = authContext;
            _gameHistoryRepository = gameHistoryRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<bool>> Handle(AnswerTheQuestionCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();
            if (!await _gameHistoryRepository.Queryable.AnyAsync(p => p.Id == request.GameHistoryId, cancellationToken))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }
            var gameVocabularyResult = await _systemService.ExecuteListGameVocabularyQueryAsync(new BaseQueryModel
            {
                Filters = new List<GenericFilterModel>() { new GenericFilterModel { Property = "Id", Operator = Common.Enums.EnumFilterOperator.Equal, Value = request.GameVocabularyId } },
                IncludePaths = new List<string>() { "GameVocabularyTypes" }
            });

            var gameVocabulary = gameVocabularyResult.Content?.Result?.FirstOrDefault();
            if (gameVocabulary == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }
            if (!gameVocabulary.GameVocabularyTypes!.Select(p => p.Id).Contains(request.GameVocabularyTypeId))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }
            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode || studentResult.Content?.Result == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }
            var student = studentResult.Content.Result;

            var gameAnswer = _mapper.Map<GameAnswer>(request);
            gameAnswer.IsCorrect = gameVocabulary.Key?.ToLower(CultureInfo.CurrentCulture) == request.Answer?.ToLower(CultureInfo.CurrentCulture);
            gameAnswer.StudentId = student.Id;

            _gameAnswerRepository.Add(gameAnswer);
            await _gameAnswerRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            methodResult.Result = gameAnswer.IsCorrect;
            return methodResult;
        }
    }
}
