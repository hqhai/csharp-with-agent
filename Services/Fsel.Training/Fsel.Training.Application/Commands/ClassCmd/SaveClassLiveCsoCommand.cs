// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Commands.ClassCmd
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Training.Domain.Entities;
    using Fsel.Training.Domain.Enums.ErrorCodes;
    using Fsel.Training.Domain.IRepositories;
    using Fsel.Training.Domain.Models.CommandModels.Classes;
    using Fsel.Training.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SaveClassLiveCsoCommand : SaveClassLiveCsoCommandModel, IRequest<MethodResult<ClassModel>>
    {
    }

    public class SaveClassLiveCsoCommandHandler : IRequestHandler<SaveClassLiveCsoCommand, MethodResult<ClassModel>>
    {
        private readonly IClassRepository _classRepository;
        private readonly IMapper _mapper;

        public SaveClassLiveCsoCommandHandler(IClassRepository classRepository, IMapper mapper)
        {
            _classRepository = classRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<ClassModel>> Handle(SaveClassLiveCsoCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<ClassModel> methodResult = new MethodResult<ClassModel>();

            var @class = await _classRepository.Queryable.Include(x => x.ClassLiveCalendars).FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (@class == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassErrorCode.ClassNotFound));
                return methodResult;
            }
            if (request.AccessLink != null)
            {
                @class.ClassLiveCalendars = request.AccessLink.Select(x => new ClassLiveCalendar
                {
                    AccessLink = x.ToString(),
                }).ToList();
            }
            if (request.Note != null)
            {
                @class.ClassLiveCalendars = request.Note.Select(x => new ClassLiveCalendar
                {
                    Note = x.ToString(),
                }).ToList();
            }
            await _classRepository.ExecuteTransactionAsync(async () =>
            {
                _classRepository.Update(@class);
                await _classRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<ClassModel>(@class);
                return methodResult;
            });

            return methodResult;
        }
    }
}
