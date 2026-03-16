// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Course.Application.Services.SystemServices;
using Fsel.Course.Application.Services.SystemServices.CommandModels;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.CommandModels.Units;
using Fsel.Course.Domain.Models.EntityModels;
using Fsel.Course.Infrastructure.Common.UnitHelper;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Fsel.Course.Application.Commands.UnitCmd
{
    public class CreateUnitCommand : UpdateUnitCommandModel, IRequest<MethodResult<UnitModel>>
    {
        public SaveChatbotConfigCommandModel? ChatbotConfig { get; set; }
    }

    public class CreateUnitCommandHandler : IRequestHandler<CreateUnitCommand, MethodResult<UnitModel>>
    {
        private readonly IUnitRepository _unitRepository;
        private readonly IServiceProvider _serviceProvider;
        private readonly IMapper _mapper;
        private readonly ISystemService _systemService;

        public CreateUnitCommandHandler(IUnitRepository unitRepository,
            IServiceProvider serviceProvider,
            IMapper mapper,
            ISystemService systemService)
        {
            _unitRepository = unitRepository;
            _serviceProvider = serviceProvider;
            _mapper = mapper;
            _systemService = systemService;
        }

        public async Task<MethodResult<UnitModel>> Handle(CreateUnitCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<UnitModel>();

            var unit = UnitFactory.Create(request).Build(version: 0, originalId: Guid.NewGuid());
            if (!await unit.IsValid(_serviceProvider))
            {
                methodResult.AddErrorBadRequest(unit.ErrorMessages);
                return methodResult;
            }

            if (await unit.ValidateDuplicateUnit(_unitRepository).ConfigureAwait(false))
            {
                methodResult.AddErrorBadRequest(unit.ErrorMessages);
                return methodResult;
            }

            await _unitRepository.ExecuteTransactionAsync(async () =>
            {
                unit = _unitRepository.Add(unit);
                await _unitRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                if (request.ChatbotConfig != null && methodResult.Result != null)
                {
                    request.ChatbotConfig.UnitId = methodResult.Result.Id;
                    var chatbotConfigResult = await _systemService.SaveChatBotConfigAsync(request.ChatbotConfig);
                    if (!chatbotConfigResult.IsSuccessStatusCode)
                    {
                        methodResult.AddError(chatbotConfigResult.Error);
                        return methodResult;
                    }
                }

                methodResult.Result = _mapper.Map<UnitModel>(unit);
                methodResult.StatusCode = StatusCodes.Status201Created;
                return methodResult;
            });

            return methodResult;
        }
    }
}
