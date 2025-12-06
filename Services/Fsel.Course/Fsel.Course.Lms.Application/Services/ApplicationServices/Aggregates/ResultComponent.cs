// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices.Aggregates
{
    using Fsel.Core.Entities;
    using Fsel.Course.Domain.Models.EntityModels.PlacementTestModels;

    public abstract class ResultComponent
    {
        public Entity? Result { get; set; }

        public ResultComponent? Parent { get; set; }

        public abstract bool IsBelongTo(Guid id);

        public abstract BaseTestStateModel ExportState();

        public abstract Task Submit(Guid id);

        public IServiceProvider ServiceProvider { get; set; }
    }
}
