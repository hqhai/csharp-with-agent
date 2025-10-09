// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices.Graphs
{
    using Fsel.Course.Domain.Entities.FlowConfigs;
    using Fsel.Course.Domain.Entities.TestConfigs;

    public class Node : IGraphComponent
    {
        public StepFlow StepFlow { get; set; }
        public TestResult? TestResult { get; set; }

        public ICollection<Connection> Connections { get; set; }

        public Node(StepFlow stepFlow)
        {
            StepFlow = stepFlow;
            Connections = new List<Connection>();

            if (stepFlow.ChildActionFlows != null && stepFlow.ChildActionFlows.Any())
            {
                foreach (var stepFlowChild in stepFlow.ChildActionFlows)
                {
                    Connections.Add(new Connection(this, new Node(stepFlowChild.ToStepFlow), stepFlowChild.StartPercent, stepFlowChild.EndPercent));
                }
            }
        }

        public void ConnectTo(Node targetNode, int from, int to)
        {
            Connections.Add(new Connection(this, targetNode, from, to));
        }

        public bool AssignStepResult(TestResult testResult)
        {
            ArgumentNullException.ThrowIfNull(testResult);

            if (testResult.StepFlowId == StepFlow.Id)
            {
                TestResult = testResult;
                return true;
            }

            foreach (var connection in Connections)
            {
                if (connection.Target.AssignStepResult(testResult))
                {
                    return true;
                }
            }

            return false;
        }

        public Node? GetNextNode()
        {
            foreach (var connection in Connections)
            {
                var nextStep = connection.GetNextNode();
                if (nextStep != null)
                {
                    return nextStep;
                }
            }
            return null;
        }

        public static Node CreateStartNode(StepFlow startStep)
        {
            return new Node(startStep);
        }
    }
}
