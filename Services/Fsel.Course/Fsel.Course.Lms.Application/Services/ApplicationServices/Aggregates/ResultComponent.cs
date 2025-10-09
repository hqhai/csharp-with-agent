// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices.Aggregates
{
    using Fsel.Core.Entities;

    public abstract class ResultComponent
    {
        public Entity Result { get; set; }

        public ResultComponent Parent { get; set; }

        public abstract bool IsBelongTo(Guid id);

        public abstract Task Submit();

        public IServiceProvider ServiceProvider { get; set; }
    }
}
