// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices.Aggregates
{
    using System;
    using System.Threading.Tasks;

    public abstract class ResultComposite : ResultComponent
    {
        public ICollection<ResultComponent> Children { get; set; } = new List<ResultComponent>();

        public abstract void Start();

        public abstract void GenerateChildren();

        public override bool IsBelongTo(Guid id)
        {
            if (Result.Id == id)
            {
                return true;
            }

            return Children?.Any(c => c.IsBelongTo(id)) ?? false;
        }

        public override async Task Submit()
        {
            if (Children != null)
            {
                foreach (var child in Children)
                {
                    await child.Submit();
                }
            }
        }
    }
}
