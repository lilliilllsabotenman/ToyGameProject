// Editor/Nodes/ActionNode.cs
using UnityEditor.Experimental.GraphView;
using System;
using System.Collections.Generic;

public class ActionNode : BaseNode, IDataResultNode
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

    public string SourceTypeName;

    // sourceType×methodNameだけで、パラメータ一覧・戻り値型・表示名をNodeMethodOptions経由で自分から取りに行くための短いオーバーロード。
    public ActionNode(Type sourceType, string methodName, List<NodeParamEntry> savedParams = null)
        : this(methodName, NodeMethodOptions.GetMethodParams(sourceType, methodName), NodeMethodOptions.GetReturnType(sourceType, methodName), savedParams, NodeMethodOptions.GetDisplayName<VisualScriptingActionAttribute>(sourceType, methodName), sourceType?.AssemblyQualifiedName)
    {
    }

    public ActionNode(string initialMethodName, List<MethodParamInfo> param = null, Type returnType = null, List<NodeParamEntry> savedParams = null, string displayName = null, string sourceTypeName = null) : base("Action")
    {
        SourceTypeName = sourceTypeName;
        inputContainer.Add(CreateExecPort(Direction.Input, Port.Capacity.Multi));
        outputContainer.Add(CreateExecPort(Direction.Output, Port.Capacity.Multi));

        Type sourceType = string.IsNullOrEmpty(sourceTypeName) ? null : Type.GetType(sourceTypeName);
        if (NodeMethodOptions.RequiresTarget(sourceType, initialMethodName))
        {
            AddInput("Instance", sourceType, null);
        }

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
            AddOutput("returnValue", returnType);
        }

        ActionKey = initialMethodName;
        if (!string.IsNullOrEmpty(displayName)) title = displayName;

        RefreshExpandedState();
        RefreshPorts();
    }
}
