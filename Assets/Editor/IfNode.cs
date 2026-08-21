// Editor/Nodes/IfNode.cs
using UnityEditor.Experimental.GraphView;
using System.Collections.Generic;

public class IfNode : BaseNode
{
    public IfNode() : this(null)
    {
    }

    public IfNode(List<NodeParamEntry> savedParams) : base("もし〜なら")
    {
        inputContainer.Add(CreateExecPort(Direction.Input, Port.Capacity.Multi));

        string initialValue = savedParams?.Find(e => e.Key == "Condition")?.Value;
        AddInput("Condition", typeof(bool), initialValue);

        Port truePort = CreateExecPort(Direction.Output, Port.Capacity.Multi);
        truePort.portName = "True";
        outputContainer.Add(truePort);

        Port falsePort = CreateExecPort(Direction.Output, Port.Capacity.Multi);
        falsePort.portName = "False";
        outputContainer.Add(falsePort);

        RefreshExpandedState();
        RefreshPorts();
    }
}
