// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices.Aggregates
{
    using Fsel.Core.Entities;
    using Fsel.Course.Domain.Models.EntityModels.PlacementTestModels;
    using Fsel.Shared.Enums;

    public abstract class ResultComponent
    {
        public Entity? Result { get; set; }

        public ResultComponent? Parent { get; set; }

        public abstract bool IsBelongTo(Guid id);

        public abstract BaseTestStateModel ExportState();

        public abstract BaseTestStateModel ExportForTestState();

        public abstract Task Submit(SubmitContext context);

        public abstract Task SubmitTest(SubmitContext context);

        public IServiceProvider ServiceProvider { get; set; }
    }

    public class SubmitContext
    {
        public Guid Id { get; set; }
        public EnumScoringFormulaType? ScoringFormulaType { get; set; }

    }
}
