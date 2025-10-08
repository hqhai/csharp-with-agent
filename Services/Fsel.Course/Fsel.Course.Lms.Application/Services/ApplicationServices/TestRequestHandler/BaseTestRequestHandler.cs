// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices.TestRequestHandler
{
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Course.Domain.Models.CommandModels.Tests;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.EntityModels.PlacementTestModels;
    using Fsel.Course.Lms.Application.Services.UserServices.Models;

    public interface IBaseTestRequestHandler
    {
        Task Handle(TestRequestContext context);

        IBaseTestRequestHandler SetNext(IBaseTestRequestHandler next);
    }

    public abstract class BaseTestRequestHandler : IBaseTestRequestHandler
    {
        public IBaseTestRequestHandler Next { get; set; }

        public abstract Task Handle(TestRequestContext context);

        public virtual IBaseTestRequestHandler SetNext(IBaseTestRequestHandler next)
        {
            Next = next;
            return next;
        }
    }

    public class TestRequestContext
    {
        public bool IsSubmit { get; set; }

        public StudentModel Student { get; set; }

        public Guid TestGroupResultId { get; set; }

        public SubmitAnswerCommandModel TestRequestCommand { get; set; }

        public TestSectionResult TestSectionResult { get; set; }

        public bool StartNewModule { get; set; }

        public PTStateModel PTState { get; set; }

        public MethodResult<PTStateModel> MethodResult { get; set; } = new MethodResult<PTStateModel>();
    }
}
