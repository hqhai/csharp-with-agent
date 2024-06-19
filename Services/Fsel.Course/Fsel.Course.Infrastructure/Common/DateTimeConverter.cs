// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Common
{
    public class DateTimeConverter
    {
        public double SetRemainingTime(double executionTime, double workingTime)
        {
            var remainingTime = executionTime - workingTime;
            return remainingTime > 0 ? remainingTime : default;
        }

        public double SetWorkingTime(double workingTime, double accessTime, double executionTime)
        {
            workingTime += accessTime;
            if (executionTime != default && workingTime >= executionTime)
            {
                workingTime = executionTime;
            }
            return workingTime;
        }
    }
}
