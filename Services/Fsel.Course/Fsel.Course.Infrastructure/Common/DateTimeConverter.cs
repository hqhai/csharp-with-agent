// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Common
{
    using Fsel.Core.Entities;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Shared.Helpers;

    public class DateTimeConverter
    {
        public double GetWorkingTime(double workingTime, double executionTime, Entity entity)
        {
            if (executionTime != default)
            {
                workingTime += GetWorkingTime(GetDateTimeEntity(entity), DateTime.UtcNow, executionTime);
                if (workingTime >= executionTime)
                {
                    workingTime = executionTime;
                }
            }
            else
            {
                workingTime += DateTimeHelper.GetSecondBetweenDate(GetDateTimeEntity(entity), DateTime.UtcNow);
            }
            return workingTime;
        }

        public double GetWorkingTime(DateTime inputDate, DateTime outputDate, double executionTime)
        {
            var sectionBetweenDate = DateTimeHelper.GetSecondBetweenDate(inputDate, outputDate);
            if (executionTime != default)
            {
                return sectionBetweenDate <= executionTime ? sectionBetweenDate : executionTime;
            }
            return sectionBetweenDate;
        }

        private static DateTime GetDateTimeEntity(Entity entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            return entity.UpdatedDate ?? entity.CreatedDate;
        }

        public double GetRemainingTime(BaseResult baseResult, double executionTime)
        {
            ArgumentNullException.ThrowIfNull(baseResult);
            if (baseResult.Status == EnumResultStatus.Done && baseResult.UpdatedDate.HasValue)
            {
                return executionTime - GetWorkingTime(baseResult.CreatedDate, baseResult.UpdatedDate.Value, executionTime);
            }
            return executionTime - GetWorkingTime(baseResult.CreatedDate, DateTime.UtcNow, executionTime);
        }

        public double GetRemainingTime(double executionTime, double workingTime)
        {
            var remainingTime = executionTime - workingTime;
            return remainingTime > 0 ? remainingTime : default;
        }
    }
}
