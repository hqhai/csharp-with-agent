// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.QuestBoardCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Shared.Enums;
    using Fsel.System.Application.Services.OrderServices;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.Enums.ErrorCodes;
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
        private readonly IQuestBoardTaskRepository _questBoardTaskRepository;
        private readonly IQuestBoardConfigRepository _questBoardConfigRepository;

        public CreateQuestBoardCommandHandler(IMapper mapper
            , IQuestBoardRepository questBoardRepository
            , IOrderService orderService
            , IQuestBoardTaskRepository questBoardTaskRepository
            , IQuestBoardConfigRepository questBoardConfigRepository)
        {
            _mapper = mapper;
            _questBoardRepository = questBoardRepository;
            _orderService = orderService;
            _questBoardTaskRepository = questBoardTaskRepository;
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
            var isCheckPackageIds = request.PackageIds?.All(y => packages?.Any(x => x.Id == y) ?? default) ?? default;
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
            var date = request.StartDate.Date.AddHours(request.StartDate.Hour);
            if (date < DateTime.Now.Date.AddHours(DateTime.Now.Hour))
            {
                methodResult.AddErrorBadRequest(nameof(EnumQuestBoardErrorcode.StartDateMustMorethanDateNowPlus1));
                return methodResult;
            }
            if (request.StartDate > request.EndDate)
            {
                methodResult.AddErrorBadRequest(nameof(EnumQuestBoardErrorcode.EndtDateMustMorethanStartDate));
                return methodResult;
            }
            var dependentTasks = new List<QuestBoardTask>();
            if (request.DependentId != null)
            {
                dependentTasks = await _questBoardTaskRepository.Queryable.Where(x => x.QuestBoardId == request.DependentId).ToListAsync(cancellationToken);
                if (dependentTasks == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(questBoardConfig));
                    return methodResult;
                }
            }

            #endregion Validate QuestBoard

            var questBoard = _mapper.Map<QuestBoard>(request);
            if (!questBoard.IsValid())
            {
                methodResult.AddErrorBadRequest(questBoard.ErrorMessages);
                return methodResult;
            }
            questBoard.QuestBoardTasks = await GetDateRangeAsync(request.StartDate, request.EndDate, request.RepeatType, request.DependentId);

            await _questBoardRepository.ExecuteTransactionAsync(async () =>
            {
                questBoard = _questBoardRepository.Add(questBoard);
                await _questBoardRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<QuestBoardModel>(questBoard);
                return methodResult;
            });

            return methodResult;
        }

        private async Task<List<QuestBoardTask>> GetDateRangeAsync(DateTime startDate, DateTime endDate, EnumRepeatType repeatType, Guid? id)
        {
            int repeatInterval = 0;
            switch (repeatType)
            {
                case EnumRepeatType.Day:
                    repeatInterval = 1;
                    break;

                case EnumRepeatType.Week:
                    repeatInterval = 7;
                    break;

                case EnumRepeatType.Month:
                    repeatInterval = 30;
                    break;
            }
            List<QuestBoardTask> questBoardTasks = Enumerable.Range(0, (endDate - startDate).Days / repeatInterval + 1)
               .Select(offset => new QuestBoardTask
               {
                   ImplementDate = startDate.AddDays(offset * repeatInterval),
               })
               .ToList();
            if (id != null)
            {
                var dependentTasks = await _questBoardTaskRepository.Queryable.Where(x => x.Id == id).ToListAsync();
                foreach (var task in questBoardTasks)
                {
                    task.DependentTaskId = dependentTasks.FirstOrDefault(x => x.ImplementDate == task.ImplementDate)?.Id ?? default;
                }
            }
            return questBoardTasks;
        }
    }
}
