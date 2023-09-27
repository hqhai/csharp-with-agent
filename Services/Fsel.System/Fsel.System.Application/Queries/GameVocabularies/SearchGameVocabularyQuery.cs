// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.GameVocabularies
{
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.System.Application.Queries.QuestBoardStudentQuery;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using Fsel.System.Domain.Models.QueryModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchGameVocabularyQuery : SearchGameVocabularyQueryModel, IRequest<MethodResult<PagingItemsModel<GameVocabularyModel>>>
    {
    }

    public class SearchGameVocabularyQueryHandler : IRequestHandler<SearchGameVocabularyQuery, MethodResult<PagingItemsModel<GameVocabularyModel>>>
    {
        private readonly IGameVocabularyRepository _gameVocabularyRepository;

        public SearchGameVocabularyQueryHandler(IGameVocabularyRepository gameVocabularyRepository)
        {
            _gameVocabularyRepository = gameVocabularyRepository;
        }

        public async Task<MethodResult<PagingItemsModel<GameVocabularyModel>>> Handle(SearchGameVocabularyQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<PagingItemsModel<GameVocabularyModel>> methodResult = new MethodResult<PagingItemsModel<GameVocabularyModel>>();
            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }

            var query = _gameVocabularyRepository.Queryable.Include(c => c.GameCenter).Include(t => t.GameTopic).Select(p => new GameVocabularyModel
            {
                Id = p.Id,
                CreatedDate = p.CreatedDate,
                Code = p.Code,
                Key = p.Key,
                CefrLevel = p.CefrLevel,
                CourseLevel = p.CourseLevel,
                UnitOrder = p.UnitOrder,
                AlternateSpelling = p.AlternateSpelling,
                AlternateSpellingStr = p.AlternateSpellingStr,
                UsEquivalent = p.UsEquivalent,
                PartSpeech = p.PartSpeech,
                Definition = p.Definition,
                Hint = p.Hint,
                ExampleSentence = p.ExampleSentence,
                ImagePath = p.ImagePath,
                AudioPath = p.AudioPath,
                Synonym = p.Synonym,
                Antonym = p.Antonym,
                PhoneticTranscription = p.PhoneticTranscription,
                NameOfGame = p.GameCenter == null ? null : p.GameCenter.Name,
                GameCenterId = p.GameCenterId,
                WordCategoryId = p.WordCategoryId,
                WordCategory = p.GameTopic == null ? null : p.GameTopic.Value
            });
            if (!string.IsNullOrEmpty(request.Keyword))
            {
                query = query.Where(p => p.Key!.Contains(request.Keyword));
            }
            if (request.CourseLevel.HasValue)
            {
                query = query.Where(p => p.CourseLevel == request.CourseLevel);
            }
            if (request.UnitOrder.HasValue)
            {
                query = query.Where(p => p.UnitOrder == request.UnitOrder);
            }
            if (request.WordCategoryId.HasValue)
            {
                query = query.Where(p => p.WordCategoryId == request.WordCategoryId);
            }
            if (request.PartSpeech.HasValue)
            {
                query = query.Where(p => p.PartSpeech == request.PartSpeech);
            }
            if (request.GameCenterId.HasValue)
            {
                query = query.Where(p => p.GameCenterId == request.GameCenterId);
            }
            int totalItem = await query.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await query
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            methodResult.Result = new PagingItemsModel<GameVocabularyModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
