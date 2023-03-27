// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Queries.ClassQuery
{
    using System.Collections.Generic;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums;
    using Fsel.Training.Doman.Entities;
    using Fsel.Training.Doman.Enums.ErrorCodes;
    using Fsel.Training.Doman.IRepositories;
    using Fsel.Training.Doman.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetClassByStatusNewQuery : IRequest<MethodResult<IList<ClassModel>>>
    {
    }

    public class GetClassByStatusNewQueryHandler : IRequestHandler<GetClassByStatusNewQuery, MethodResult<IList<ClassModel>>>
    {
        private readonly IMapper _mapper;
        private readonly ITrainingRepository _trainingRepository;

        public GetClassByStatusNewQueryHandler(IMapper mapper, ITrainingRepository trainingRepository)
        {
            _mapper = mapper;
            _trainingRepository = trainingRepository;
        }

        public async Task<MethodResult<IList<ClassModel>>> Handle(GetClassByStatusNewQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<ClassModel>> methodResult = new MethodResult<IList<ClassModel>>();

            List<Class> trainings = await _trainingRepository.Queryable.Where(e => e.Status == EnumTrainingType.New)
                                                            .ToListAsync(cancellationToken: cancellationToken);
            if (trainings.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassErrorCode.TrainingsNotExits));
                return methodResult;
            }
            methodResult.Result = _mapper.Map<IList<ClassModel>>(trainings);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
