// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Commands.FlagCmd
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.CommandModels.Flags;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class UpdateStatusFlagCommand : UpdateStatusFlagsCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class UpdateStatusFlagCommandHandler : IRequestHandler<UpdateStatusFlagCommand, MethodResult<bool>>
    {
        private readonly IFlagRepository _flagRepository;
        private readonly IMapper _mapper;

        public UpdateStatusFlagCommandHandler(IFlagRepository flagRepository, IMapper mapper)
        {
            _flagRepository = flagRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<bool>> Handle(UpdateStatusFlagCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();
            /*var flags = await _flagRepository.Queryable.Where(x => x.ObjectId == request.ListFlag!.Select(x => x.ObjectId).FirstOrDefault()).ToListAsync(cancellationToken);*/
            var objectIds = request.ListFlag!.Select(x => x.ObjectId).ToList();
            var flags = await _flagRepository.Queryable.Where(x => x.ObjectId.HasValue && objectIds.Contains(x.ObjectId.Value)).ToListAsync(cancellationToken);

            if (request.ListFlag == null || request.ListFlag.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            foreach (var item in request.ListFlag)
            {
                var listFlag = flags.Where(x => x.ObjectId == item.ObjectId).ToList();
                if (listFlag == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(listFlag));
                    return methodResult;
                }
                listFlag = flags.Select(x => _mapper.Map(item, x)).ToList();
            }

            /* _mapper.Map(request, flags);*/

            /* await _flagRepository.ExecuteTransactionAsync(async () =>
             {
                 _flagRepository.UpdateList(flags);

                 await _flagRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                 methodResult.StatusCode = StatusCodes.Status200OK;
                 *//*methodResult.Result = _mapper.Map<FlagModel>(flags);*//*
                 methodResult.Result = _mapper.Map<bool>(flags);
                 return methodResult;
             });

             return methodResult;*/
            _flagRepository.UpdateList(flags);
            await _flagRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
            methodResult.Result = true;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
