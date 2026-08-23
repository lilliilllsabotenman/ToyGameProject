// Editor/Nodes/ForNode.cs
using UnityEditor.Experimental.GraphView;
using System.Collections.Generic;

public class ForNode : BaseNode
{
    public ForNode() : this(null)
    {
    }

    public ForNode(List<NodeParamEntry> savedParams) : base("繰り返し")
    {
        inputContainer.Add(CreateExecPort(Direction.Input, Port.Capacity.Multi));

        string initialValue = savedParams?.Find(e => e.Key == "Count")?.Value;
        AddInput("Count", typeof(int), initialValue);

        Port bodyPort = CreateExecPort(Direction.Output, Port.Capacity.Multi);
        bodyPort.portName = "Body";
        outputContainer.Add(bodyPort);

        Port completePort = CreateExecPort(Direction.Output, Port.Capacity.Multi);
        completePort.portName = "Complete";
        outputContainer.Add(completePort);

        RefreshExpandedState();
        RefreshPorts();
    }
}
