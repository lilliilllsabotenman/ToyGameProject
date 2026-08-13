        // Editor/Nodes/ConditionNode.cs
using UnityEditor.Experimental.GraphView;
using System;
using System.Collections.Generic;

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

    public ConditionNode(string initialMethodName, List<MethodParamInfo> param = null, List<NodeParamEntry> savedParams = null, string displayName = null) : base("Condition")
    {
        inputContainer.Add(CreateExecPort(Direction.Input, Port.Capacity.Multi));

        if (param != null)
        {
            foreach (MethodParamInfo p in param)
            {
                string initialValue = savedParams?.Find(e => e.Key == p.Name)?.Value;
                AddInput(p.Name, p.Type, initialValue);
            }
        }

        // ConditionParameterの値ごとに出力ポートを生成
        foreach (ConditionParameter value in Enum.GetValues(typeof(ConditionParameter)))
        {
            Port port = CreateExecPort(Direction.Output, Port.Capacity.Multi);
            port.portName = value.ToString();
            outputContainer.Add(port);
        }

        ConditionKey = initialMethodName;
        if (!string.IsNullOrEmpty(displayName)) title = displayName;

        RefreshExpandedState();
        RefreshPorts();
    }
}
