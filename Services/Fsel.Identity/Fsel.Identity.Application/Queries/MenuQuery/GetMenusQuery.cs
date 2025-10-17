using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Identity.Domain.IRepositories;
using Fsel.Shared.Models.ShareModels;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Identity.Application.Queries.MenuQuery
{
    public class GetMenusQuery : IRequest<MethodResult<IList<MenuModel>>>
    {
    }

    public class GetMenusQueryHandler : IRequestHandler<GetMenusQuery, MethodResult<IList<MenuModel>>>
    {
        private readonly IMenuRepository _menuRepository;
        private readonly IMapper _mapper;

        public GetMenusQueryHandler(IMenuRepository menuRepository, IMapper mapper)
        {
            _menuRepository = menuRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<MenuModel>>> Handle(GetMenusQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<MenuModel>>();
            var menus = await _menuRepository.Queryable.OrderBy(p => p.Index).ToListAsync(cancellationToken);
            methodResult.Result = _mapper.Map<IList<MenuModel>>(menus);
            return methodResult;
        }
    }
}
