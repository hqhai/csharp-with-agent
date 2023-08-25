// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Application.Workers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base.Interfaces;

    public class AssignmentScheduleWorker : IWorker
    {
        //private readonly ITrainingService _trainingService;

        //public AssignmentScheduleWorker(ITrainingService trainingService)
        //{
        //    _trainingService = trainingService;
        //}
        public Task RunAsync<T>(T? data = null) where T : class
        {
            return Task.CompletedTask;
        }

        public Task RunAsync()
        {
            return Task.CompletedTask;
            //await _trainingService.ApproveAutoAsync();
        }
    }
}
