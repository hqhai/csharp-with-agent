// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Application.Commands.ZMatterCmd
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Cms.PlanetDefender.Domain.IRepositories;
    using Fsel.Cms.PlanetDefender.Domain.Models.CommandModel;
    using Fsel.Cms.PlanetDefender.Domain.Models.EntityModels;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class UpdateStatusZMatterCommand : UpdateStatusZMatterCommandModel, IRequest<MethodResult<ZMatterModel>>
    {
    }

    public class UpdateStatusZMatterCommandHandler : IRequestHandler<UpdateStatusZMatterCommand, MethodResult<ZMatterModel>>
    {
        private readonly IMapper _mapper;
        private readonly IZMatterRepository _zMatterRepository;

        public UpdateStatusZMatterCommandHandler(IMapper mapper, IZMatterRepository zMatterRepository)
        {
            _mapper = mapper;
            _zMatterRepository = zMatterRepository;
        }

        public async Task<MethodResult<ZMatterModel>> Handle(UpdateStatusZMatterCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<ZMatterModel> methodResult = new MethodResult<ZMatterModel>();
            var zMatter = await _zMatterRepository.GetByIdAsync(request.Id);

            if (zMatter == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(zMatter));
                return methodResult;
            }
            _mapper.Map(request, zMatter);

            await _zMatterRepository.ExecuteTransactionAsync(async () =>
            {
                zMatter = _zMatterRepository.Update(zMatter);

                await _zMatterRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<ZMatterModel>(zMatter);
                return methodResult;
            });

            return methodResult;
        }
    }
}
