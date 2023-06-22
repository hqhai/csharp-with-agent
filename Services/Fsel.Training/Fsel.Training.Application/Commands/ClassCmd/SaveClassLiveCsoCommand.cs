// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Commands.ClassCmd
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Training.Domain.Enums.ErrorCodes;
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
        private readonly IClassLiveCalendarRepository _classLiveCalenderRepository;
        private readonly IMapper _mapper;

        public SaveClassLiveCsoCommandHandler(IClassLiveCalendarRepository classLiveCalenderRepository, IMapper mapper)
        {
            _classLiveCalenderRepository = classLiveCalenderRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<ClassLiveCalendarModel>> Handle(SaveClassLiveCsoCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<ClassLiveCalendarModel> methodResult = new MethodResult<ClassLiveCalendarModel>();

            var classLive = await _classLiveCalenderRepository.Queryable.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (classLive == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassErrorCode.ClassNotFound));
                return methodResult;
            }
            _mapper.Map(request, classLive);
            await _classLiveCalenderRepository.ExecuteTransactionAsync(async () =>
            {
                _classLiveCalenderRepository.Update(classLive);
                await _classLiveCalenderRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<ClassLiveCalendarModel>(classLive);
                return methodResult;
            });

            return methodResult;
        }
    }
}
