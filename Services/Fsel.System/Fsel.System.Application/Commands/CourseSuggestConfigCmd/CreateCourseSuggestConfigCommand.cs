// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.CourseSuggestConfigCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Shared.Enums;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.Enums;
    using Fsel.System.Domain.Enums.ErrorCodes;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.CommandModels.CourseSuggestConfigs;
    using Fsel.System.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateCourseSuggestConfigCommand : CreateCourseSuggestConfigCommandModel, IRequest<MethodResult<CourseSuggestConfigModel>>
    {
    }

    public class CreateCourseSuggestConfigCommandHandler : IRequestHandler<CreateCourseSuggestConfigCommand, MethodResult<CourseSuggestConfigModel>>
    {
        private readonly ICourseSuggestConfigRepository _courseSuggestConfigRepository;
        private readonly IMapper _mapper;

        public CreateCourseSuggestConfigCommandHandler(ICourseSuggestConfigRepository courseSuggestConfigRepository,
                                                       IMapper mapper)
        {
            _courseSuggestConfigRepository = courseSuggestConfigRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<CourseSuggestConfigModel>> Handle(CreateCourseSuggestConfigCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<CourseSuggestConfigModel> methodResult = new MethodResult<CourseSuggestConfigModel>();

            #region validate
            if (request.FromAge > request.ToAge)
            {
                methodResult.AddErrorBadRequest(nameof(EnumCourseSuggestConfigErrorCode.ToAgeNotThanFromAge), nameof(request.ToAge), nameof(request.ToAge), nameof(request.FromAge));
                return methodResult;
            }

            if (await _courseSuggestConfigRepository.Queryable.AnyAsync(x => x.Type == request.Type && x.PlacementTestLevel == request.PlacementTestLevel && request.FromAge <= x.ToAge && request.ToAge >= x.FromAge, cancellationToken))
            {
                methodResult.AddErrorBadRequest(nameof(EnumCourseSuggestConfigErrorCode.RecordWithSameTypeAndLevelHasOverlappingAges), nameof(request));
                return methodResult;
            }
            #endregion

            await _courseSuggestConfigRepository.ExecuteTransactionAsync(async () =>
            {
                var command = _mapper.Map<CourseSuggestConfig>(request);

                if (!command.IsValid())
                {
                    methodResult.AddErrorBadRequest(command.ErrorMessages);
                    return methodResult;
                }

                _courseSuggestConfigRepository.Add(command);
                await _courseSuggestConfigRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

                methodResult.Result = _mapper.Map<CourseSuggestConfigModel>(command);
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            });

            return methodResult;
        }
    }
}
