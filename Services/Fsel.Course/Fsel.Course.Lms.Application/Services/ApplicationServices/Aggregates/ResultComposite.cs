// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices.Aggregates
{
    using System;
    using System.Threading.Tasks;
    using Fsel.Course.Domain.Models.EntityModels.PlacementTestModels;
    using Fsel.Shared.Enums;

    public abstract class ResultComposite : ResultComponent
    {
        public string? Name { get; set; }

        public ICollection<ResultComponent> Children { get; set; } = new List<ResultComponent>();

        public abstract void Start();

        public abstract void GenerateChildren();

        public abstract Task LoadTotalScoreData();

        public abstract Task LoadTestHierarchicalData();

        public override IEnumerable<T> GetComponentByType<T>()
        {
            if (this is T t)
            {
                yield return t;
            }

            if (Children != null && Children.Any())
            {
                foreach (var child in Children)
                {
                    foreach (var component in child.GetComponentByType<T>())
                    {
                        yield return component;
                    }
                }
            }
        }

        public virtual IEnumerable<QuestionStateModel> GetPlainQuestionStates()
        {
            foreach (var child in Children.OfType<ResultComposite>().ToList())
            {
                foreach (var item in child.GetPlainQuestionStates())
                {
                    yield return item;
                }
            }
        }

        public override bool IsBelongTo(Guid id)
        {
            if (Result != null && Result.Id == id)
            {
                return true;
            }

            return Children?.Any(c => c.IsBelongTo(id)) ?? false;
        }

        public override async Task Submit(SubmitContext context)
        {
            foreach (var child in Children)
            {
                await child.Submit(context);
            }
        }

        public override async Task SubmitTest(SubmitContext context)
        {
            foreach (var child in Children)
            {
                await child.SubmitTest(context);
            }
        }
    }
}
