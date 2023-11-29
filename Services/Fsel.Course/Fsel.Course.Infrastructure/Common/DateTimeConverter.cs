// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Common
{
    using Fsel.Core.Entities;
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

        public static double GetWorkingTime(DateTime inputDate, DateTime outputDate, double executionTime)
        {
            if (executionTime != default)
            {
                var sectionBetweenDate = DateTimeHelper.GetSecondBetweenDate(inputDate, outputDate);
                return sectionBetweenDate <= executionTime ? sectionBetweenDate : executionTime;
            }
            return default;
        }

        public static DateTime GetDateTimeEntity(Entity entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            return entity.UpdatedDate ?? entity.CreatedDate;
        }
    }
}
