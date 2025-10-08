// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices.ContinuePTHandlers
{
    using System;
    using System.Threading.Tasks;
    using Fsel.Course.Domain.Entities.FlowConfigs;
    using Fsel.Course.Domain.Models.EntityModels.PlacementTestModels;

    public interface IBaseContinuePTHandler
    {
        Task Handle(ContinuePTTestContext context);

        IBaseContinuePTHandler SetNex(IBaseContinuePTHandler next);
    }

    public abstract class BaseContinuePTTestHandler : IBaseContinuePTHandler
    {
        public IBaseContinuePTHandler Next { get; set; }

        public abstract Task Handle(ContinuePTTestContext context);

        public virtual IBaseContinuePTHandler SetNex(IBaseContinuePTHandler next)
        {
            Next = next;
            return next;
        }
    }

    public class ContinuePTTestContext
    {
        public Guid StudentId { get; set; }

        public PTStateModel PTState { get; set; }

        public Flow FlowOfPT { get; set; }

        public CurrentState? CurrentState { get; set; }
    }

    public enum CurrentState
    {
        StartNewTest,
        TestInprogress,
        Done,
        NotTestYet
    }
}
