// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.FeatureAccessTimeCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
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
        private readonly AuthContext _authContext;

        public SaveFeatureAccessTimeCommandHandler(IMapper mapper, IFeatureAccessTimeRepository featureAccessTimeRepository, AuthContext authContext)
        {
            _mapper = mapper;
            _featureAccessTimeRepository = featureAccessTimeRepository;
            _authContext = authContext;
        }

        public async Task<MethodResult<FeatureAccessTimeModel>> Handle(SaveFeatureAccessTimeCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<FeatureAccessTimeModel> methodResult = new MethodResult<FeatureAccessTimeModel>();

            await _featureAccessTimeRepository.ExecuteTransactionAsync(async () =>
            {
                var featureAccessTime = await _featureAccessTimeRepository.Queryable.OrderByDescending(x => x.LastVisited).FirstOrDefaultAsync(x => x.CreatedUserId == _authContext.CurrentUserId && (x.ObjectId == request.ObjectId || (x.UnitId == null && x.LessonId == null && request.LessonId == null)) && x.EnumFeature == request.EnumFeature, cancellationToken);

                if (featureAccessTime == null)
                {
                    featureAccessTime = AddNewFeatureAccessTime(request);
                }
                else if (featureAccessTime != null && request.LessonId == null && !IsSameRangeHour(featureAccessTime))
                {
                    featureAccessTime = AddNewFeatureAccessTime(request);
                }
                else if (featureAccessTime != null && request.LessonId == null && IsSameRangeHour(featureAccessTime))
                {
                    featureAccessTime = UpdateExistingFeatureAccessTime(featureAccessTime, request);
                }
                else if (featureAccessTime != null)
                {
                    featureAccessTime = UpdateExistingFeatureAccessTime(featureAccessTime, request);
                }
                await _featureAccessTimeRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<FeatureAccessTimeModel>(featureAccessTime);
                return methodResult;
            });

            return methodResult;
        }

        private FeatureAccessTime AddNewFeatureAccessTime(SaveFeatureAccessTimeCommand request)
        {
            var featureAccessTime = _mapper.Map<FeatureAccessTime>(request);
            featureAccessTime.Visit = 1;
            featureAccessTime.LastVisited = DateTime.UtcNow;
            return _featureAccessTimeRepository.Add(featureAccessTime);
        }

        private FeatureAccessTime UpdateExistingFeatureAccessTime(FeatureAccessTime featureAccessTime, SaveFeatureAccessTimeCommand request)
        {
            if (request.AccessTime == null)
            {
                featureAccessTime.Visit += 1;
            }
            featureAccessTime.AccessTime += request.AccessTime ?? default;
            featureAccessTime.LastVisited = DateTime.UtcNow;

            return _featureAccessTimeRepository.Update(featureAccessTime);
        }



        private static bool IsSameRangeHour(FeatureAccessTime featureAccessTime)
        {
            bool isValid = false;
            var now = DateTime.UtcNow;
            var lastVisited = featureAccessTime.LastVisited;

            if (lastVisited!.Value.Year == now.Year
                       && lastVisited!.Value.Month == now.Month
                       && lastVisited!.Value.Day == now.Day
                       && lastVisited!.Value.Hour == now.Hour)
            {
                isValid = true;
            }
            return isValid;
        }

    }
}
