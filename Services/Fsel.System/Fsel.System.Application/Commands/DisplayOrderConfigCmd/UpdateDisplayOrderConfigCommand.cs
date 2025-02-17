// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.DisplayOrderConfigCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.CommandModels.DisplayOrderConfigs;
    using Fsel.System.Domain.Models.EntityModels;
    using global::System.Collections.Generic;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class UpdateDisplayOrderConfigCommand : UpdateDisplayOrderConfigsCommandModel, IRequest<MethodResult<IList<DisplayOrderConfigModel>>>
    {

    }

    public class UpdateDisplayOrderConfigCommandHandler : IRequestHandler<UpdateDisplayOrderConfigCommand, MethodResult<IList<DisplayOrderConfigModel>>>
    {
        private readonly IDisplayOrderConfigRepository _displayOrderConfigRepository;
        private readonly IMapper _mapper;

        public UpdateDisplayOrderConfigCommandHandler(IDisplayOrderConfigRepository displayOrderConfigRepository, IMapper mapper)
        {
            _displayOrderConfigRepository = displayOrderConfigRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<DisplayOrderConfigModel>>> Handle(UpdateDisplayOrderConfigCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<DisplayOrderConfigModel>> methodResult = new MethodResult<IList<DisplayOrderConfigModel>>();

            if (request.UpdateDisplayOrderConfigs == null || !request.UpdateDisplayOrderConfigs.Any())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request));
                return methodResult;
            }

            var displayOrderConfigs = await _displayOrderConfigRepository.Queryable.ToListAsync(cancellationToken);
            if (displayOrderConfigs == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(displayOrderConfigs));
                return methodResult;
            }

            foreach (var item in displayOrderConfigs)
            {
                var displayOrderConfig = request.UpdateDisplayOrderConfigs.FirstOrDefault(x => x.Id == item.Id);
                if (displayOrderConfig != null)
                {
                    item.DisplayOrder = displayOrderConfig.DisplayOrder;
                    item.Status = displayOrderConfig.Status;
                }
            }

            await _displayOrderConfigRepository.ExecuteTransactionAsync(async () =>
            {
                _displayOrderConfigRepository.UpdateList(displayOrderConfigs);
                await _displayOrderConfigRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<IList<DisplayOrderConfigModel>>(displayOrderConfigs.OrderBy(x => x.DisplayOrder));
                return methodResult;
            });

            return methodResult;
        }
    }
}
