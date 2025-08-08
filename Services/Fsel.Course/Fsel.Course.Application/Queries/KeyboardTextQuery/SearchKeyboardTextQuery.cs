// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.KeyboardTextQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;

    public class SearchKeyboardTextQuery : BaseQueryModel, IRequest<MethodResult<PagingItemsModel<KeyboardTextModel>>>
    {
        public char? Text { get; set; }

        public Guid? KeyboardLayoutId { get; set; }
    }

    public class SearchKeyboardTextQueryHandler : IRequestHandler<SearchKeyboardTextQuery, MethodResult<PagingItemsModel<KeyboardTextModel>>>
    {
        private readonly IKeyboardTextRepository _keyboardTextRepository;

        public SearchKeyboardTextQueryHandler(IKeyboardTextRepository keyboardTextRepository)
        {
            _keyboardTextRepository = keyboardTextRepository;
        }

        public async Task<MethodResult<PagingItemsModel<KeyboardTextModel>>> Handle(SearchKeyboardTextQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var querys = _keyboardTextRepository.ReadQueryable;

            if (request.Text.HasValue)
            {
                int unicode = request.Text.Value;

                querys = querys.Where(x => x.Unicode == unicode);
            }

            if (request.KeyboardLayoutId.HasValue)
            {
                querys = querys.Where(x => x.KeyboardLayoutId == request.KeyboardLayoutId);
            }

            return await _keyboardTextRepository.GetListByPageResultAsync<KeyboardTextModel>(querys, request, cancellationToken);
        }
    }
}
