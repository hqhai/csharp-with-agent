// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.QuestBoardQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Ordering.Domain.Enums;
    using Fsel.Shared.Enums;
    using Fsel.System.Application.Services.OrderServices;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using global::System.Linq;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetQuestBoardsActiveByAdminQuery : IRequest<MethodResult<IList<QuestBoardModel>>>
    {
        public EnumQuestBoardType? QuestBoardType { get; set; }
    }

    public class GetQuestBoardsActiveByAdminQueryHandler : IRequestHandler<GetQuestBoardsActiveByAdminQuery, MethodResult<IList<QuestBoardModel>>>
    {
        private readonly IQuestBoardRepository _questBoardRepository;
        private readonly IMapper _mapper;
        private readonly IOrderService _orderService;

        public GetQuestBoardsActiveByAdminQueryHandler(IQuestBoardRepository questBoardRepository, IMapper mapper, IOrderService orderService)
        {
            _questBoardRepository = questBoardRepository;
            _mapper = mapper;
            _orderService = orderService;
        }

        public async Task<MethodResult<IList<QuestBoardModel>>> Handle(GetQuestBoardsActiveByAdminQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<QuestBoardModel>>();

            if (request.QuestBoardType == null)
            {
                methodResult.Result = null;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            var packageResults = await _orderService.GetPackages();
            if (!packageResults.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(packageResults));
                return methodResult;
            }
            var packages = packageResults?.Content?.Result;
            var packageId = packages?.FirstOrDefault(x => x.Code == EnumPackageCode.STANDARD)?.Id;
            var questBoards = await _questBoardRepository.Queryable.Where(x => x.Type == request.QuestBoardType && x.IsActive && x.DependentId == null).ToListAsync(cancellationToken);
            questBoards = questBoards.Where(x => x.PackageIds != null && x.PackageIds!.Any(x => x == packageId)).ToList();
            if (questBoards == null || questBoards.Count == 0)
            {
                methodResult.Result = null;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            methodResult.Result = _mapper.Map<IList<QuestBoardModel>>(questBoards);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
