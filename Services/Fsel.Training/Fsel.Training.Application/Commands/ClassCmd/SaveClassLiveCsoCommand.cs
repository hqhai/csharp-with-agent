// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Commands.ClassCmd
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Training.Domain.IRepositories;
    using Fsel.Training.Domain.Models.CommandModels.Classes;
    using Fsel.Training.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SaveClassLiveCsoCommand : SaveClassLiveCsoCommandModel, IRequest<MethodResult<ClassLiveCalendarModel>>
    {
    }

    public class SaveClassLiveCsoCommandHandler : IRequestHandler<SaveClassLiveCsoCommand, MethodResult<ClassLiveCalendarModel>>
    {
        private readonly IClassLiveCalendarRepository _classLiveCalendarRepository;
        private readonly IMapper _mapper;

        public SaveClassLiveCsoCommandHandler(IClassLiveCalendarRepository classLiveCalendarRepository, IMapper mapper)
        {
            _classLiveCalendarRepository = classLiveCalendarRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<ClassLiveCalendarModel>> Handle(SaveClassLiveCsoCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<ClassLiveCalendarModel> methodResult = new MethodResult<ClassLiveCalendarModel>();

            var classLive = await _classLiveCalendarRepository.Queryable.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (classLive == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(classLive));
                return methodResult;
            }
            _mapper.Map(request, classLive);
            await _classLiveCalendarRepository.ExecuteTransactionAsync(async () =>
            {
                _classLiveCalendarRepository.Update(classLive);
                await _classLiveCalendarRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<ClassLiveCalendarModel>(classLive);
                return methodResult;
            });

            return methodResult;
        }
    }
}
