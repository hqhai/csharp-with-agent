// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.KeyboardLayoutQuery
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;

    public class SearchKeyboardLayoutQuery : BaseQueryModel, IRequest<MethodResult<PagingItemsModel<KeyboardLayoutModel>>>
    {
        public Guid? LanguageId { get; set; }
    }

    public class SearchKeyboardLayoutQueryHandler : IRequestHandler<SearchKeyboardLayoutQuery, MethodResult<PagingItemsModel<KeyboardLayoutModel>>>
    {
        private readonly IKeyboardLayoutRepository _keyboardLayoutRepository;

        public SearchKeyboardLayoutQueryHandler(IKeyboardLayoutRepository keyboardLayoutRepository)
        {
            _keyboardLayoutRepository = keyboardLayoutRepository;
        }

        public async Task<MethodResult<PagingItemsModel<KeyboardLayoutModel>>> Handle(SearchKeyboardLayoutQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var querys = _keyboardLayoutRepository.ReadQueryable;

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                querys = querys.Where(x => !string.IsNullOrEmpty(x.Name) && x.Name.Trim() == request.Keyword.Trim());
            }

            if (request.LanguageId.HasValue)
            {
                querys = querys.Where(x => x.LanguageId == request.LanguageId);
            }

            return await _keyboardLayoutRepository.GetListByPageResultAsync<KeyboardLayoutModel>(querys, request, cancellationToken);
        }
    }
}
