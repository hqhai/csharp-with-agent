// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.GameVocabularies
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.System.Application.Services.UserServices;
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
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        public SearchGameVocabularyQueryHandler(IGameVocabularyRepository gameVocabularyRepository, IUserService userService, IMapper mapper)
        {
            _gameVocabularyRepository = gameVocabularyRepository;
            _userService = userService;
            _mapper = mapper;
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

            var query = _gameVocabularyRepository.Queryable.Include(t => t.GameTopic).Include(x => x.GameVocabularyTypes).Select(p => new GameVocabularyModel
            {
                Id = p.Id,
                CreatedDate = p.CreatedDate,
                Code = p.Code,
                Key = p.Key,
                CefrLevel = p.CefrLevel,
                CourseLevel = p.CourseLevel,
                UnitOrder = p.UnitOrder,
                PartSpeech = p.PartSpeech,
                PlatformId = p.PlatformId,
                WordCategoryId = p.WordCategoryId,
                WordCategory = p.GameTopic == null ? null : p.GameTopic.Value,
                GameVocabularyTypes = p.GameVocabularyTypes == null ? null : _mapper.Map<IList<GameVocabularyTypeModel>>(p.GameVocabularyTypes),
            });
            if (!string.IsNullOrEmpty(request.Keyword))
            {
                query = query.Where(p => (p.Key ?? string.Empty).ToLower().Trim().Contains(request.Keyword.ToLower().Trim()));
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
            if (request.PlatformId.HasValue)
            {
                query = query.Where(p => p.PlatformId == request.PlatformId);
            }
            if (request.GameVocabularyIds != null)
            {
                query = query.Where(p => request.GameVocabularyIds.Contains(p.Id));
            }
            int totalItem = await query.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await query
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            var platformsResult = await _userService.GetAllPlatform();
            if (!platformsResult.IsSuccessStatusCode || platformsResult.Content?.Result == null)
            {
                methodResult.AddError(platformsResult.Error);
                return methodResult;
            }
            var platforms = platformsResult.Content.Result;

            lists.ForEach(p =>
            {
                p.PlatformName = platforms.FirstOrDefault(x => x.Id == p.PlatformId)?.Name;
            });

            methodResult.Result = new PagingItemsModel<GameVocabularyModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
