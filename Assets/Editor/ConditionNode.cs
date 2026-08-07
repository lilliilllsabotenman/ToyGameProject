// Editor/Nodes/ConditionNode.cs
using UnityEditor.Experimental.GraphView;

public class ConditionNode : BaseNode
{
    private string _conditionKey;

    public string ConditionKey
    {
        get { return _conditionKey; }
        set
        {
            _conditionKey = value;
            if (!string.IsNullOrEmpty(value)) title = value;
        }
    }

    public ConditionNode() : this(null)
    {
    }

    public ConditionNode(string initialMethodName) : base("Condition")
    {
        inputContainer.Add(CreateExecPort(Direction.Input, Port.Capacity.Multi));

        // True/Falseの2出力
        Port truePort = CreateExecPort(Direction.Output, Port.Capacity.Multi);
        truePort.portName = "True";
        outputContainer.Add(truePort);

        Port falsePort = CreateExecPort(Direction.Output, Port.Capacity.Multi);
        falsePort.portName = "False";
        outputContainer.Add(falsePort);

        ConditionKey = initialMethodName;

        RefreshExpandedState();
        RefreshPorts();
    }
}
