// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.QuestBoardCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.System.Application.Services.OrderServices;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.CommandModels.QuestBoards;
    using Fsel.System.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateQuestBoardCommand : CreateQuestBoardCommandModel, IRequest<MethodResult<QuestBoardModel>>
    {
    }

    public class CreateQuestBoardCommandHandler : IRequestHandler<CreateQuestBoardCommand, MethodResult<QuestBoardModel>>
    {
        private readonly IMapper _mapper;
        private readonly IQuestBoardRepository _questBoardRepository;
        private readonly IOrderService _orderService;
        private readonly IQuestBoardConfigRepository _questBoardConfigRepository;

        public CreateQuestBoardCommandHandler(IMapper mapper
            , IQuestBoardRepository questBoardRepository
            , IOrderService orderService
            , IQuestBoardConfigRepository questBoardConfigRepository)
        {
            _mapper = mapper;
            _questBoardRepository = questBoardRepository;
            _orderService = orderService;
            _questBoardConfigRepository = questBoardConfigRepository;
        }

        public async Task<MethodResult<QuestBoardModel>> Handle(CreateQuestBoardCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<QuestBoardModel>();

            #region Validate QuestBoard

            var questBoardConfig = await _questBoardConfigRepository.Queryable.FirstOrDefaultAsync(x => x.Type == request.Type && x.Category == request.Category, cancellationToken);
            if (questBoardConfig == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(questBoardConfig));
                return methodResult;
            }
            var packageResults = await _orderService.GetPackages();
            if (!packageResults.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(packageResults));
                return methodResult;
            }
            var packages = packageResults?.Content?.Result;
            var isCheckPackageIds = request.PackageIds?.All(y => packages?.Any(x => x.Equals(y)) ?? default) ?? default;
            if (!isCheckPackageIds)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(isCheckPackageIds));
                return methodResult;
            }

            if (request.DependentId != null)
            {
                var questBoardDependent = await _questBoardRepository.Queryable.FirstOrDefaultAsync(x => x.Id == request.DependentId, cancellationToken);
                if (questBoardDependent == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(questBoardDependent));
                    return methodResult;
                }
            }

            #endregion Validate QuestBoard

            var questBoard = _mapper.Map<QuestBoard>(request);
            methodResult.StatusCode = StatusCodes.Status201Created;
            return methodResult;
        }
    }
}
