// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.FeatureAccessTimeCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Shared.Enums;
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
        List<FeatureAccessTime> _featureAccessTimes = new List<FeatureAccessTime>();
        List<FeatureAccessTime> _featureAccessTimesUpdate = new List<FeatureAccessTime>();

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
                //Phân tách dữ liệu accesstime theo từng khung giờ
                var accessTimes = GetAccessTimeByRangeHour(DateTime.UtcNow, request.AccessTime);


                EnumFeature featureType = ConvertType(request.Type!);
                foreach (var (time, seconds) in accessTimes)
                {

                    var featureAccessTimeCheck = await _featureAccessTimeRepository.Queryable.OrderByDescending(x => x.LastVisited).FirstOrDefaultAsync(x => x.CreatedUserId == request.UserId && (x.ObjectId == request.ObjectId || x.EnumFeature == EnumFeature.Other) && x.EnumFeature == featureType, cancellationToken);

                    if (featureAccessTimeCheck == null || !IsSameRangeHour(featureAccessTimeCheck))
                    {
                        AddNewFeatureAccessTime(request, seconds, time);
                    }
                    else if (featureAccessTimeCheck != null && request.LessonId == null && !IsSameRangeHour(featureAccessTimeCheck))
                    {
                        AddNewFeatureAccessTime(request, seconds, time);
                    }
                    else if (featureAccessTimeCheck != null && IsSameRangeHour(featureAccessTimeCheck))
                    {
                        UpdateExistingFeatureAccessTime(featureAccessTimeCheck, request, seconds);
                    }
                    else if (featureAccessTimeCheck != null)
                    {
                        UpdateExistingFeatureAccessTime(featureAccessTimeCheck, request, seconds);
                    }
                }

                await _featureAccessTimeRepository.AddList(_featureAccessTimes);
                _featureAccessTimeRepository.UpdateList(_featureAccessTimesUpdate);
                await _featureAccessTimeRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                //methodResult.Result = _mapper.Map<FeatureAccessTimeModel>(featureAccessTime);
                return methodResult;
            });

            return methodResult;
        }

        private static EnumFeature ConvertType(string value)
        {
            return Enum.TryParse(value, out EnumFeature result) ? result : EnumFeature.Other;
        }

        private void AddNewFeatureAccessTime(SaveFeatureAccessTimeCommand request, long accessTime, DateTime vistedTime)
        {

            var featureAccessTime = _mapper.Map<FeatureAccessTime>(request);
            featureAccessTime.Visit = 1;
            featureAccessTime.LastVisited = DateTime.UtcNow;
            featureAccessTime.AccessTime = accessTime;
            featureAccessTime.EnumFeature = ConvertType(request.Type!);
            _featureAccessTimes.Add(featureAccessTime);
        }

        private void UpdateExistingFeatureAccessTime(FeatureAccessTime featureAccessTime, SaveFeatureAccessTimeCommand request, long accessTime)
        {
            if (request.AccessTime == null)
            {
                featureAccessTime.Visit += 1;
            }
            featureAccessTime.AccessTime += accessTime;
            featureAccessTime.LastVisited = DateTime.UtcNow;
            ;
            featureAccessTime.EnumFeature = ConvertType(request.Type!);
            _featureAccessTimesUpdate.Add(featureAccessTime);
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



        /// <summary>
        /// Tính toán accesstime theo từng khung giờ
        /// </summary>
        /// <param name="currentTime"></param>
        /// <param name="accessTimeInSeconds"></param>
        /// <returns></returns>
        public static List<(DateTime, long)> GetAccessTimeByRangeHour(DateTime currentTime, long? accessTimeInSeconds)
        {
            List<(DateTime, long)> hourlyAccess = new List<(DateTime, long)>();
            long remainingTime = (long)accessTimeInSeconds!;

            // Tính toán số giây từ currentTime đến đầu giờ hiện tại
            int secondsFromHourStart = currentTime.Minute * 60 + currentTime.Second;

            // Vòng lặp qua các giờ, bắt đầu từ giờ hiện tại và đi ngược về trước
            while (remainingTime > 0)
            {
                long secondsToAllocate;

                if (secondsFromHourStart > 0)
                {
                    // Sử dụng số giây từ đầu giờ đến currentTime cho lần lặp đầu tiên
                    secondsToAllocate = Math.Min(secondsFromHourStart, remainingTime);
                    hourlyAccess.Add((currentTime.AddSeconds(-secondsFromHourStart + secondsToAllocate), secondsToAllocate));
                    secondsFromHourStart = 0; // Đặt lại để các lần lặp sau sử dụng đủ 3600 giây
                }
                else
                {
                    // Mỗi giờ tiếp theo sử dụng đủ 3600 giây hoặc số giây còn lại nếu nhỏ hơn
                    secondsToAllocate = Math.Min(3600, remainingTime);
                    currentTime = currentTime.AddHours(-1); // Lùi về đầu giờ trước
                    hourlyAccess.Add((currentTime, secondsToAllocate));
                }

                remainingTime -= secondsToAllocate;
            }

            hourlyAccess.Reverse(); // Đảo ngược thứ tự để thời gian tăng dần

            return hourlyAccess;
        }
    }
}
