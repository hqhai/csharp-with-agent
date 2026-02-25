// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices.Graphs
{
    using Fsel.Course.Domain.Entities.TestConfigs;

    public class Connection : IGraphComponent
    {
        public Node Source { get; }
        public Node Target { get; }

        public int From { get; set; }

        public int To { get; set; }

        public Connection(Node source, Node target, int from, int to)
        {
            Source = source;
            Target = target;
            From = from;
            To = to;
        }

        public bool AssignStepResult(TestResult testResult)
        {
            return Target.AssignStepResult(testResult);
        }

        public Node? GetNextNode()
        {
            if (Source.TestResult == null)
            {
                return null;
            }

            if (Source.TestResult.PercentModule != 0)
            {
                return Source.TestResult.PercentModule >= From && Source.TestResult.PercentModule <= To ? Target : null;
            }
            else
            {
                return Source.TestResult.Percent >= From && Source.TestResult.Percent <= To ? Target : null;
            }
        }
    }
}
