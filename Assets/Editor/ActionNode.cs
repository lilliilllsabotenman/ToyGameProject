// Editor/Nodes/ActionNode.cs
using UnityEditor.Experimental.GraphView;
using System;
using System.Collections.Generic;

public class ActionNode : BaseNode
{
    private string _actionKey;

    public string ActionKey
    {
        get { return _actionKey; }
        set
        {
            _actionKey = value;
            if (!string.IsNullOrEmpty(value)) title = value;
        }
    }

    public ActionNode(string initialMethodName, List<MethodParamInfo> param = null, Type returnType = null, List<NodeParamEntry> savedParams = null) : base("Action")
    {
        inputContainer.Add(CreateExecPort(Direction.Input, Port.Capacity.Multi));
        outputContainer.Add(CreateExecPort(Direction.Output, Port.Capacity.Multi));

        if (param != null)
        {
            foreach (MethodParamInfo p in param)
            {
                string initialValue = savedParams?.Find(e => e.Key == p.Name)?.Value;
                AddInput(p.Name, p.Type, initialValue);
            }
        }

        if (returnType != null && returnType != typeof(void))
        {
            AddOutput("Result", returnType);
        }

        ActionKey = initialMethodName;

        RefreshExpandedState();
        RefreshPorts();
    }
}
