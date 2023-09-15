// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.FeatureAccessTimeCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.CommandModels.FeatureAccessTimes;
    using Fsel.System.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SaveFeatureAccessTimeCommand : SaveFeatureAccessTimeCommandModel, IRequest<MethodResult<FeatureAccessTimeModel>>
    {
    }

    public class SaveFeatureAccessTimeCommandHandler : IRequestHandler<SaveFeatureAccessTimeCommand, MethodResult<FeatureAccessTimeModel>>
    {
        private readonly IMapper _mapper;
        private readonly IFeatureAccessTimeRepository _featureAccessTimeRepository;

        public SaveFeatureAccessTimeCommandHandler(IMapper mapper, IFeatureAccessTimeRepository featureAccessTimeRepository)
        {
            _mapper = mapper;
            _featureAccessTimeRepository = featureAccessTimeRepository;
        }

        public async Task<MethodResult<FeatureAccessTimeModel>> Handle(SaveFeatureAccessTimeCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<FeatureAccessTimeModel> methodResult = new MethodResult<FeatureAccessTimeModel>();

            await _featureAccessTimeRepository.ExecuteTransactionAsync(async () =>
            {
                var featureAccessTime = await _featureAccessTimeRepository.Queryable.FirstOrDefaultAsync(x => x.ObjectId == request.ObjectId && x.EnumFeature == request.EnumFeature, cancellationToken);

                if (featureAccessTime == null)
                {
                    featureAccessTime = _mapper.Map<FeatureAccessTime>(request);
                    featureAccessTime.Visit = 1;
                    featureAccessTime.LastVisited = DateTime.Now;
                    featureAccessTime = _featureAccessTimeRepository.Add(featureAccessTime);
                }
                else
                {
                    if (request.AccessTime == 0)
                    {
                        featureAccessTime.Visit += 1;
                    }
                    featureAccessTime.AccessTime += request.AccessTime;
                    featureAccessTime.LastVisited = DateTime.Now;
                    featureAccessTime = _featureAccessTimeRepository.Update(featureAccessTime);
                }
                await _featureAccessTimeRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<FeatureAccessTimeModel>(featureAccessTime);
                return methodResult;
            });

            return methodResult;
        }
    }
}
