// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Queries.TrainingQuery
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

    public class GetClassByStatusNewQuery : IRequest<MethodResult<IList<TrainingModel>>>
    {
    }

    public class GetClassByStatusNewQueryHandler : IRequestHandler<GetClassByStatusNewQuery, MethodResult<IList<TrainingModel>>>
    {
        private readonly IMapper _mapper;
        private readonly ITrainingRepository _trainingRepository;

        public GetClassByStatusNewQueryHandler(IMapper mapper, ITrainingRepository trainingRepository)
        {
            _mapper = mapper;
            _trainingRepository = trainingRepository;
        }

        public async Task<MethodResult<IList<TrainingModel>>> Handle(GetClassByStatusNewQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<TrainingModel>> methodResult = new MethodResult<IList<TrainingModel>>();

            List<Class> trainings = await _trainingRepository.Queryable.Where(e => e.Status == EnumTrainingType.New)
                                                            .ToListAsync(cancellationToken: cancellationToken);
            if (trainings.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumTrainingErrorCode.TrainingsNotExits));
                return methodResult;
            }
            methodResult.Result = _mapper.Map<IList<TrainingModel>>(trainings);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
