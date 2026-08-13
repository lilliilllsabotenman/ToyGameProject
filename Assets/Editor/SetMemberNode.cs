// Editor/SetMemberNode.cs
using UnityEditor.Experimental.GraphView;
using System;

public class SetMemberNode : BaseNode
{
    public string MemberName;

    public SetMemberNode(string memberName, Type portType) : base(memberName + " を設定")
    {
        MemberName = memberName;

        inputContainer.Add(CreateExecPort(Direction.Input, Port.Capacity.Multi));
        outputContainer.Add(CreateExecPort(Direction.Output, Port.Capacity.Multi));
        inputContainer.Add(CreateDataPort(Direction.Input, "Value", portType ?? typeof(object)));

        RefreshExpandedState();
        RefreshPorts();
    }
}
