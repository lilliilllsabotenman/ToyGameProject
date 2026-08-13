// Editor/Nodes/ActionNode.cs
using UnityEditor.Experimental.GraphView;
using System;
using System.Collections.Generic;

public class GetterNode : BaseNode
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

    public GetterNode(string initialMethodName, List<MethodParamInfo> param = null, Type returnType = null, List<NodeParamEntry> savedParams = null, string displayName = null) : base("Action")
    {
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
        if (!string.IsNullOrEmpty(displayName)) title = displayName;

        RefreshExpandedState();
        RefreshPorts();
    }
}