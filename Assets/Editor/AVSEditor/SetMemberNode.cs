// Editor/SetMemberNode.cs
using UnityEditor.Experimental.GraphView;
using System;
using System.Collections.Generic;

public class SetMemberNode : BaseNode
{
    public string MemberName;

    public SetMemberNode(string memberName, Type portType) : this(memberName, portType, null)
    {
    }

    public SetMemberNode(string memberName, Type portType, List<NodeParamEntry> savedParams) : base(memberName + " を設定")
    {
        MemberName = memberName;

        inputContainer.Add(CreateExecPort(Direction.Input, Port.Capacity.Multi));
        outputContainer.Add(CreateExecPort(Direction.Output, Port.Capacity.Multi));

        string initialValue = savedParams?.Find(e => e.Key == "Value")?.Value;
        AddInput("Value", portType ?? typeof(object), initialValue);

        RefreshExpandedState();
        RefreshPorts();
    }
}
