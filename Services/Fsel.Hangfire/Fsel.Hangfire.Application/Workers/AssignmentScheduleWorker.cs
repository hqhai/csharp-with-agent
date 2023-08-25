// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base.Interfaces;
using Fsel.Hangfire.Application.Services.TrainingServices;

namespace Fsel.Hangfire.Application.Workers
{
    public class AssignmentScheduleWorker : IWorker
    {
        private readonly ITrainingService _trainingService;

        public AssignmentScheduleWorker(ITrainingService trainingService)
        {
            _trainingService = trainingService;
        }

        public Task RunAsync<T>(T? data = null) where T : class
        {
            return Task.CompletedTask;
        }

        public async Task RunAsync()
        {
            var approveTeacherResult = await _trainingService.ApproveAutoAsync();
        }
    }
}
