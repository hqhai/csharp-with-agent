// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Infrastructure.Common
{
    public static class DateTimeHelper
    {
        public static double SetRemainingTime(double executionTime, double workingTime)
        {
            var remainingTime = executionTime - workingTime;
            return remainingTime > 0 ? remainingTime : default;
        }

        public static double SetWorkingTime(double workingTime, double accessTime, double executionTime)
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
