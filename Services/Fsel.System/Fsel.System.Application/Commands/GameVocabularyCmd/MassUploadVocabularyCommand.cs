// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.GameVocabularyCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Common.Models.Excels;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Fsel.System.Application.Services.UserServices;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.CommandModels.GameVocabularies;
    using global::System.Globalization;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class MassUploadVocabularyCommand : BaseImportCommandModel, IRequest<MethodResult<Stream>>
    {
    }

    public class MassUploadVocabularyCommandHandler : IRequestHandler<MassUploadVocabularyCommand, MethodResult<Stream>>
    {
        private readonly IGameVocabularyRepository _gameVocabularyRepository;
        private readonly IGameTopicRepository _gameTopicRepository;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;

        public MassUploadVocabularyCommandHandler(IGameVocabularyRepository gameVocabularyRepository, IGameTopicRepository gameTopicRepository, IMapper mapper, IUserService userService)
        {
            _gameVocabularyRepository = gameVocabularyRepository;
            _gameTopicRepository = gameTopicRepository;
            _mapper = mapper;
            _userService = userService;
        }

        public async Task<MethodResult<Stream>> Handle(MassUploadVocabularyCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<Stream>();

            var result = request.FormFile?.ImportAndValidateExcel(async (MassUploadVocabularyCommandModel x, IList<MassUploadVocabularyCommandModel> models, int rowIndex, IList<ValidateExcelModel> errors) =>
            {
                if (!string.IsNullOrEmpty(x.Code) && CheckDuplicate(models.Where(p => !string.IsNullOrEmpty(p.Code)).Select(x => x.Code).ToList(), x.Code))
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.Code), Message = "Duplicate Code" });
                }

                if (!string.IsNullOrEmpty(x.Code) && (x.Code?.Length != 7 || !int.TryParse(x.Code, out int checkCode)))
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.Code), Message = "Invalid Code" });
                }

                if (!string.IsNullOrEmpty(x.Code) && _gameVocabularyRepository.Queryable.Any(p => p.Code == x.Code))
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.Code), Message = "Code Already Exist" });
                }

                if (!string.IsNullOrEmpty(x.Key) && CheckDuplicate(models.Where(p => !string.IsNullOrEmpty(p.Key)).Select(x => x.Key).ToList(), x.Key))
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.Key), Message = "Duplicate Key" });
                }

                if (string.IsNullOrEmpty(x.Key))
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.Key), Message = "Key Null" });
                }

                if (_gameVocabularyRepository.Queryable.Any(p => p.Key == x.Key))
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.Key), Message = "Key Already Exist" });
                }

                if (!string.IsNullOrEmpty(x.WordCategory) && !_gameTopicRepository.Queryable.Any(p => p.Value == x.WordCategory))
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.WordCategory), Message = "Word Category Not Exist" });
                }

                return await Task.FromResult(errors.Count == 0);
            });

            if (result?.Stream != null)
            {
                methodResult.Result = result.Stream;
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }

            var datas = result?.Datas.ToList();
            var gameVocabularies = new List<GameVocabulary>();

            var platformsResult = await _userService.GetPlatformsQueryAsync(new BaseQueryModel { });
            var platforms = platformsResult.Content?.Result;

            var countGameVocabulary = await _gameVocabularyRepository.Queryable.CountAsync(cancellationToken);

            if (datas != null)
            {
                foreach (var item in datas)
                {
                    var gameVocabulary = _mapper.Map<GameVocabulary>(item);

                    if (string.IsNullOrEmpty(item.Code))
                    {
                        while (true)
                        {
                            string generateCode = countGameVocabulary.ToString("D7", CultureInfo.CurrentCulture);
                            if (!_gameVocabularyRepository.Queryable.Any(p => p.Code == generateCode) && !gameVocabularies.Any(p => p.Code == generateCode))
                            {
                                gameVocabulary.Code = generateCode;
                                break;
                            }
                            countGameVocabulary++;
                        }
                    }

                    gameVocabulary.WordCategoryId = _gameTopicRepository.Queryable.FirstOrDefault(p => p.Value == item.WordCategory)?.Id;

                    AddGameVocabularyType(gameVocabulary, nameof(item.AlternateSpelling), item.AlternateSpelling);
                    AddGameVocabularyType(gameVocabulary, nameof(item.UsEquivalent), item.UsEquivalent);
                    AddGameVocabularyType(gameVocabulary, nameof(item.Definition), item.Definition);
                    AddGameVocabularyType(gameVocabulary, nameof(item.Hint), item.Hint);
                    AddGameVocabularyType(gameVocabulary, nameof(item.ExampleSentence), item.ExampleSentence);
                    AddGameVocabularyType(gameVocabulary, nameof(item.Image), item.Image);
                    AddGameVocabularyType(gameVocabulary, nameof(item.Audio), item.Audio);
                    AddGameVocabularyType(gameVocabulary, nameof(item.Synonym), item.Synonym);
                    AddGameVocabularyType(gameVocabulary, nameof(item.Antonym), item.Antonym);
                    AddGameVocabularyType(gameVocabulary, nameof(item.PhoneticTranscription), item.PhoneticTranscription);
                    AddGameVocabularyType(gameVocabulary, EnumGameVocabType.JumbledSpelling.ToString(), item.Key);

                    var listEnumGameVocabType = gameVocabulary.GameVocabularyTypes?.Select(p => p.GameVocabType).ToList();

                    var platformIds = new List<Guid>();
                    if (listEnumGameVocabType?.Count > 0)
                    {
                        var platformCodes = PlatformCodeHelper.GetEnumPlatformCodes(listEnumGameVocabType);
                        platformIds = platforms?.Where(p => platformCodes != null && platformCodes.Contains(p.Code)).Select(p => p.Id).Distinct().ToList();
                    }

                    if (platformIds != null && platformIds.Count > 0)
                    {
                        platformIds.ForEach(p => { gameVocabulary.GameVocabularyPlatforms.Add(new GameVocabularyPlatform { PlatformId = p }); });
                    }

                    gameVocabularies.Add(gameVocabulary);
                }
            }

            await _gameVocabularyRepository.ExecuteTransactionAsync(async () =>
            {
                await _gameVocabularyRepository.AddList(gameVocabularies);
                await _gameVocabularyRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            });
            return methodResult;
        }

        private static bool CheckDuplicate(IList<string?> objects, string? value)
        {
            var duplicate = objects.Where(p => p == value).ToList();
            if (duplicate.Count >= 2)
            {
                return true;
            }
            return false;
        }

        private static void AddGameVocabularyType(GameVocabulary gameVocabulary, string gameVocabularyType, string? value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                gameVocabulary.GameVocabularyTypes.Add(new GameVocabularyType { GameVocabType = gameVocabularyType.EnumParse<EnumGameVocabType>(), QuestionContent = value });
            }
        }
    }
}
